using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Animaciones del HUD del nivel: contador, barra de progreso, corazones, cartel de
// inicio, destellos de pantalla y las pantallas de pausa, victoria y derrota.
// Todo corre con tiempo sin escala para seguir animandose en pausa.
public class HudJuego : MonoBehaviour
{
    [Header("Contador")]
    public RectTransform icono;
    public TextMeshProUGUI textoContador;
    public Image barraProgreso;

    [Header("Vidas")]
    public Image[] corazones;

    [Header("Avisos")]
    public RectTransform banner;
    public TextMeshProUGUI textoBanner;
    public TextMeshProUGUI subtextoBanner;
    public Image destello;

    [Header("Victoria")]
    public RectTransform tituloVictoria;
    public Image[] estrellas;
    public TextMeshProUGUI textoEstadisticas;
    public TextMeshProUGUI textoRecord;
    public RectTransform[] botonesVictoria;

    [Header("Derrota")]
    public RectTransform tituloDerrota;
    public TextMeshProUGUI textoResumenDerrota;
    public RectTransform[] botonesDerrota;

    private int mostrado;
    private int vidasMostradas;
    private Coroutine cuenta;
    private Coroutine barra;
    private Coroutine flash;

    // -------------------------------------------------------------- inicio

    public void Iniciar(int total, int vidas)
    {
        mostrado = 0;
        vidasMostradas = vidas;
        EscribirContador(0, total);
        if (barraProgreso != null) barraProgreso.fillAmount = 0f;
        PintarCorazones(vidas);
        if (destello != null) destello.color = Paleta.ConAlfa(Color.white, 0f);
        if (banner != null) StartCoroutine(Banner(total, vidas));
    }

    private IEnumerator Banner(int total, int vidas)
    {
        textoBanner.text = "¡A RECOGER CHUPETAS!";
        subtextoBanner.text = total + " chupetas  ·  " + vidas + " vidas  ·  ESC para pausa";
        banner.gameObject.SetActive(true);
        banner.localScale = Vector3.zero;
        yield return Ease.Esperar(0.35f);
        Sonidos.Reproducir(Sonidos.Tipo.Inicio, 0.8f, 0f);
        yield return Ease.Tween(0.35f, t => banner.localScale = Vector3.one * t, Ease.OutBack);
        yield return Ease.Esperar(1.8f);
        yield return Ease.Tween(0.2f, t => banner.localScale = Vector3.one * (1f - t), Ease.OutCubic);
        banner.gameObject.SetActive(false);
    }

    // -------------------------------------------------------------- contador

    public void ActualizarContador(int actual, int total)
    {
        if (cuenta != null) StopCoroutine(cuenta);
        cuenta = StartCoroutine(Contar(actual, total));
        if (barraProgreso != null)
        {
            if (barra != null) StopCoroutine(barra);
            float desde = barraProgreso.fillAmount;
            float hasta = total > 0 ? actual / (float)total : 0f;
            barra = StartCoroutine(Ease.Tween(0.3f, t => barraProgreso.fillAmount = Mathf.Lerp(desde, hasta, t), Ease.OutCubic));
        }
        if (icono != null) StartCoroutine(Pop(icono, 1.35f, 0.25f));
        if (textoContador != null) StartCoroutine(Pop(textoContador.rectTransform, 1.2f, 0.2f));
    }

    private IEnumerator Contar(int hasta, int total)
    {
        int desde = mostrado;
        yield return Ease.Tween(0.25f, t =>
        {
            mostrado = Mathf.RoundToInt(Mathf.Lerp(desde, hasta, t));
            EscribirContador(mostrado, total);
        }, Ease.OutCubic);
    }

    private void EscribirContador(int actual, int total)
    {
        if (textoContador != null) textoContador.text = actual + " / " + total;
    }

    // -------------------------------------------------------------- vidas

    public void ActualizarVidas(int vidas)
    {
        if (corazones == null) return;
        int anterior = vidasMostradas;
        vidasMostradas = vidas;
        PintarCorazones(vidas);

        if (vidas < anterior && anterior - 1 < corazones.Length && anterior - 1 >= 0)
            StartCoroutine(Sacudir(corazones[anterior - 1].rectTransform));
        else if (vidas > anterior && vidas - 1 < corazones.Length && vidas - 1 >= 0)
            StartCoroutine(Pop(corazones[vidas - 1].rectTransform, 1.5f, 0.35f));
    }

