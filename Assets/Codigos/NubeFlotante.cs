using UnityEngine;

// Nube decorativa que avanza despacio y vuelve a aparecer por la izquierda.
// Con "parallax" se mueve ademas una fraccion de lo que se mueve la camara:
// 0 = fija en el mundo (parece cerca), cerca de 1 = casi pegada a la camara (lejos).
[DefaultExecutionOrder(10000)]
public class NubeFlotante : MonoBehaviour
{
    public float velocidad = 0.3f;
    public float limiteIzquierdo = -30f;
    public float limiteDerecho = 55f;
    public float parallax = 0f;

    private Transform camara;
    private Vector3 inicio;
    private Vector3 camaraInicio;
    private float avance;

    private void Start()
    {
        inicio = transform.position;
        if (Camera.main != null)
        {
            camara = Camera.main.transform;
            camaraInicio = camara.position;
        }
    }

    private void LateUpdate()
    {
        avance += velocidad * Time.deltaTime;
        float x = inicio.x + avance;
        float ancho = limiteDerecho - limiteIzquierdo;
        if (x > limiteDerecho)
        {
            avance -= ancho;
            x -= ancho;
        }

        Vector3 d = camara != null ? camara.position - camaraInicio : Vector3.zero;
        transform.position = new Vector3(x + d.x * parallax, inicio.y + d.y * parallax, inicio.z);
    }
}
