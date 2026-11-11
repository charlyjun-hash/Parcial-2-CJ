using UnityEngine;

// Corazon que late y flota; al tocarlo devuelve una vida (si no estas al maximo).
public class CorazonVida : MonoBehaviour
{
    private Vector3 posicionBase;
    private Vector3 escalaBase;
    private GameManagerNivel gestor;

    private void Start()
    {
        posicionBase = transform.position;
        escalaBase = transform.localScale;
        gestor = FindAnyObjectByType<GameManagerNivel>();
    }

    private void Update()
    {
        float t = Time.time;
        transform.position = posicionBase + Vector3.up * (Mathf.Sin(t * 2f) * 0.12f);
        // dos golpes rapidos y una pausa, como un latido
        float latido = Mathf.Max(0f, Mathf.Sin(t * 7f)) * (Mathf.Repeat(t, 1.2f) < 0.6f ? 1f : 0f);
        transform.localScale = escalaBase * (1f + latido * 0.12f);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!otro.CompareTag("Personaje") || gestor == null) return;
        if (!gestor.RecuperarVida()) return;

        Efectos.Estallido(transform.position);
        Sonidos.Reproducir(Sonidos.Tipo.Vida, 0.9f, 0f);
        Destroy(gameObject);
    }
}