    private void PintarCorazones(int vidas)
    {
        if (corazones == null) return;
        for (int i = 0; i < corazones.Length; i++)
            corazones[i].color = i < vidas ? Paleta.Fresa : Paleta.ConAlfa(Paleta.Ciruela, 0.45f);
    }

    // -------------------------------------------------------------- destellos

    public void Destello(Color color, float alfa = 0.5f, float duracion = 0.2f)
    {
        if (destello == null) return;
        if (flash != null) StopCoroutine(flash);
        flash = StartCoroutine(Ease.Tween(duracion, t => destello.color = Paleta.ConAlfa(color, alfa * (1f - t)), Ease.OutCubic));
    }

    // -------------------------------------------------------------- paneles

    public void MostrarPanel(GameObject panel)
    {
        panel.SetActive(true);
        var grupo = Grupo(panel);
        StartCoroutine(Ease.Tween(0.2f, t => grupo.alpha = t, Ease.OutCubic));
        var rt = (RectTransform)panel.transform;
        for (int i = 0; i < rt.childCount; i++)
        {
            var hijo = rt.GetChild(i) as RectTransform;
            if (hijo != null) StartCoroutine(Entrar(hijo, i * 0.05f));
        }
    }

    public void OcultarPanel(GameObject panel)
    {
        if (panel.activeSelf) StartCoroutine(Ocultar(panel));
    }

    private IEnumerator Ocultar(GameObject panel)
    {
        var grupo = Grupo(panel);
        yield return Ease.Tween(0.12f, t => grupo.alpha = 1f - t, Ease.OutCubic);
        panel.SetActive(false);
    }

    private IEnumerator Entrar(RectTransform rt, float retraso)
    {
        rt.localScale = Vector3.zero;
        yield return Ease.Esperar(retraso);
        yield return Ease.Tween(0.3f, t => rt.localScale = Vector3.one * t, Ease.OutBack);
    }

    // -------------------------------------------------------------- victoria

    public void MostrarVictoria(GameObject panel, int total, float tiempo, int vidas, int vidasMax,
        int estrellasGanadas, bool esRecord, float mejorTiempo)
    {
        StartCoroutine(Victoria(panel, total, tiempo, vidas, vidasMax, estrellasGanadas, esRecord, mejorTiempo));
    }

    private IEnumerator Victoria(GameObject panel, int total, float tiempo, int vidas, int vidasMax,
        int estrellasGanadas, bool esRecord, float mejorTiempo)
    {
        Destello(Paleta.Limon, 0.6f, 0.15f);
        Sonidos.Reproducir(Sonidos.Tipo.Fanfarria, 1f, 0f);
        var camara = Camera.main;
        if (camara != null)
        {
            Vector3 centro = camara.transform.position;
            centro.z = 0f;
            Efectos.Confeti(centro, camara.orthographicSize, camara.orthographicSize * camara.aspect);
        }

        panel.SetActive(true);
        var grupo = Grupo(panel);
        foreach (var e in estrellas)
        {
            e.color = Paleta.ConAlfa(Paleta.Ciruela, 0.5f);
            e.rectTransform.localScale = Vector3.zero;
        }
        foreach (var b in botonesVictoria) b.localScale = Vector3.zero;
        textoEstadisticas.text = "";
        textoRecord.text = "";
        tituloVictoria.localScale = Vector3.zero;

        yield return Ease.Tween(0.25f, t => grupo.alpha = t, Ease.OutCubic);
        yield return Ease.Tween(0.35f, t => tituloVictoria.localScale = Vector3.one * t, Ease.OutBack);

        for (int i = 0; i < estrellas.Length; i++)
        {
            var e = estrellas[i];
            StartCoroutine(Ease.Tween(0.25f, t => e.rectTransform.localScale = Vector3.one * t, Ease.OutBack));
            if (i < estrellasGanadas)
            {
                e.color = Paleta.Limon;
                Sonidos.Reproducir(Sonidos.Tipo.Estrella, 1f, 0f, 1f + i * 0.2f);
            }
            yield return Ease.Esperar(0.3f);
        }

        string vidasTexto = vidas + " / " + vidasMax;
        yield return Ease.Tween(0.6f, t =>
        {
            int n = Mathf.RoundToInt(total * t);
            textoEstadisticas.text = "Chupetas  " + n + " / " + total + "\nTiempo  " + Formato(tiempo * t) + "\nVidas  " + vidasTexto;
        }, Ease.OutCubic);

        if (esRecord)
        {
            textoRecord.text = "¡NUEVO RECORD!";
            textoRecord.color = Paleta.Limon;
            StartCoroutine(Pop(textoRecord.rectTransform, 1.4f, 0.4f));
        }
        else
        {
            textoRecord.text = "Record: " + Formato(mejorTiempo);
            textoRecord.color = Paleta.ConAlfa(Paleta.Crema, 0.8f);
        }

        foreach (var b in botonesVictoria)
        {
            StartCoroutine(Ease.Tween(0.3f, t => b.localScale = Vector3.one * t, Ease.OutBack));
            yield return Ease.Esperar(0.08f);
        }
    }

