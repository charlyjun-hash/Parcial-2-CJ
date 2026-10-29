using UnityEngine;

// Mueve un elemento del menu en un vaiven suave (y lo gira un poco),
// para que el fondo del inicio se sienta vivo en vez de estatico.
public class DecoracionMenu : MonoBehaviour
{
    public float amplitud = 20f;
    public float velocidad = 1f;
    public float rotacionMaxima = 8f;
    public float desfase = 0f;

    private RectTransform rect;
    private Vector2 posicionInicial;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        posicionInicial = rect.anchoredPosition;
    }

    private void Update()
    {
        float t = Time.unscaledTime * velocidad + desfase;
        rect.anchoredPosition = posicionInicial + new Vector2(0f, Mathf.Sin(t) * amplitud);
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.6f) * rotacionMaxima);
    }
}
