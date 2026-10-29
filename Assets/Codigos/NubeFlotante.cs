using UnityEngine;

// Nube decorativa que avanza despacio y vuelve a aparecer por la izquierda.
public class NubeFlotante : MonoBehaviour
{
    public float velocidad = 0.3f;
    public float limiteIzquierdo = -30f;
    public float limiteDerecho = 55f;

    private void Update()
    {
        Vector3 p = transform.position;
        p.x += velocidad * Time.deltaTime;
        if (p.x > limiteDerecho) p.x = limiteIzquierdo;
        transform.position = p;
    }
}