    // -------------------------------------------------------------- derrota

    public void MostrarDerrota(GameObject panel, int recogidas, int total, bool porVacio = false)
    {
        StartCoroutine(Derrota(panel, recogidas, total, porVacio));
    }

    private IEnumerator Derrota(GameObject panel, int recogidas, int total, bool porVacio)
    {
        var titulo = tituloDerrota.GetComponent<TextMeshProUGUI>();
        if (titulo != null) titulo.text = porVacio ? "¡CAISTE AL VACIO!" : "¡TE QUEDASTE SIN VIDAS!";
        Destello(Paleta.Fresa, 0.5f, 0.3f);
        Sonidos.Reproducir(Sonidos.Tipo.Derrota, 1f, 0f);
        panel.SetActive(true);
        var grupo = Grupo(panel);
        foreach (var b in botonesDerrota) b.localScale = Vector3.zero;
        textoResumenDerrota.text = "";

        Vector2 final = tituloDerrota.anchoredPosition;
        Vector2 arriba = final + Vector2.up * 400f;
        tituloDerrota.anchoredPosition = arriba;
        yield return Ease.Tween(0.3f, t => grupo.alpha = t, Ease.OutCubic);
        yield return Ease.Tween(0.5f, t => tituloDerrota.anchoredPosition = Vector2.LerpUnclamped(arriba, final, t), Ease.OutBack);
        StartCoroutine(Sacudir(tituloDerrota));

        textoResumenDerrota.text = "Recogiste " + recogidas + " de " + total + " chupetas.\n¡Tu puedes, intentalo otra vez!";
        foreach (var b in botonesDerrota)
        {
            StartCoroutine(Ease.Tween(0.3f, t => b.localScale = Vector3.one * t, Ease.OutBack));
            yield return Ease.Esperar(0.08f);
        }
    }

    // -------------------------------------------------------------- utilidades

    public static string Formato(float segundos)
    {
        int m = Mathf.FloorToInt(segundos / 60f);
        int s = Mathf.FloorToInt(segundos % 60f);
        return m.ToString("00") + ":" + s.ToString("00");
    }

    private static CanvasGroup Grupo(GameObject panel)
    {
        var g = panel.GetComponent<CanvasGroup>();
        if (g == null) g = panel.AddComponent<CanvasGroup>();
        return g;
    }

    private IEnumerator Pop(RectTransform rt, float escala, float duracion)
    {
        yield return Ease.Tween(duracion, t =>
        {
            float s = t < 0.35f ? Mathf.Lerp(1f, escala, t / 0.35f) : Mathf.LerpUnclamped(escala, 1f, Ease.OutBack((t - 0.35f) / 0.65f));
            rt.localScale = Vector3.one * s;
        }, Ease.Lineal);
        rt.localScale = Vector3.one;
    }

    private IEnumerator Sacudir(RectTransform rt)
    {
        Vector2 base_ = rt.anchoredPosition;
        yield return Ease.Tween(0.35f, t =>
        {
            float fuerza = (1f - t) * 14f;
            rt.anchoredPosition = base_ + new Vector2(Mathf.Sin(t * 60f) * fuerza, 0f);
        }, Ease.Lineal);
        rt.anchoredPosition = base_;
    }
}
