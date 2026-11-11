using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Pantalla de inicio: la camara recorre despacio el paisaje de fondo, suena la
// musica, se muestra el record y se abre o cierra el panel de opciones.
public class MenuInicio : MonoBehaviour
{
    public Transform camara;
    public float recorrido = 7f;
    public float velocidadCamara = 0.5f;
    public GameObject panelOpciones;
    public TextMeshProUGUI textoRecord;
    public ControlEscenas controlEscenas;

    private Vector3 camaraBase;

    private void Start()
    {
        Time.timeScale = 1f;
        if (camara != null) camaraBase = camara.position;
        if (panelOpciones != null) panelOpciones.SetActive(false);
        Sonidos.TocarMusica();
        Efectos.IniciarAmbiente(Camera.main);

        if (textoRecord != null)
        {
            float mejor = PlayerPrefs.GetFloat("MejorTiempoNivel2", -1f);
            int estrellas = PlayerPrefs.GetInt("MejorTiempoNivel2Estrellas", 0);
            textoRecord.text = mejor > 0f
                ? "Mejor tiempo  " + HudJuego.Formato(mejor) + "   ·   " + estrellas + (estrellas == 1 ? " estrella" : " estrellas")
                : "¡Recoge todas las chupetas para marcar un record!";
        }
    }

    private void Update()
    {
        // recorrido lento de ida y vuelta (unas 0.5 unidades por segundo)
        if (camara != null)
        {
            float t = Mathf.PingPong(Time.unscaledTime * velocidadCamara / recorrido, 1f);
            camara.position = camaraBase + Vector3.right * Mathf.Lerp(-recorrido * 0.5f, recorrido * 0.5f, Mathf.SmoothStep(0f, 1f, t));
        }

        var teclado = Keyboard.current;
        if (teclado == null) return;
        bool opcionesAbiertas = panelOpciones != null && panelOpciones.activeSelf;
        if (opcionesAbiertas && teclado.escapeKey.wasPressedThisFrame) CerrarOpciones();
        else if (!opcionesAbiertas && (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame) && controlEscenas != null)
        {
            Sonidos.Reproducir(Sonidos.Tipo.Click);
            controlEscenas.CargarEscenaNivel2();
        }
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        Sonidos.Reproducir(Sonidos.Tipo.Click);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }
}
