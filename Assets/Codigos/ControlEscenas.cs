using UnityEngine;

public class ControlEscenas : MonoBehaviour
{
    private void Awake()
    {
        print("Control de Escenas - Activado");
    }

    public void CargarEscenaInicio()
    {
        Transicion.Ir("inicio");
    }

    public void CargarEscenaNivel()
    {
        Transicion.Ir("nivel");
    }

    public void CargarEscenaNivel2()
    {
        Transicion.Ir("nivel2");
    }

    public void SalirDelJuego()
    {
        Transicion.Salir();     // en el editor detiene el modo Play, en el juego exportado lo cierra
    }
}
