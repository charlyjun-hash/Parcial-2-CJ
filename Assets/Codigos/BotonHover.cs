using UnityEngine;
using UnityEngine.EventSystems;

// Boton que crece al pasar el mouse, suena al pasar y al hacer clic, y puede
// latir suavemente para llamar la atencion (por ejemplo el boton JUGAR).
public class BotonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public float escalaHover = 1.08f;
    public float velocidad = 10f;
    public float pulso = 0f;

    private RectTransform rect;
    private Vector3 escalaNormal;
    private bool encima;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        escalaNormal = rect.localScale;
    }

    private void Update()
    {
        float objetivo = encima ? escalaHover : 1f + Mathf.Sin(Time.unscaledTime * 4f) * pulso;
        rect.localScale = Vector3.Lerp(rect.localScale, escalaNormal * objetivo, Time.unscaledDeltaTime * velocidad);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        encima = true;
        Sonidos.Reproducir(Sonidos.Tipo.Hover, 0.6f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        encima = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Sonidos.Reproducir(Sonidos.Tipo.Click);
    }
}
