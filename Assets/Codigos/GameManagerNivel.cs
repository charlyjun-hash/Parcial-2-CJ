using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Director del nivel: chupetas, vidas, cronometro, pausa, danio y el final
// (victoria con estrellas y record, o derrota).
public class GameManagerNivel : MonoBehaviour
{
    private const string ClaveRecord = "MejorTiempoNivel2";

    [Header("Jugador")]
    public ControlPersonaje jugador;
    public float segundosInvulnerable = 1.2f;
    public float fuerzaEmpuje = 7f;
    public float limiteVacio = -9f;
    public AudioSource sonidoDanio;

    [Header("Contador de chupetas")]
    public ContadorChupetas1 contadorChupetas;
    public TextMeshProUGUI textoMeta;

    [Header("Vidas")]
    public TextMeshProUGUI textoVidas;

    [Header("Cronometro")]
    public TextMeshProUGUI textoTiempo;
    public float tiempoTresEstrellas = 300f;

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    public TextMeshProUGUI textoResumenVictoria;

    [Header("Animaciones del HUD")]
    public HudJuego hud;

    private int totalChupetas;
    private int recogidasMostradas;
    private int vidasMax;
    private float tiempo;
    private bool pausado;
    private bool terminado;
    private bool invulnerable;
    private Vector3 posicionInicial;
    private Rigidbody2D cuerpoJugador;
    private SpriteRenderer spriteJugador;
    private CinemachineImpulseSource sacudida;

    private void Start()
    {
        Time.timeScale = 1f;

        if (jugador != null)
        {
            posicionInicial = jugador.transform.position;
            cuerpoJugador = jugador.GetComponent<Rigidbody2D>();
            spriteJugador = jugador.GetComponent<SpriteRenderer>();
            vidasMax = Mathf.Max(1, jugador.vidasTotales);
        }

        var chupetas = FindObjectsByType<RecolectableChupeta1_Controlador>(FindObjectsInactive.Exclude);
        totalChupetas = chupetas.Length;
        foreach (var c in chupetas)
        {
            c.contador = contadorChupetas;
            if (c.GetComponent<ChupetaAnimada>() == null)
                c.gameObject.AddComponent<ChupetaAnimada>();
        }
        if (contadorChupetas != null) contadorChupetas.IniciarContador();

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);
        if (textoVidas != null) textoVidas.gameObject.SetActive(hud == null);

        PrepararSacudida();
        Efectos.IniciarAmbiente(Camera.main);
        Sonidos.TocarMusica();
        Sonidos.CambioVolumen += AplicarVolumenEfectos;
        AplicarVolumenEfectos();

