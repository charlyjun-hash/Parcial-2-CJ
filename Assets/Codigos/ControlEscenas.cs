using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlEscenas : MonoBehaviour
{
    private void Awake()
    {
        print("Control de Escenas - Activado");
    }

    public void CargarEscenaInicio()
    {
        SceneManager.LoadScene("inicio");
    }

    public void CargarEscenaNivel()
    {
        SceneManager.LoadScene("nivel");
    }

    public void CargarEscenaNivel2()
    {
        SceneManager.LoadScene("nivel2");
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;    // dentro de Unity detiene el modo Play
#else
        Application.Quit();                                 // en el juego exportado lo cierra
#endif
    }
}
