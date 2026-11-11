using UnityEngine;
using UnityEngine.UI;

// Conecta un Slider con el volumen de la musica o de los efectos.
[RequireComponent(typeof(Slider))]
public class ControlVolumen : MonoBehaviour
{
    public bool esMusica = true;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(Cambiar);
    }

    private void OnEnable()
    {
        slider.SetValueWithoutNotify(esMusica ? Sonidos.VolumenMusica : Sonidos.VolumenEfectos);
    }

    private void Cambiar(float valor)
    {
        if (esMusica) Sonidos.VolumenMusica = valor;
        else
        {
            Sonidos.VolumenEfectos = valor;
            Sonidos.Reproducir(Sonidos.Tipo.Hover, 1f, 0f);
        }
    }
}
