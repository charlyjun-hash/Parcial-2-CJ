using UnityEngine;

// Fondo con profundidad: se mueve una fraccion de lo que se mueve la camara.
// factor 1 = viaja pegado a la camara (muy lejos), 0 = fijo en el mundo (cerca).
[DefaultExecutionOrder(10000)]
public class Parallax : MonoBehaviour
{
    public float factor = 0.5f;
    public float factorVertical = 0.3f;

    private Transform camara;
    private Vector3 posicionBase;
    private Vector3 camaraBase;

    private void Start()
    {
        if (Camera.main == null) return;
        camara = Camera.main.transform;
        posicionBase = transform.position;
        camaraBase = camara.position;
    }

    private void LateUpdate()
    {
        if (camara == null) return;
        Vector3 d = camara.position - camaraBase;
        transform.position = posicionBase + new Vector3(d.x * factor, d.y * factorVertical, 0f);
    }
}
