using System;
using UnityEngine;

// Sonidos y musica del juego, sintetizados por codigo (no hacen falta archivos).
// Tambien guarda el volumen de musica y efectos (PlayerPrefs) y se mantiene vivo
// entre escenas, asi la musica no se corta al pasar del menu al nivel.
public class Sonidos : MonoBehaviour
{
    public enum Tipo { Salto, Aterrizaje, Rebote, Pisoton, Vida, Click, Hover, Fanfarria, Derrota, Estrella, Inicio, Caida, Puerta }

    private const int Frecuencia = 22050;
    private const string ClaveMusica = "VolMusica";
    private const string ClaveEfectos = "VolEfectos";
    private const float VolumenBaseMusica = 0.32f;

    private static Sonidos instancia;
    public static event Action CambioVolumen;

    private AudioSource musica;
    private AudioSource[] canales;
    private int siguienteCanal;
    private AudioClip[] clips;
    private bool atenuado;

    public static Sonidos Instancia
    {
        get
        {
            if (instancia == null)
            {
                var go = new GameObject("Sonidos");
                DontDestroyOnLoad(go);
                instancia = go.AddComponent<Sonidos>();
            }
            return instancia;
        }
    }

    public static float VolumenMusica
    {
        get => PlayerPrefs.GetFloat(ClaveMusica, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(ClaveMusica, Mathf.Clamp01(value));
            if (instancia != null) instancia.AplicarVolumenMusica();
            CambioVolumen?.Invoke();
        }
    }

    public static float VolumenEfectos
    {
        get => PlayerPrefs.GetFloat(ClaveEfectos, 0.9f);
        set
        {
            PlayerPrefs.SetFloat(ClaveEfectos, Mathf.Clamp01(value));
            CambioVolumen?.Invoke();
        }
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;

        musica = gameObject.AddComponent<AudioSource>();
        musica.loop = true;
        musica.playOnAwake = false;
        musica.ignoreListenerPause = true;

        canales = new AudioSource[6];
        for (int i = 0; i < canales.Length; i++)
        {
            canales[i] = gameObject.AddComponent<AudioSource>();
            canales[i].playOnAwake = false;
            canales[i].ignoreListenerPause = true;
        }

        clips = new AudioClip[Enum.GetValues(typeof(Tipo)).Length];
        clips[(int)Tipo.Salto] = Barrido("Salto", 0.12f, 420f, 760f, Onda.Seno, 0.35f);
        clips[(int)Tipo.Aterrizaje] = Golpe("Aterrizaje", 0.07f, 0.4f);
        clips[(int)Tipo.Rebote] = Boing();
        clips[(int)Tipo.Pisoton] = Barrido("Pisoton", 0.13f, 320f, 110f, Onda.Cuadrada, 0.35f);
        clips[(int)Tipo.Vida] = Notas("Vida", new[] { 660f, 990f, 1320f }, 0.09f, Onda.Seno, 0.4f);
        clips[(int)Tipo.Click] = Golpe("Click", 0.025f, 0.35f, 1400f);
        clips[(int)Tipo.Hover] = Barrido("Hover", 0.03f, 1500f, 1700f, Onda.Seno, 0.12f);
        clips[(int)Tipo.Fanfarria] = Notas("Fanfarria", new[] { 523f, 659f, 784f, 1047f }, 0.13f, Onda.Triangular, 0.45f, 0.5f);
        clips[(int)Tipo.Derrota] = Notas("Derrota", new[] { 392f, 330f, 262f }, 0.28f, Onda.Triangular, 0.4f, 0.3f);
        clips[(int)Tipo.Estrella] = Notas("Estrella", new[] { 1047f, 1568f }, 0.08f, Onda.Seno, 0.4f, 0.15f);
        clips[(int)Tipo.Inicio] = Notas("Inicio", new[] { 523f, 784f }, 0.1f, Onda.Triangular, 0.35f, 0.2f);
        clips[(int)Tipo.Caida] = Barrido("Caida", 0.7f, 950f, 180f, Onda.Seno, 0.4f);
        clips[(int)Tipo.Puerta] = Barrido("Puerta", 0.12f, 300f, 170f, Onda.Cuadrada, 0.18f);

        musica.clip = CrearMusica();
        AplicarVolumenMusica();
    }

    // -------------------------------------------------------------- uso

    public static void Reproducir(Tipo tipo, float volumen = 1f, float variacionTono = 0.08f, float tono = 1f)
    {
        Instancia.ReproducirClip(Instancia.clips[(int)tipo], volumen, variacionTono, tono);
    }