        if (hud != null) hud.Iniciar(totalChupetas, Vidas);
        else ActualizarVidas();
        Debug.Log("[Juego] Nivel iniciado: " + totalChupetas + " chupetas, " + Vidas + " vidas");
    }

    private void OnDestroy()
    {
        Sonidos.CambioVolumen -= AplicarVolumenEfectos;
    }

    private int Vidas => jugador != null ? Mathf.Max(0, jugador.vidasTotales) : 0;

    private void Update()
    {
        int recogidas = contadorChupetas != null ? contadorChupetas.contador : 0;
        if (recogidas != recogidasMostradas)
        {
            recogidasMostradas = recogidas;
            if (hud != null) hud.ActualizarContador(recogidas, totalChupetas);
            Debug.Log("[Juego] Chupeta recogida " + recogidas + "/" + totalChupetas);
        }
        if (hud == null && textoMeta != null) textoMeta.text = recogidas + " / " + totalChupetas;

        Keyboard teclado = Keyboard.current;
        if (!terminado && teclado != null && teclado.escapeKey.wasPressedThisFrame)
            AlternarPausa();

        if (terminado || pausado) return;

        if (jugador != null && jugador.transform.position.y < limiteVacio)
        {
            CaerAlVacio();
            return;
        }

        tiempo += Time.deltaTime;
        if (textoTiempo != null) textoTiempo.text = HudJuego.Formato(tiempo);

        if (totalChupetas > 0 && recogidas >= totalChupetas)
            Victoria();
    }

    // -------------------------------------------------------------- pausa

    public void AlternarPausa()
    {
        if (terminado) return;
        pausado = !pausado;
        Time.timeScale = pausado ? 0f : 1f;
        Sonidos.Atenuar(pausado);
        Sonidos.Reproducir(Sonidos.Tipo.Click, 0.8f);
        if (panelPausa == null) return;
        if (hud == null) panelPausa.SetActive(pausado);
        else if (pausado) hud.MostrarPanel(panelPausa);
        else hud.OcultarPanel(panelPausa);
    }

    // -------------------------------------------------------------- vidas

    // agua: pierde una vida y vuelve al inicio
    public void QuitarVida()
    {
        if (!PerderVida(Paleta.Agua)) return;
        StartCoroutine(Invulnerable(true));
    }

    // gomita: pierde una vida y sale empujado hacia atras
    public void Daniar(Vector2 origen)
    {
        if (!PerderVida(Paleta.Fresa)) return;
        if (cuerpoJugador != null)
        {
            float lado = Mathf.Sign(cuerpoJugador.position.x - origen.x);
            if (lado == 0f) lado = 1f;
            StartCoroutine(Empujar(lado));
        }
        StartCoroutine(Invulnerable(false));
    }

    // caer al vacio no perdona: se pierden todas las vidas de una vez
    private void CaerAlVacio()
    {
        if (terminado || jugador == null) return;
        jugador.vidasTotales = 0;
        ActualizarVidas();
        Sonidos.Reproducir(Sonidos.Tipo.Caida, 1f, 0f);
        if (sacudida != null) sacudida.GenerateImpulseWithForce(1f);
        Debug.Log("[Juego] Cayo al vacio: pierde todas las vidas");
        Derrota(true);
    }

    public bool RecuperarVida()
    {
        if (terminado || jugador == null || jugador.vidasTotales >= vidasMax) return false;
        jugador.vidasTotales++;
        ActualizarVidas();
        if (hud != null) hud.Destello(Paleta.Menta, 0.35f, 0.25f);
        Debug.Log("[Juego] Vida recuperada: " + Vidas);
        return true;
    }

    private bool PerderVida(Color color)
    {
        if (terminado || invulnerable || jugador == null) return false;

        jugador.vidasTotales--;
        ActualizarVidas();
        if (sonidoDanio != null) sonidoDanio.Play();
        if (sacudida != null) sacudida.GenerateImpulseWithForce(0.6f);
        if (hud != null) hud.Destello(color, 0.45f, 0.3f);
        Debug.Log("[Juego] Vida perdida: quedan " + Vidas);

        if (jugador.vidasTotales <= 0)
        {
            Derrota();
            return false;
        }
        return true;
    }

    private void ActualizarVidas()
    {
        if (hud != null) hud.ActualizarVidas(Vidas);
        if (textoVidas != null) textoVidas.text = "Vidas: " + Vidas;
    }

    // el script de movimiento fija la velocidad horizontal cada paso de fisica,
    // asi que el empujon se aplica moviendo la posicion durante un instante
    private IEnumerator Empujar(float lado)
    {
        cuerpoJugador.linearVelocity = new Vector2(cuerpoJugador.linearVelocity.x, 9f);
        for (float t = 0f; t < 0.25f; t += Time.fixedDeltaTime)
        {
            cuerpoJugador.position += new Vector2(lado * fuerzaEmpuje * (1f - t / 0.25f) * Time.fixedDeltaTime, 0f);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator Invulnerable(bool volverAlInicio)
    {
        invulnerable = true;
        if (volverAlInicio && jugador != null)
        {
            yield return new WaitForSeconds(0.25f);
            jugador.transform.position = posicionInicial;
            if (cuerpoJugador != null) cuerpoJugador.linearVelocity = Vector2.zero;
        }

        float fin = Time.time + segundosInvulnerable;
        while (Time.time < fin)
        {
            if (spriteJugador != null) spriteJugador.enabled = !spriteJugador.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        if (spriteJugador != null) spriteJugador.enabled = true;
        invulnerable = false;
    }

    // -------------------------------------------------------------- final

    private void Victoria()
    {
        terminado = true;
        Time.timeScale = 0f;

        int estrellas = 1;
        if (Vidas >= vidasMax) estrellas++;
        if (tiempo <= tiempoTresEstrellas) estrellas++;

        float mejor = PlayerPrefs.GetFloat(ClaveRecord, float.MaxValue);
        bool esRecord = tiempo < mejor;
        if (esRecord)
        {
            PlayerPrefs.SetFloat(ClaveRecord, tiempo);
            PlayerPrefs.SetInt(ClaveRecord + "Estrellas", estrellas);
            mejor = tiempo;
        }
        Debug.Log("[Juego] Victoria: " + estrellas + " estrellas en " + HudJuego.Formato(tiempo) + (esRecord ? " (record)" : ""));

        if (hud != null && panelVictoria != null)
        {
            hud.MostrarVictoria(panelVictoria, totalChupetas, tiempo, Vidas, vidasMax, estrellas, esRecord, mejor);
            return;
        }
        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (textoResumenVictoria != null)
            textoResumenVictoria.text = "Recogiste las " + totalChupetas + " chupetas en " + HudJuego.Formato(tiempo);
    }

    private void Derrota(bool porVacio = false)
    {
        terminado = true;
        Time.timeScale = 0f;
        Debug.Log("[Juego] Derrota");
        if (hud != null && panelDerrota != null) hud.MostrarDerrota(panelDerrota, recogidasMostradas, totalChupetas, porVacio);
        else if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    // -------------------------------------------------------------- sonido y camara

    private void AplicarVolumenEfectos()
    {
        float v = Sonidos.VolumenEfectos;
        if (contadorChupetas != null)
        {
            if (contadorChupetas.sonidoAumentar != null) contadorChupetas.sonidoAumentar.volume = 0.8f * v;
            if (contadorChupetas.sonidoDisminuir != null) contadorChupetas.sonidoDisminuir.volume = 0.8f * v;
        }
        if (sonidoDanio != null) sonidoDanio.volume = 0.8f * v;
    }

    private void PrepararSacudida()
    {
        var camaraVirtual = FindAnyObjectByType<CinemachineCamera>();
        if (camaraVirtual != null && camaraVirtual.GetComponent<CinemachineImpulseListener>() == null)
            camaraVirtual.gameObject.AddComponent<CinemachineImpulseListener>();

        sacudida = GetComponent<CinemachineImpulseSource>();
        if (sacudida == null) sacudida = gameObject.AddComponent<CinemachineImpulseSource>();
        sacudida.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        sacudida.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        sacudida.ImpulseDefinition.ImpulseDuration = 0.25f;
        sacudida.DefaultVelocity = new Vector3(0.4f, -0.8f, 0f);
    }
}
