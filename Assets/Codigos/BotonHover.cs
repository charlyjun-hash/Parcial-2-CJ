using UnityEngine;
using UnityEngine.EventSystems;

// Hace que un boton crezca un poco cuando el mouse pasa por encima
// y vuelva a su tamano normal al salir. Pensado para los botones del menu.
public class BotonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float escalaHover = 1.08f;
    public float velocidad = 10f;

    private RectTransform rect;
    private Vector3 escalaNormal;
    private Vector3 escalaObjetivo;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        escalaNormal = rect.localScale;
        escalaObjetivo = escalaNormal;
    }

    private void Update()
    {
        rect.localScale = Vector3.Lerp(rect.localScale, escalaObjetivo, Time.unscaledDeltaTime * velocidad);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaObjetivo = escalaNormal * escalaHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaObjetivo = escalaNormal;
    }
}
