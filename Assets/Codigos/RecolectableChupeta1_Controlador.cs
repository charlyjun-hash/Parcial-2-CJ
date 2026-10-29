using UnityEngine;

public class RecolectableChupeta1_Controlador : MonoBehaviour
{
    public AudioSource SonidoOcio;
    public AudioSource SonidoRecolectado;
    public ContadorChupetas1 contador;      // contador de la interfaz que sube +1 cuando se recoge esta chupeta

    private bool recolectada = false;       // evita que una misma chupeta sume dos veces

    private void Awake()
    {
        if (SonidoOcio != null)
            SonidoOcio.Play();              // reproducir sonido de ocio
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (recolectada) return;

        if (collision.CompareTag("Personaje"))
        {
            recolectada = true;
            EfectoRecoger.Crear(transform.position);

            if (contador != null)
                contador.AumentarContador();    // +1 en la interfaz, el contador emite su sonido
            else
                Debug.LogWarning("La chupeta " + name + " no tiene un contador asignado");

            Destruir(); //>> Ejecutar destruccion
        }
    }

    [ContextMenu("Destruir Chupeta")]
    public void Destruir()
    {
        Destroy(gameObject);                // destruir la chupeta1
    }

    // el +1 no se hace en OnDestroy porque OnDestroy tambien se ejecuta al cambiar de escena
    // o al salir del modo Play, y eso sumaria chupetas que nadie recogio
}
