using UnityEngine;

// Hace que una chupeta flote y se balancee suavemente en su sitio.
public class ChupetaAnimada : MonoBehaviour
{
    public float altura = 0.15f;
    public float velocidad = 2.5f;
    public float giro = 12f;

    private Vector3 posicionBase;
    private bool listo;
    private float fase;

    private void Start()
    {
        fase = Random.value * 10f;
    }

    private void Update()
    {
        // la posicion base se toma en el primer Update, despues de que el
        // GeneradorChupetas ya las repartio en su Start
        if (!listo)
        {
            posicionBase = transform.position;
            listo = true;
        }

        float t = Time.time * velocidad + fase;
        transform.position = posicionBase + Vector3.up * (Mathf.Sin(t) * altura);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f) * giro);
    }
}