    public void ReproducirClip(AudioClip clip, float volumen = 1f, float variacionTono = 0.08f, float tono = 1f)
    {
        if (clip == null) return;
        var canal = canales[siguienteCanal];
        siguienteCanal = (siguienteCanal + 1) % canales.Length;
        canal.pitch = tono * (1f + UnityEngine.Random.Range(-variacionTono, variacionTono));
        canal.volume = volumen * VolumenEfectos;
        canal.clip = clip;
        canal.Play();
    }

    public static void TocarMusica()
    {
        var s = Instancia;
        if (!s.musica.isPlaying) s.musica.Play();
    }

    // en pausa la musica baja a la mitad (unos -6 dB)
    public static void Atenuar(bool valor)
    {
        Instancia.atenuado = valor;
        Instancia.AplicarVolumenMusica();
    }

    private void AplicarVolumenMusica()
    {
        musica.volume = VolumenBaseMusica * VolumenMusica * (atenuado ? 0.5f : 1f);
    }

    // -------------------------------------------------------------- sintesis

    private enum Onda { Seno, Cuadrada, Triangular }

    private static float Muestra(Onda onda, float fase)
    {
        switch (onda)
        {
            case Onda.Cuadrada: return Mathf.Sin(fase) >= 0f ? 0.5f : -0.5f;
            case Onda.Triangular: return Mathf.PingPong(fase / Mathf.PI, 2f) - 1f;
            default: return Mathf.Sin(fase);
        }
    }

    // suaviza 5 ms al inicio y al final para que no suene un "clic"
    private static float Bordes(int i, int total)
    {
        int borde = Frecuencia / 200;
        return Mathf.Clamp01(Mathf.Min(i, total - 1 - i) / (float)borde);
    }

    private static AudioClip Clip(string nombre, float[] datos)
    {
        var clip = AudioClip.Create(nombre, datos.Length, 1, Frecuencia, false);
        clip.SetData(datos, 0);
        return clip;
    }

