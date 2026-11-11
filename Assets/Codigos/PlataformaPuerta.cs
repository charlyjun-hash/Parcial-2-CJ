using UnityEngine;

// Plataforma de dos mitades que se abren hacia los lados y se vuelven a cerrar.
// Antes de abrirse parpadea en rojo para avisar. Si estas encima cuando se abre, caes.
public class PlataformaPuerta : MonoBehaviour
{
    public Rigidbody2D izquierda;
    public Rigidbody2D derecha;
    public float apertura = 1.3f;
    public float tiempoCerrada = 2.6f;
    public float tiempoAbierta = 1.3f;
    public float tiempoMovimiento = 0.35f;
    public float aviso = 0.6f;
    public float desfase = 0f;

    private Vector2 baseIzquierda;
    private Vector2 baseDerecha;
    private SpriteRenderer[] cuerpos;
    private Color[] colores;
    private bool abiertaAntes;

    private void Awake()
    {
        baseIzquierda = izquierda.position;
        baseDerecha = derecha.position;
        cuerpos = new[] { izquierda.GetComponent<SpriteRenderer>(), derecha.GetComponent<SpriteRenderer>() };
        colores = new Color[cuerpos.Length];
        for (int i = 0; i < cuerpos.Length; i++) colores[i] = cuerpos[i].color;
    }

    private float Ciclo => tiempoCerrada + tiempoMovimiento * 2f + tiempoAbierta;
    private float Momento => Mathf.Repeat(Time.time + desfase, Ciclo);

    private void FixedUpdate()
    {
        float abierto = Apertura(Momento);
        izquierda.MovePosition(baseIzquierda + Vector2.left * (apertura * abierto));
        derecha.MovePosition(baseDerecha + Vector2.right * (apertura * abierto));

        bool abierta = abierto > 0.01f;
        if (abierta != abiertaAntes && cuerpos[0].isVisible)
            Sonidos.Reproducir(Sonidos.Tipo.Puerta, 0.7f, 0.05f, abierta ? 1f : 1.25f);
        abiertaAntes = abierta;
    }

    private void Update()
    {
        float t = Momento;
        bool avisando = t > tiempoCerrada - aviso && t < tiempoCerrada;
        float parpadeo = avisando ? (Mathf.Sin(Time.time * 30f) + 1f) * 0.5f : 0f;
        for (int i = 0; i < cuerpos.Length; i++)
            cuerpos[i].color = Color.Lerp(colores[i], Paleta.Fresa, parpadeo * 0.8f);
    }

    private float Apertura(float t)
    {
        if (t < tiempoCerrada) return 0f;
        t -= tiempoCerrada;
        if (t < tiempoMovimiento) return Mathf.SmoothStep(0f, 1f, t / tiempoMovimiento);
        t -= tiempoMovimiento;
        if (t < tiempoAbierta) return 1f;
        t -= tiempoAbierta;
        return Mathf.SmoothStep(1f, 0f, t / tiempoMovimiento);
    }
}
