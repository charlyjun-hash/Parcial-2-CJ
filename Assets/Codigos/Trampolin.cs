using System.Collections;
using UnityEngine;

// Cama elastica: al caerle encima lanza al jugador muy alto.
public class Trampolin : MonoBehaviour
{
    public float fuerza = 20f;
    public Transform colchon;

    private Vector3 escalaColchon;
    private Coroutine animacion;

    private void Awake()
    {
        if (colchon != null) escalaColchon = colchon.localScale;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Personaje") || c.rigidbody == null) return;

        bool desdeArriba = false;
        for (int i = 0; i < c.contactCount; i++)
            if (c.GetContact(i).normal.y < -0.5f) { desdeArriba = true; break; }
        if (!desdeArriba) return;

        var vida = c.rigidbody.GetComponent<VidaJugador>();
        if (vida != null) vida.PermitirImpulso();
        c.rigidbody.linearVelocity = new Vector2(c.rigidbody.linearVelocity.x, fuerza);

        Sonidos.Reproducir(Sonidos.Tipo.Rebote);
        Efectos.Polvo(c.GetContact(0).point, 6);
        if (colchon != null)
        {
            if (animacion != null) StopCoroutine(animacion);
            animacion = StartCoroutine(Resorte());
        }
    }

    private IEnumerator Resorte()
    {
        Vector3 aplastado = new Vector3(escalaColchon.x * 1.1f, escalaColchon.y * 0.4f, 1f);
        yield return Ease.Tween(0.45f, t => colchon.localScale = Vector3.LerpUnclamped(aplastado, escalaColchon, t), Ease.OutElastic, false);
        animacion = null;
    }
}
