using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class ContadorChupetas1 : MonoBehaviour
{
    public int contador = 0;
    public TextMeshProUGUI Texto_Contador_TMPro;
    public AudioSource sonidoAumentar;
    public AudioSource sonidoDisminuir;

    [ContextMenu("Aumentar Contador")]
    public void AumentarContador()
    {
        contador++;
        Texto_Contador_TMPro.text = contador.ToString();
        sonidoAumentar.Play();
    }

    public void AumentarContador(int _cantidad)
    {
        contador += _cantidad;
        Texto_Contador_TMPro.text = contador.ToString();
        sonidoAumentar.Play();
    }

    [ContextMenu("Disminuir Contador")]
    public void DisminuirContador()
    {
        contador--;
        Texto_Contador_TMPro.text = contador.ToString();
        sonidoDisminuir.Play();
    }
    public void DisminuirContador(int _cantidad)
    {
        contador -= _cantidad;
        Texto_Contador_TMPro.text = contador.ToString();
        sonidoDisminuir.Play();
    }

    [ContextMenu("Iniciar Contador")]
    public void IniciarContador()
    {
        contador = 0;
        Texto_Contador_TMPro.text = contador.ToString();
    }

    public void IniciarContador(int _cantidad)
    {

    }

    public void AlIniciarScript()
    {

    }

    public void AlActivarScript()
    {

    }

    public void AlDesactivarScript()
    {

    }
}
