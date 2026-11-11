using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Fundido de 0.3 s entre escenas: la pantalla se oscurece, cambia la escena
// y vuelve a aclararse. Usa tiempo sin escala para funcionar aunque haya pausa.
public class Transicion : MonoBehaviour
{
    private const float Duracion = 0.3f;

    private static Transicion instancia;
    private Image cortina;
    private CanvasGroup grupo;
    private bool ocupado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AlIniciarJuego()
    {
        Instancia.StartCoroutine(Instancia.Aclarar());
    }

    private static Transicion Instancia
    {
        get
        {
            if (instancia == null)
            {
                var go = new GameObject("Transicion");
                DontDestroyOnLoad(go);
                instancia = go.AddComponent<Transicion>();
                instancia.Crear();
            }
            return instancia;
        }
    }

    public static void Ir(string escena)
    {
        if (Instancia.ocupado) return;
        Instancia.StartCoroutine(Instancia.Cambiar(escena));
    }

    public static void Salir()
    {
        if (Instancia.ocupado) return;
        Instancia.StartCoroutine(Instancia.Cerrar());
    }

    private void Crear()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        gameObject.AddComponent<GraphicRaycaster>();
        grupo = gameObject.AddComponent<CanvasGroup>();

        var go = new GameObject("Cortina");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        cortina = go.AddComponent<Image>();
        cortina.color = Paleta.Ciruela;
        Poner(1f);
    }

    private void Poner(float alfa)
    {
        grupo.alpha = alfa;
        grupo.blocksRaycasts = alfa > 0.01f;
    }

    private IEnumerator Oscurecer()
    {
        yield return Ease.Tween(Duracion, t => Poner(t), Ease.OutCubic);
    }

    private IEnumerator Aclarar()
    {
        yield return Ease.Tween(Duracion, t => Poner(1f - t), Ease.OutCubic);
    }

    private IEnumerator Cambiar(string escena)
    {
        ocupado = true;
        yield return Oscurecer();
        Time.timeScale = 1f;
        Sonidos.Atenuar(false);
        yield return SceneManager.LoadSceneAsync(escena);
        yield return Aclarar();
        ocupado = false;
    }

    private IEnumerator Cerrar()
    {
        ocupado = true;
        yield return Oscurecer();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