    private static AudioClip Barrido(string nombre, float duracion, float desde, float hasta, Onda onda, float volumen)
    {
        int n = Mathf.CeilToInt(duracion * Frecuencia);
        var d = new float[n];
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            fase += 2f * Mathf.PI * Mathf.Lerp(desde, hasta, t) / Frecuencia;
            d[i] = Muestra(onda, fase) * volumen * (1f - t) * Bordes(i, n);
        }
        return Clip(nombre, d);
    }

    private static AudioClip Golpe(string nombre, float duracion, float volumen, float filtro = 300f)
    {
        int n = Mathf.CeilToInt(duracion * Frecuencia);
        var d = new float[n];
        var azar = new System.Random(7);
        float previo = 0f;
        float k = Mathf.Clamp01(2f * Mathf.PI * filtro / Frecuencia);
        for (int i = 0; i < n; i++)
        {
            float ruido = (float)(azar.NextDouble() * 2.0 - 1.0);
            previo += (ruido - previo) * k;
            float t = i / (float)n;
            d[i] = previo * volumen * 3f * (1f - t) * (1f - t) * Bordes(i, n);
        }
        return Clip(nombre, d);
    }

    private static AudioClip Boing()
    {
        int n = Mathf.CeilToInt(0.28f * Frecuencia);
        var d = new float[n];
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float f = Mathf.Lerp(180f, 620f, Ease.OutCubic(t)) * (1f + 0.06f * Mathf.Sin(t * 60f));
            fase += 2f * Mathf.PI * f / Frecuencia;
            d[i] = Mathf.Sin(fase) * 0.45f * (1f - t) * Bordes(i, n);
        }
        return Clip("Rebote", d);
    }

    private static AudioClip Notas(string nombre, float[] notas, float duracionNota, Onda onda, float volumen, float colaFinal = 0f)
    {
        int porNota = Mathf.CeilToInt(duracionNota * Frecuencia);
        int cola = Mathf.CeilToInt(colaFinal * Frecuencia);
        int n = porNota * notas.Length + cola;
        var d = new float[n];
        for (int k = 0; k < notas.Length; k++)
        {
            int inicio = k * porNota;
            int largo = k == notas.Length - 1 ? porNota + cola : porNota + porNota / 3;
            for (int i = 0; i < largo && inicio + i < n; i++)
            {
                float t = i / (float)largo;
                float fase = 2f * Mathf.PI * notas[k] * i / Frecuencia;
                d[inicio + i] += Muestra(onda, fase) * volumen * Mathf.Pow(1f - t, 2f);
            }
        }
        for (int i = 0; i < n; i++) d[i] *= Bordes(i, n);
        return Clip(nombre, d);
    }

    // Musica alegre en bucle: bajo, arpegio y melodia sobre C - G - Am - F.
    private static AudioClip CrearMusica()
    {
        const float bpm = 112f;
        float pulso = 60f / bpm;
        int compases = 8;
        int total = Mathf.CeilToInt(compases * 4 * pulso * Frecuencia);
        var d = new float[total];

        float[][] acordes =
        {
            new[] { 261.6f, 329.6f, 392.0f }, // C
            new[] { 196.0f, 246.9f, 293.7f }, // G
            new[] { 220.0f, 261.6f, 329.6f }, // Am
            new[] { 174.6f, 220.0f, 261.6f }, // F
            new[] { 261.6f, 329.6f, 392.0f }, // C
            new[] { 196.0f, 246.9f, 293.7f }, // G
            new[] { 174.6f, 220.0f, 261.6f }, // F
            new[] { 196.0f, 246.9f, 293.7f }  // G
        };
        float[] melodia =
        {
            659.3f, 784.0f, 659.3f, 523.3f,
            587.3f, 784.0f, 493.9f, 587.3f,
            523.3f, 659.3f, 880.0f, 659.3f,
            440.0f, 523.3f, 698.5f, 523.3f,
            784.0f, 659.3f, 523.3f, 659.3f,
            493.9f, 587.3f, 784.0f, 587.3f,
            440.0f, 523.3f, 587.3f, 523.3f,
            493.9f, 587.3f, 784.0f, 0f
        };

        for (int c = 0; c < compases; c++)
        {
            float[] acorde = acordes[c];
            for (int b = 0; b < 4; b++)
            {
                float inicio = (c * 4 + b) * pulso;

                // bajo en los tiempos 1 y 3
                if (b % 2 == 0) Nota(d, inicio, pulso * 1.8f, acorde[0] * 0.5f, 0.22f, Onda.Seno, 3f);

                // arpegio en corcheas
                for (int k = 0; k < 2; k++)
                {
                    float f = acorde[(b * 2 + k) % 3] * 2f;
                    Nota(d, inicio + k * pulso * 0.5f, pulso * 0.45f, f, 0.07f, Onda.Triangular, 7f);
                }

                // melodia en negras
                float m = melodia[c * 4 + b];
                if (m > 0f) Nota(d, inicio, pulso * 0.9f, m, 0.09f, Onda.Seno, 2.5f, true);

                // platillo suave a contratiempo
                Ruido(d, inicio + pulso * 0.5f, 0.04f, 0.025f);
            }
        }

        float pico = 0f;
        foreach (float v in d) pico = Mathf.Max(pico, Mathf.Abs(v));
        if (pico > 0f) for (int i = 0; i < d.Length; i++) d[i] *= 0.7f / pico;
        return Clip("Musica", d);
    }

    private static void Nota(float[] d, float inicio, float duracion, float frecuencia, float volumen, Onda onda, float caida, bool vibrato = false)
    {
        int a = Mathf.FloorToInt(inicio * Frecuencia);
        int n = Mathf.CeilToInt(duracion * Frecuencia);
        for (int i = 0; i < n && a + i < d.Length; i++)
        {
            float t = i / (float)Frecuencia;
            float f = vibrato ? frecuencia * (1f + 0.004f * Mathf.Sin(t * 30f)) : frecuencia;
            float env = Mathf.Exp(-t * caida) * Mathf.Clamp01(t * 200f) * Mathf.Clamp01((n - i) / (Frecuencia * 0.01f));
            d[a + i] += Muestra(onda, 2f * Mathf.PI * f * t) * volumen * env;
        }
    }

    private static void Ruido(float[] d, float inicio, float duracion, float volumen)
    {
        int a = Mathf.FloorToInt(inicio * Frecuencia);
        int n = Mathf.CeilToInt(duracion * Frecuencia);
        var azar = new System.Random(a);
        float previo = 0f;
        for (int i = 0; i < n && a + i < d.Length; i++)
        {
            float r = (float)(azar.NextDouble() * 2.0 - 1.0);
            float agudo = r - previo;
            previo = r;
            d[a + i] += agudo * volumen * (1f - i / (float)n);
        }
    }
}
