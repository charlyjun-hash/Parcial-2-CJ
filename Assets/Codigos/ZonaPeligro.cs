using UnityEngine;

// Zona de peligro (agua, trampas, etc). Al tocarla el jugador pierde una vida
// y el GameManagerNivel decide si reaparece o si se termino el juego.
[RequireComponent(typeof(Collider2D))]
public class ZonaPeligro : MonoBehaviour
{
    public GameManagerNivel gestor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Personaje")) return;
        if (gestor != null) gestor.QuitarVida();
    }
}
