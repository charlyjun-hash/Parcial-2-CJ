using System.Collections.Generic;
using UnityEngine;

// Plataforma que va y viene entre su posicion inicial y esa posicion
// desplazada por "desplazamiento". Lleva consigo a lo que este parado encima,
// porque el jugador fija su propia velocidad y si no se quedaria atras.
[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    public Vector2 desplazamiento = new Vector2(10f, 0f);
    public float velocidad = 1f;
    public float desfase = 0f;

    private Rigidbody2D rb;
    private Vector2 inicio;
    private readonly HashSet<Rigidbody2D> pasajeros = new HashSet<Rigidbody2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        inicio = rb.position;
    }

    private void FixedUpdate()
    {
        float t = (Mathf.Sin(Time.time * velocidad + desfase) + 1f) * 0.5f;
        Vector2 destino = inicio + desplazamiento * t;
        Vector2 delta = destino - rb.position;
        rb.MovePosition(destino);

        foreach (var p in pasajeros)
            if (p != null) p.position += delta;
        pasajeros.Clear();
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (c.rigidbody == null || c.rigidbody.bodyType != RigidbodyType2D.Dynamic) return;
        for (int i = 0; i < c.contactCount; i++)
        {
            // normal hacia abajo = el otro cuerpo esta parado encima de la plataforma
            if (c.GetContact(i).normal.y < -0.5f)
            {
                pasajeros.Add(c.rigidbody);
                break;
            }
        }
    }
}
