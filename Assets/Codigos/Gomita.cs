using System.Collections;
using UnityEngine;

// Enemigo gomita: camina de un lado a otro. Si le saltas encima se aplasta y
// rebotas; si te toca de lado pierdes una vida y te empuja.
[RequireComponent(typeof(Rigidbody2D))]
public class Gomita : MonoBehaviour
{
    public float distancia = 4f;
    public float velocidad = 1.5f;
    public float rebote = 13f;
    public Transform cuerpo;
    public Transform[] pupilas;

    private Rigidbody2D rb;
    private GameManagerNivel gestor;
    private Vector2 inicio;
    private Vector3 escalaCuerpo;
    private Vector3[] posicionPupilas;
    private float tiempo;
    private float direccion = 1f;
    private bool aplastada;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        inicio = rb.position;
        tiempo = Random.value * 3f;
        if (cuerpo != null) escalaCuerpo = cuerpo.localScale;
        posicionPupilas = new Vector3[pupilas.Length];
        for (int i = 0; i < pupilas.Length; i++) posicionPupilas[i] = pupilas[i].localPosition;
    }

    private void Start()
    {
        gestor = FindAnyObjectByType<GameManagerNivel>();
    }

    private void FixedUpdate()
    {
        if (aplastada) return;
        tiempo += Time.fixedDeltaTime;
        float x = Mathf.PingPong(tiempo * velocidad, distancia);
        float nuevaDireccion = Mathf.Sign(Mathf.PingPong((tiempo + 0.01f) * velocidad, distancia) - x);
        if (nuevaDireccion != 0f) direccion = nuevaDireccion;
        rb.MovePosition(new Vector2(inicio.x + x, inicio.y));
    }

    private void Update()
    {
        if (aplastada || cuerpo == null) return;
        float s = Mathf.Sin(Time.time * 10f) * 0.08f;
        cuerpo.localScale = new Vector3(escalaCuerpo.x * (1f + s), escalaCuerpo.y * (1f - s), 1f);
        for (int i = 0; i < pupilas.Length; i++)
            pupilas[i].localPosition = posicionPupilas[i] + Vector3.right * (0.05f * direccion);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        Tocar(otro);
    }

    private void OnTriggerStay2D(Collider2D otro)
    {
        Tocar(otro);
    }

    private void Tocar(Collider2D otro)
    {
        if (aplastada || !otro.CompareTag("Personaje")) return;
        var cuerpoJugador = otro.attachedRigidbody;

        // los pies del jugador por encima del centro de la gomita y bajando = pisoton
        bool desdeArriba = otro.bounds.min.y > transform.position.y && cuerpoJugador != null && cuerpoJugador.linearVelocity.y <= 0.5f;
        if (desdeArriba) Aplastar(cuerpoJugador);
        else if (gestor != null) gestor.Daniar(transform.position);
    }

    private void Aplastar(Rigidbody2D jugador)
    {
        aplastada = true;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

        var vida = jugador.GetComponent<VidaJugador>();
        if (vida != null) vida.PermitirImpulso(0.6f);
        jugador.linearVelocity = new Vector2(jugador.linearVelocity.x, rebote);

        Sonidos.Reproducir(Sonidos.Tipo.Pisoton);
        Efectos.Estallido(transform.position);
        Debug.Log("[Juego] Gomita aplastada");
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        if (cuerpo != null)
        {
            Vector3 plana = new Vector3(escalaCuerpo.x * 1.4f, escalaCuerpo.y * 0.25f, 1f);
            yield return Ease.Tween(0.12f, t => cuerpo.localScale = Vector3.LerpUnclamped(escalaCuerpo, plana, t), Ease.OutCubic, false);
        }
        yield return new WaitForSeconds(0.25f);
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        yield return Ease.Tween(0.2f, t =>
        {
            foreach (var r in renderers) r.color = Paleta.ConAlfa(r.color, 1f - t);
        }, Ease.Lineal, false);
        Destroy(gameObject);
    }
}
