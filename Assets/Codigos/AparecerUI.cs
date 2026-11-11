using System.Collections;
using UnityEngine;

// Entrada animada de un elemento de la interfaz al aparecer: crece desde cero o
// se desliza desde un lado, con un pequeno rebote al final.
public class AparecerUI : MonoBehaviour
{
    public enum Modo { Escala, DesdeAbajo, DesdeIzquierda, DesdeDerecha }

    public Modo modo = Modo.Escala;
    public float retraso = 0f;
    public float duracion = 0.4f;
    public float distancia = 600f;

    private RectTransform rt;
    private Vector2 posicionFinal;
    private CanvasGroup grupo;

    private void Awake()
    {
        rt = (RectTransform)transform;
        posicionFinal = rt.anchoredPosition;
        grupo = GetComponent<CanvasGroup>();
        if (grupo == null) grupo = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StartCoroutine(Entrar());
    }

    private IEnumerator Entrar()
    {
        Vector2 inicio = posicionFinal + Desplazamiento();
        Poner(0f, inicio);
        yield return Ease.Esperar(retraso);
        yield return Ease.Tween(duracion, t => Poner(t, Vector2.LerpUnclamped(inicio, posicionFinal, t)), Ease.OutBack);
        Poner(1f, posicionFinal);
    }

    private void Poner(float t, Vector2 posicion)
    {
        grupo.alpha = Mathf.Clamp01(t * 2f);
        if (modo == Modo.Escala) rt.localScale = Vector3.one * t;
        else rt.anchoredPosition = posicion;
    }

    private Vector2 Desplazamiento()
    {
        switch (modo)
        {
            case Modo.DesdeAbajo: return Vector2.down * distancia;
            case Modo.DesdeIzquierda: return Vector2.left * distancia;
            case Modo.DesdeDerecha: return Vector2.right * distancia;
            default: return Vector2.zero;
        }
    }
}
