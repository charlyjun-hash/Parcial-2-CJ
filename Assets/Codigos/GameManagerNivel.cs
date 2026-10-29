using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Director del nivel: conecta el contador de chupetas, las vidas, el cronometro,
// la pausa y las pantallas de victoria / derrota. Todo lo que antes quedaba
// sin conectar (el contador, el menu de pausa, el final del nivel) vive aqui.
public class GameManagerNivel : MonoBehaviour
{
    [Header("Jugador")]
    public ControlPersonaje jugador;
    public float segundosInvulnerable = 1f;
    public AudioSource sonidoDanio;

    [Header("Contador de chupetas")]
    public ContadorChupetas1 contadorChupetas;
    public TextMeshProUGUI textoMeta;

    [Header("Vidas")]
    public TextMeshProUGUI textoVidas;

    [Header("Cronometro")]
    public TextMeshProUGUI textoTiempo;

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    public TextMeshProUGUI textoResumenVictoria;

    private int totalChupetas;
    private float tiempo;
    private bool pausado;
    private bool terminado;
    private bool invulnerable;
    private Vector3 posicionInicial;

    private void Start()
    {
        if (jugador != null) posicionInicial = jugador.transform.position;

        var chupetas = FindObjectsByType<RecolectableChupeta1_Controlador>(FindObjectsInactive.Exclude);
        totalChupetas = chupetas.Length;
        foreach (var c in chupetas)
        {
            c.contador = contadorChupetas;
            if (c.GetComponent<ChupetaAnimada>() == null)
                c.gameObject.AddComponent<ChupetaAnimada>();
        }

        if (contadorChupetas != null) contadorChupetas.IniciarContador();
        ActualizarVidas();

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (textoMeta != null && contadorChupetas != null)
            textoMeta.text = contadorChupetas.contador + " / " + totalChupetas;

        Keyboard teclado = Keyboard.current;
        if (!terminado && teclado != null && teclado.escapeKey.wasPressedThisFrame)
            AlternarPausa();

        if (terminado || pausado) return;

        tiempo += Time.deltaTime;
        if (textoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(tiempo / 60f);
            int segundos = Mathf.FloorToInt(tiempo % 60f);
            textoTiempo.text = minutos.ToString("00") + ":" + segundos.ToString("00");
        }

        if (totalChupetas > 0 && contadorChupetas != null && contadorChupetas.contador >= totalChupetas)
            Victoria();
    }

    public void AlternarPausa()
    {
        if (terminado) return;
        pausado = !pausado;
        Time.timeScale = pausado ? 0f : 1f;
        if (panelPausa != null) panelPausa.SetActive(pausado);
    }

    public void QuitarVida()
    {
        if (terminado || invulnerable || jugador == null) return;

        jugador.vidasTotales--;
        ActualizarVidas();
        if (sonidoDanio != null) sonidoDanio.Play();

        if (jugador.vidasTotales <= 0)
            Derrota();
        else
            StartCoroutine(Reaparecer());
    }

    private IEnumerator Reaparecer()
    {
        invulnerable = true;
        if (jugador != null)
        {
            jugador.transform.position = posicionInicial;
            var rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // parpadeo mientras dura la invulnerabilidad
        var sprite = jugador != null ? jugador.GetComponent<SpriteRenderer>() : null;
        float fin = Time.time + segundosInvulnerable;
        while (Time.time < fin)
        {
            if (sprite != null) sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        if (sprite != null) sprite.enabled = true;
        invulnerable = false;
    }

    private void Victoria()
    {
        terminado = true;
        Time.timeScale = 0f;
        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (textoResumenVictoria != null)
        {
            int minutos = Mathf.FloorToInt(tiempo / 60f);
            int segundos = Mathf.FloorToInt(tiempo % 60f);
            textoResumenVictoria.text = "Recogiste las " + totalChupetas + " chupetas en "
                + minutos.ToString("00") + ":" + segundos.ToString("00");
        }
    }

    private void Derrota()
    {
        terminado = true;
        Time.timeScale = 0f;
        if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    private void ActualizarVidas()
    {
        if (textoVidas == null) return;
        int vidas = jugador != null ? Mathf.Max(0, jugador.vidasTotales) : 0;
        textoVidas.text = "Vidas: " + vidas;
    }
}
