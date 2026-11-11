using System;
using System.Collections;
using UnityEngine;

// Curvas de suavizado y un tween simple con corrutina. Por defecto usa tiempo
// sin escala, asi las animaciones de la interfaz siguen andando en pausa.
public static class Ease
{
    public static float OutCubic(float t)
    {
        t = 1f - t;
        return 1f - t * t * t;
    }

    public static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t -= 1f;
        return 1f + c3 * t * t * t + c1 * t * t;
    }

    public static float OutElastic(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        const float c4 = 2f * Mathf.PI / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    public static float Lineal(float t)
    {
        return t;
    }

    public static IEnumerator Tween(float duracion, Action<float> aplicar, Func<float, float> curva, bool sinEscala = true)
    {
        for (float t = 0f; t < 1f; t += (sinEscala ? Time.unscaledDeltaTime : Time.deltaTime) / duracion)
        {
            aplicar(curva(t));
            yield return null;
        }
        aplicar(curva(1f));
    }

    public static IEnumerator Esperar(float segundos, bool sinEscala = true)
    {
        if (sinEscala) yield return new WaitForSecondsRealtime(segundos);
        else yield return new WaitForSeconds(segundos);
    }
}
