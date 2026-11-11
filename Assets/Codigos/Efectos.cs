using UnityEngine;

// Efectos de particulas del juego. Cada efecto es UN sistema de particulas que se
// reutiliza con Emit(), asi no se crean ni destruyen objetos en cada evento.
public class Efectos : MonoBehaviour
{
    private static Efectos instancia;
    private static Material material;

    private ParticleSystem estallido;
    private ParticleSystem polvo;
    private ParticleSystem salpicadura;
    private ParticleSystem confeti;
    private ParticleSystem destellosAmbiente;
    private int capa;

    private static Efectos Instancia
    {
        get
        {
            if (instancia == null)
            {
                instancia = new GameObject("Efectos").AddComponent<Efectos>();
                instancia.Crear();
            }
            return instancia;
        }
    }

    private void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    private void Crear()
    {
        capa = SortingLayer.NameToID("UI 3");
        if (!SortingLayer.IsValid(capa)) capa = 0;
        int capaArriba = SortingLayer.NameToID("Bloqueo");
        if (!SortingLayer.IsValid(capaArriba)) capaArriba = capa;

        estallido = Sistema("Estallido", 0.35f, 0.6f, 0.6f, false, capa, 40);
        polvo = Sistema("Polvo", 0.25f, 0.4f, -0.2f, false, capa, 30);
        salpicadura = Sistema("Salpicadura", 0.4f, 0.75f, 2.2f, false, capa, 40);
        confeti = Sistema("Confeti", 2.5f, 3.5f, 0.3f, true, capaArriba, 150);
    }

    // -------------------------------------------------------------- efectos

    public static void Estallido(Vector3 posicion)
    {
        var e = Instancia;
        for (int i = 0; i < 12; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            e.Emitir(e.estallido, posicion, dir * Random.Range(2f, 3.5f),
                Paleta.Dulces[i % Paleta.Dulces.Length], Random.Range(0.18f, 0.32f));
        }
    }

    public static void Polvo(Vector3 posicion, int cantidad = 5)
    {
        var e = Instancia;
        Color c = Color.Lerp(Paleta.Crema, Paleta.Chocolate, 0.25f);
        for (int i = 0; i < cantidad; i++)
        {
            float lado = i % 2 == 0 ? -1f : 1f;
            var vel = new Vector2(lado * Random.Range(0.8f, 2f), Random.Range(0.2f, 0.9f));
            e.Emitir(e.polvo, posicion + Vector3.right * lado * 0.2f, vel, Paleta.ConAlfa(c, 0.85f), Random.Range(0.25f, 0.4f));
        }
    }

    public static void Salpicadura(Vector3 posicion)
    {
        var e = Instancia;
        for (int i = 0; i < 18; i++)
        {
            var vel = new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(4f, 7.5f));
            Color c = i % 3 == 0 ? Paleta.Crema : Paleta.Agua;
            e.Emitir(e.salpicadura, posicion, vel, c, Random.Range(0.15f, 0.3f));
        }
    }

    // lluvia de confeti desde arriba de la pantalla y dos chorros desde las esquinas de abajo
    public static void Confeti(Vector3 centro, float medioAlto, float medioAncho)
    {
        var e = Instancia;
        for (int i = 0; i < 70; i++)
        {
            var pos = centro + new Vector3(Random.Range(-medioAncho, medioAncho), medioAlto + Random.Range(0f, 1.5f), 0f);
            var vel = new Vector2(Random.Range(-1f, 1f), Random.Range(-2.5f, -0.5f));
            e.Emitir(e.confeti, pos, vel, Paleta.Dulces[i % 4], Random.Range(0.35f, 0.6f), Random.Range(2.5f, 3.5f));
        }
        for (int lado = -1; lado <= 1; lado += 2)
        {
            var esquina = centro + new Vector3(lado * medioAncho * 0.9f, -medioAlto, 0f);
            for (int i = 0; i < 30; i++)
            {
                float angulo = (lado * -25f + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
                var vel = new Vector2(Mathf.Sin(angulo), Mathf.Cos(angulo)) * Random.Range(9f, 14f);
                e.Emitir(e.confeti, esquina, vel, Paleta.Dulces[i % 4], Random.Range(0.35f, 0.6f), Random.Range(2f, 3f));
            }
        }
    }

    // brillitos que flotan siempre alrededor de la camara
    public static void IniciarAmbiente(Camera camara)
    {
        var e = Instancia;
        if (e.destellosAmbiente != null || camara == null) return;

        var ps = e.Sistema("Destellos Ambiente", 4f, 6f, 0f, false, 0, 60);
        ps.transform.SetParent(camara.transform, false);
        ps.transform.localPosition = new Vector3(0f, 0f, -camara.transform.position.z);
        ps.GetComponent<ParticleSystemRenderer>().sortingOrder = -3;

        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Paleta.ConAlfa(Paleta.Crema, 0.7f), Paleta.ConAlfa(Paleta.Rosa, 0.6f));
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);

        var emision = ps.emission;
        emision.enabled = true;
        emision.rateOverTime = 6f;

        var forma = ps.shape;
        forma.enabled = true;
        forma.shapeType = ParticleSystemShapeType.Box;
        float alto = camara.orthographicSize * 2f + 4f;
        forma.scale = new Vector3(alto * camara.aspect + 4f, alto, 0.1f);

        // los tres ejes en el mismo modo, si no la consola se llena de errores
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
        vel.y = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var colorVida = ps.colorOverLifetime;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        colorVida.color = grad;

        e.destellosAmbiente = ps;
        ps.Play();
    }

    // -------------------------------------------------------------- utilidades

    private void Emitir(ParticleSystem ps, Vector3 posicion, Vector2 velocidad, Color color, float tamano, float vida = -1f)
    {
        var p = new ParticleSystem.EmitParams
        {
            position = new Vector3(posicion.x, posicion.y, 0f),
            velocity = velocidad,
            startColor = color,
            startSize = tamano,
            applyShapeToPosition = false
        };
        if (vida > 0f) p.startLifetime = vida;
        ps.Emit(p, 1);
    }

    private ParticleSystem Sistema(string nombre, float vidaMin, float vidaMax, float gravedad, bool sinEscala, int capaOrden, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(vidaMin, vidaMax);
        main.startSpeed = 0f;
        main.gravityModifier = gravedad;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;
        main.useUnscaledTime = sinEscala;

        var emision = ps.emission;
        emision.enabled = false;
        var forma = ps.shape;
        forma.enabled = false;

        // se encogen hasta cero en vez de desaparecer de golpe
        var tamanoVida = ps.sizeOverLifetime;
        tamanoVida.enabled = true;
        tamanoVida.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var rotacion = ps.rotationOverLifetime;
        rotacion.enabled = sinEscala;
        rotacion.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.material = Material();
        r.sortingLayerID = capaOrden;
        r.sortingOrder = orden;

        ps.Play();
        return ps;
    }

    private static Material Material()
    {
        if (material != null) return material;
        var shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        material = new Material(shader) { name = "Particulas Dulces" };

        const int lado = 32;
        var tex = new Texture2D(lado, lado) { name = "Punto Suave", wrapMode = TextureWrapMode.Clamp };
        float radio = lado / 2f;
        for (int y = 0; y < lado; y++)
            for (int x = 0; x < lado; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radio, radio));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radio - d)));
            }
        tex.Apply();
        material.mainTexture = tex;
        return material;
    }
}
