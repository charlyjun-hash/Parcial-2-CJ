using System.Collections;
using UnityEngine;

// Le da vida al personaje sin tocar su script de movimiento: se estira al saltar,
// se aplasta al caer, levanta polvo y suena al saltar y aterrizar. Tambien pone
// un tope a la velocidad de subida para que el salto siempre tenga la misma fuerza.
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Rigidbody2D))]
public class VidaJugador : MonoBehaviour
{
    public float caidaParaAterrizaje = 6f;
    public float margenSalto = 0.5f;

    private Rigidbody2D rb;
    private MovimientoJugador movimiento;
    private float impulsoHasta;
    private Vector3 escalaBase;
    private Coroutine animacion;
    private ContactFilter2D filtro;
    private readonly RaycastHit2D[] golpes = new RaycastHit2D[4];
    private bool enSueloAntes = true;
    private float velocidadYAntes;
    private float silencioHasta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movimiento = GetComponent<MovimientoJugador>();
        escalaBase = transform.localScale;
        filtro.useTriggers = false;
    }

    // camas elasticas y pisotones lanzan mas alto que un salto normal a proposito
    public void PermitirImpulso(float segundos = 1.2f)
    {
        impulsoHasta = Time.time + segundos;
        Silenciar(0.3f);
    }

    // un trampolin o un pisoton ya hacen su propio sonido
    public void Silenciar(float segundos = 0.3f)
    {
        silencioHasta = Time.time + segundos;
    }

    private void FixedUpdate()
    {
        float tope = (movimiento != null ? movimiento.fuerzaSalto : 14f) + margenSalto;
        if (Time.time > impulsoHasta && rb.linearVelocity.y > tope)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, tope);

        bool enSuelo = EnSuelo();
        float vy = rb.linearVelocity.y;

        if (enSueloAntes && !enSuelo && vy > 4f)
        {
            if (Time.time > silencioHasta) Sonidos.Reproducir(Sonidos.Tipo.Salto, 0.6f);
            Efectos.Polvo(transform.position, 3);
            Deformar(new Vector3(0.86f, 1.16f, 1f), 0.2f);
        }
        else if (!enSueloAntes && enSuelo && velocidadYAntes < -caidaParaAterrizaje)
        {
            float fuerza = Mathf.InverseLerp(caidaParaAterrizaje, 25f, -velocidadYAntes);
            Sonidos.Reproducir(Sonidos.Tipo.Aterrizaje, 0.4f + fuerza * 0.4f);
            Efectos.Polvo(transform.position, 4 + Mathf.RoundToInt(fuerza * 4f));
            Deformar(new Vector3(1.12f + fuerza * 0.1f, 0.85f - fuerza * 0.08f, 1f), 0.16f);
        }

        enSueloAntes = enSuelo;
        velocidadYAntes = vy;
    }

    private bool EnSuelo()
    {
        int n = rb.Cast(Vector2.down, filtro, golpes, 0.06f);
        for (int i = 0; i < n; i++)
            if (golpes[i].normal.y > 0.5f) return true;
        return false;
    }

    private void Deformar(Vector3 forma, float duracion)
    {
        if (animacion != null) StopCoroutine(animacion);
        animacion = StartCoroutine(Volver(forma, duracion));
    }

    private IEnumerator Volver(Vector3 forma, float duracion)
    {
        Vector3 desde = Vector3.Scale(escalaBase, forma);
        yield return Ease.Tween(duracion, t => transform.localScale = Vector3.LerpUnclamped(desde, escalaBase, t), Ease.OutBack, false);
        transform.localScale = escalaBase;
        animacion = null;
    }
}
