using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Menu "Lollipops > Pulir juego completo": arma en nivel2 e inicio todo lo nuevo
// (interfaz animada, enemigos, camas elasticas, corazones, fondos con profundidad,
// opciones de volumen). Se puede ejecutar varias veces: borra y recrea lo suyo.
public static class PulidoJuego
{
    private const string RutaNivel2 = "Assets/escenas/nivel2.unity";
    private const string RutaInicio = "Assets/escenas/inicio.unity";
    private const string CarpetaSprites = "Assets/imagenes/Generadas";
    private const string RutaChupeta = "Assets/Prefabricados/Recolectable _ Chupeta1.prefab";
    private const string GuidFuente = "8f586378b4e144a9851e7b34d9b748ee";
    private const string GuidCuadrado = "311925a002f4447b3a28927169b83ea6";
    private const long IdCuadrado = 7482667652216324306;
    private const string GuidCirculo = "a86470a33a6bf42c4b3595704624658b";
    private const long IdCirculo = -2413806693520163455;
    private const string GuidCielo = "3833381965e49bb4c93845276ab67b96";
    private const long IdCielo = -2167245536030590550;

    private const string NombreInteractivos = "Interactivos (nivel 2)";
    private const string NombreFondoInicio = "Fondo Animado (inicio)";
    private const string NombreCielo = "Cielo (inicio)";

    private static Sprite corazon, estrella, cuadrado, circulo, cielo, chupeta;
    private static Sprite uiSprite, uiFondo, uiPerilla;
    private static TMP_FontAsset fuente;

    [MenuItem("Lollipops/Pulir juego completo")]
    public static void Pulir()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        PrepararRecursos();
        var registro = new List<string>();

        var escena = EditorSceneManager.OpenScene(RutaNivel2);
        PulirNivel2(registro);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        escena = EditorSceneManager.OpenScene(RutaInicio);
        PulirInicio(registro);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        EditorSceneManager.OpenScene(RutaNivel2);
        Debug.Log("Juego pulido y guardado:\n- " + string.Join("\n- ", registro));
    }

    // ================================================================ recursos

    private static void PrepararRecursos()
    {
        fuente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(GuidFuente));
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        uiFondo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        uiPerilla = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        cuadrado = CargarSprite(GuidCuadrado, IdCuadrado);
        circulo = CargarSprite(GuidCirculo, IdCirculo);
        cielo = CargarSprite(GuidCielo, IdCielo);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaChupeta);
        chupeta = prefab != null ? prefab.GetComponent<SpriteRenderer>().sprite : null;
        corazon = GenerarSprite("corazon", DentroCorazon);
        estrella = GenerarSprite("estrella", DentroEstrella);
    }

    private static Sprite CargarSprite(string guid, long id)
    {
        string ruta = AssetDatabase.GUIDToAssetPath(guid);
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ruta))
            if (o is Sprite s && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out _, out long local) && local == id)
                return s;
        return null;
    }

    private static Sprite GenerarSprite(string nombre, System.Func<float, float, bool> dentro)
    {
        const int lado = 128;
        const int sub = 4;
        string ruta = CarpetaSprites + "/" + nombre + ".png";
        if (!File.Exists(ruta))
        {
            Directory.CreateDirectory(CarpetaSprites);
            var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            for (int y = 0; y < lado; y++)
                for (int x = 0; x < lado; x++)
                {
                    int cuenta = 0;
                    for (int sy = 0; sy < sub; sy++)
                        for (int sx = 0; sx < sub; sx++)
                            if (dentro((x + (sx + 0.5f) / sub) / lado, (y + (sy + 0.5f) / sub) / lado)) cuenta++;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, cuenta / (float)(sub * sub)));
                }
            File.WriteAllBytes(ruta, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(ruta);
        }

        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        if (imp.textureType != TextureImporterType.Sprite || !Mathf.Approximately(imp.spritePixelsPerUnit, lado))
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.spritePixelsPerUnit = lado;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    private static bool DentroCorazon(float u, float v)
    {
        float x = (u - 0.5f) * 2.6f;
        float y = (v - 0.42f) * 2.6f;
        float a = x * x + y * y - 1f;
        return a * a * a - x * x * y * y * y <= 0f;
    }

    private static bool DentroEstrella(float u, float v)
    {
        var p = new Vector2(u - 0.5f, v - 0.47f);
        var puntos = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float ang = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float r = i % 2 == 0 ? 0.48f : 0.2f;
            puntos[i] = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
        }
        bool dentro = false;
        for (int i = 0, j = 9; i < 10; j = i++)
            if ((puntos[i].y > p.y) != (puntos[j].y > p.y) &&
                p.x < (puntos[j].x - puntos[i].x) * (p.y - puntos[i].y) / (puntos[j].y - puntos[i].y) + puntos[i].x)
                dentro = !dentro;
        return dentro;
    }

    // ================================================================ nivel 2

    private static void PulirNivel2(List<string> registro)
    {
        var gm = Object.FindAnyObjectByType<GameManagerNivel>(FindObjectsInactive.Include);
        var canvasGO = GameObject.Find("Canvas HUD");
        if (gm == null || canvasGO == null)
        {
            registro.Add("ERROR: no se encontro el Game Manager o el Canvas HUD en nivel2");
            return;
        }

        CrearInteractivos(registro);
        AjustarLimitesCamara();
        gm.tiempoTresEstrellas = 420f;
        gm.limiteVacio = -9f;

        if (gm.jugador != null && gm.jugador.GetComponent<VidaJugador>() == null)
            gm.jugador.gameObject.AddComponent<VidaJugador>();
        registro.Add("Jugador: se estira al saltar, se aplasta al caer y levanta polvo");

        AjustarCamara();
        registro.Add("Camara: zona muerta 0.1, anticipacion 0.5 s, suavizado amplio en X y firme en Y");

        var hud = ArmarHud(canvasGO, gm);
        gm.hud = hud;
        if (gm.textoVidas != null) gm.textoVidas.gameObject.SetActive(false);
        EditorUtility.SetDirty(gm);
        registro.Add("HUD: corazones, barra de progreso, cartel de inicio y destellos; capa 'Bloqueo' para quedar encima de todo");
        registro.Add("Pausa con volumen; victoria con estrellas y record; derrota animada");
    }

    private static void CrearInteractivos(List<string> registro)
    {
        var viejo = GameObject.Find(NombreInteractivos);
        if (viejo != null) Object.DestroyImmediate(viejo);
        var raiz = new GameObject(NombreInteractivos).transform;

        const float suelo = -3.13f;
        Trampolin(raiz, 1, new Vector2(11.6f, suelo));
        Trampolin(raiz, 2, new Vector2(17f, suelo));
        Trampolin(raiz, 3, new Vector2(72.8f, 1.2f));

        Gomita(raiz, 1, new Vector2(0.5f, suelo), 5f, Paleta.Menta);
        Gomita(raiz, 2, new Vector2(52f, 0.9f), 4.5f, Paleta.Rosa);
        Gomita(raiz, 3, new Vector2(89.2f, 1f), 4.6f, Paleta.Lila);
        Gomita(raiz, 4, new Vector2(105.6f, 2f), 4f, Paleta.Menta);

        Corazon(raiz, 1, new Vector2(-1.55f, 15f));
        Corazon(raiz, 2, new Vector2(94f, 3.2f));

        var camara = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        Colinas(raiz, "Colinas Lejanas", 0.85f, Color.Lerp(Paleta.Lila, Paleta.Crema, 0.5f), -9, 7f, 10f, camara.y - 13f, 11f, -70f, 240f, 3);
        Colinas(raiz, "Colinas Cercanas", 0.6f, Color.Lerp(Paleta.Rosa, Paleta.Crema, 0.55f), -8, 5f, 8f, camara.y - 11f, 9f, -70f, 240f, 5);

        registro.Add("Interactivos: 3 camas elasticas, 4 gomitas enemigas, 2 corazones y colinas con profundidad");

        CrearSeccion3(raiz, registro);
        NubesParallax(raiz);
        registro.Add("Nubes: 2 capas nuevas (lejanas y cercanas) mas las existentes, todas con parallax");
    }

    // ================================================================ seccion 3

    private const float FinNivel = 176f;

    private static void CrearSeccion3(Transform raiz, List<string> registro)
    {
        // el muro viejo cortaba el nivel en la Isla Final
        var ampliacion = GameObject.Find("Ampliacion (nivel 2)");
        if (ampliacion != null) Borrar(ampliacion.transform, "Muro Final");

        var s = new GameObject("Seccion 3 - Puertas de Caramelo").transform;
        s.SetParent(raiz, false);

        Plataforma(s, "Plataforma 2 - Paso 1", new Vector2(114f, 2.4f), new Vector2(2.4f, 0.4f));
        Puerta(s, 1, new Vector2(119.5f, 2.6f), 4f, 1.3f, 0f);
        Plataforma(s, "Plataforma 3 - Paso 2", new Vector2(125f, 3.6f), new Vector2(2.4f, 0.4f));
        Movil(Plataforma(s, "Plataforma 1 - Ascensor 2", new Vector2(129.5f, 3.6f), new Vector2(2.2f, 0.4f)), new Vector2(0f, 4f), 0.9f, 1f);
        Puerta(s, 2, new Vector2(135f, 7.2f), 4.4f, 1.4f, 1.5f);
        Plataforma(s, "Plataforma 2 - Alto 2", new Vector2(140.5f, 8.6f), new Vector2(2.6f, 0.4f));
        Plataforma(s, "Plataforma 3 - Nube", new Vector2(144f, 11.4f), new Vector2(2.2f, 0.4f));
        Puerta(s, 3, new Vector2(146f, 5f), 5f, 1.6f, 0.8f);
        Movil(Plataforma(s, "Plataforma 2 - Movil C", new Vector2(151.5f, 4f), new Vector2(2.4f, 0.4f)), new Vector2(5f, 0f), 1.1f, 0.4f);
        Puerta(s, 4, new Vector2(162f, 4.6f), 4.4f, 1.4f, 2.2f);
        Plataforma(s, "Isla Meta", new Vector2(169f, 3.5f), new Vector2(6f, 3f));
        Plataforma(s, "Muro Final", new Vector2(172.6f, 11f), new Vector2(1.2f, 16f));

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaChupeta);
        var posiciones = new[]
        {
            new Vector2(114f, 4f), new Vector2(119.5f, 4.4f), new Vector2(125f, 5.2f), new Vector2(129.5f, 9.4f),
            new Vector2(135f, 8.8f), new Vector2(140.5f, 10.2f), new Vector2(144f, 13f), new Vector2(146f, 6.6f),
            new Vector2(154f, 5.6f), new Vector2(162f, 6.2f), new Vector2(169f, 6.6f)
        };
        if (prefab != null)
        {
            for (int i = 0; i < posiciones.Length; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(prefab, s);
                c.name = "Chupeta Seccion 3 - " + (i + 1);
                c.transform.position = posiciones[i];
            }
        }

        Gomita(s, 5, new Vector2(166.4f, 5f), 4.2f, Paleta.Lila);
        Corazon(s, 3, new Vector2(157f, 7.2f));

        if (chupeta != null)
        {
            var gigantes = new[] { new Vector4(118f, 8f, 0.5f, 6f), new Vector4(138f, 12.5f, 0.45f, -7f), new Vector4(158f, 10f, 0.55f, 5f) };
            foreach (var g in gigantes)
            {
                var t = Sprite2D(s, "Chupeta Gigante", chupeta, Paleta.ConAlfa(Color.white, 0.35f), -4);
                t.position = new Vector3(g.x, g.y, 0f);
                t.localScale = Vector3.one * g.z;
                t.rotation = Quaternion.Euler(0f, 0f, g.w);
                var a = t.gameObject.AddComponent<ChupetaAnimada>();
                a.altura = 0.25f;
                a.velocidad = 0.8f;
                a.giro = 5f;
            }
        }

        registro.Add("Seccion 3 sobre el vacio: 8 plataformas, 4 puertas que se abren y se cierran, " + posiciones.Length + " chupetas, 1 gomita y 1 corazon");
    }

    private static GameObject Plataforma(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(centro.x, centro.y, 0f);
        go.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cuadrado;
        go.AddComponent<BoxCollider2D>();
        DecoradorNivel.DecorarSiCorresponde(sr);
        return go;
    }

    private static void Movil(GameObject go, Vector2 desplazamiento, float velocidad, float desfase)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var pm = go.AddComponent<PlataformaMovil>();
        pm.desplazamiento = desplazamiento;
        pm.velocidad = velocidad;
        pm.desfase = desfase;
    }

    private static void Puerta(Transform padre, int n, Vector2 centro, float ancho, float apertura, float desfase)
    {
        var go = new GameObject("Puerta " + n);
        go.transform.SetParent(padre, false);
        go.transform.position = centro;

        var mitades = new Rigidbody2D[2];
        for (int i = 0; i < 2; i++)
        {
            float lado = i == 0 ? -1f : 1f;
            var m = Plataforma(go.transform, i == 0 ? "Plataforma 1 - Puerta Izquierda" : "Plataforma 1 - Puerta Derecha",
                centro + Vector2.right * (lado * ancho * 0.25f), new Vector2(ancho * 0.5f - 0.06f, 0.4f));
            // las puertas son lila para distinguirlas de las plataformas fijas
            m.GetComponent<SpriteRenderer>().color = Paleta.Lila;
            var glaseado = m.transform.Find("Glaseado");
            if (glaseado != null) glaseado.GetComponent<SpriteRenderer>().color = Color.Lerp(Paleta.Lila, Paleta.Crema, 0.75f);

            var rb = m.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            mitades[i] = rb;
        }

        var p = go.AddComponent<PlataformaPuerta>();
        p.izquierda = mitades[0];
        p.derecha = mitades[1];
        p.apertura = apertura;
        p.desfase = desfase;
    }

    private static void AjustarLimitesCamara()
    {
        var limites = GameObject.Find("Limites de Camara");
        if (limites == null || !limites.TryGetComponent(out PolygonCollider2D poly)) return;
        poly.SetPath(0, new[]
        {
            new Vector2(-20f, -7f), new Vector2(FinNivel, -7f),
            new Vector2(FinNivel, 18f), new Vector2(-20f, 18f)
        });
        EditorUtility.SetDirty(poly);
    }

    // ================================================================ nubes con parallax

    private static void NubesParallax(Transform raiz)
    {
        var r = new GameObject("Nubes Parallax").transform;
        r.SetParent(raiz, false);
        // lejanas: chicas, palidas y casi pegadas a la camara; cercanas: grandes y casi fijas
        CapaNubes(r, "Nubes Lejanas", 30, 0.75f, 0.5f, 0.85f, 5f, 13f, -7, Paleta.ConAlfa(Color.Lerp(Color.white, Paleta.Lila, 0.15f), 0.55f), 0.08f, 0.18f, 21);
        CapaNubes(r, "Nubes Cercanas", 14, 0.12f, 1.4f, 2f, -1f, 16f, -2, Paleta.ConAlfa(Color.white, 0.85f), 0.35f, 0.6f, 22);
    }

    private static void CapaNubes(Transform padre, string nombre, int cantidad, float parallax, float escalaMin, float escalaMax,
        float yMin, float yMax, int orden, Color color, float velMin, float velMax, int semilla)
    {
        var capa = new GameObject(nombre).transform;
        capa.SetParent(padre, false);
        var partes = new[] { new Vector3(0f, 0f, 1.6f), new Vector3(-0.95f, -0.25f, 1.15f), new Vector3(0.95f, -0.3f, 1.1f), new Vector3(0.35f, 0.45f, 1f) };
        const float desde = -45f;
        const float hasta = FinNivel + 30f;

        Random.InitState(semilla);
        for (int i = 0; i < cantidad; i++)
        {
            var nube = new GameObject("Nube " + (i + 1)).transform;
            nube.SetParent(capa, false);
            nube.position = new Vector3(desde + (hasta - desde) * (i + Random.value) / cantidad, Random.Range(yMin, yMax), 0f);
            nube.localScale = Vector3.one * Random.Range(escalaMin, escalaMax);
            foreach (var p in partes)
            {
                var b = Sprite2D(nube, "Bola", circulo, color, orden);
                b.localPosition = new Vector3(p.x, p.y, 0f);
                b.localScale = new Vector3(p.z, p.z, 1f);
            }
            var mov = nube.gameObject.AddComponent<NubeFlotante>();
            mov.velocidad = Random.Range(velMin, velMax);
            mov.limiteIzquierdo = desde;
            mov.limiteDerecho = hasta;
            mov.parallax = parallax;
        }
    }

    private static void Trampolin(Transform padre, int n, Vector2 suelo)
    {
        var go = new GameObject("Trampolin " + n);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(suelo.x, suelo.y + 0.25f, 0f);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.6f, 0.5f);

        var baseT = Sprite2D(go.transform, "Base", cuadrado, Paleta.Chocolate, 2);
        baseT.localPosition = new Vector3(0f, -0.1f, 0f);
        baseT.localScale = new Vector3(1.6f, 0.3f, 1f);
        var colchon = Sprite2D(go.transform, "Colchon", cuadrado, Paleta.Rosa, 3);
        colchon.localPosition = new Vector3(0f, 0.14f, 0f);
        colchon.localScale = new Vector3(1.5f, 0.22f, 1f);
        var brillo = Sprite2D(colchon, "Glaseado", cuadrado, Paleta.Crema, 4);
        brillo.localPosition = new Vector3(0f, 0.32f, 0f);
        brillo.localScale = new Vector3(1f, 0.35f, 1f);

        go.AddComponent<Trampolin>().colchon = colchon;
    }

    private static void Gomita(Transform padre, int n, Vector2 suelo, float distancia, Color color)
    {
        var go = new GameObject("Gomita " + n);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(suelo.x, suelo.y + 0.38f, 0f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.42f;

        var cuerpo = Sprite2D(go.transform, "Cuerpo", circulo, color, 3);
        cuerpo.localScale = new Vector3(0.95f, 0.8f, 1f);
        var brillo = Sprite2D(cuerpo, "Brillo", circulo, Paleta.ConAlfa(Paleta.Crema, 0.7f), 4);
        brillo.localPosition = new Vector3(-0.22f, 0.22f, 0f);
        brillo.localScale = new Vector3(0.18f, 0.14f, 1f);

        var pupilas = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            float lado = i == 0 ? -1f : 1f;
            var ojo = Sprite2D(go.transform, "Ojo", circulo, Color.white, 4);
            ojo.localPosition = new Vector3(lado * 0.17f, 0.12f, 0f);
            ojo.localScale = new Vector3(0.28f, 0.3f, 1f);
            pupilas[i] = Sprite2D(go.transform, "Pupila", circulo, Paleta.Ciruela, 5);
            pupilas[i].localPosition = new Vector3(lado * 0.17f, 0.1f, 0f);
            pupilas[i].localScale = new Vector3(0.14f, 0.16f, 1f);
        }

        var g = go.AddComponent<Gomita>();
        g.cuerpo = cuerpo;
        g.pupilas = pupilas;
        g.distancia = distancia;
        g.velocidad = 1.4f + n * 0.1f;
    }

    private static void Corazon(Transform padre, int n, Vector2 posicion)
    {
        var go = new GameObject("Corazon Vida " + n);
        go.transform.SetParent(padre, false);
        go.transform.position = posicion;
        go.transform.localScale = Vector3.one * 0.8f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = corazon;
        sr.color = Paleta.Fresa;
        sr.sortingOrder = 6;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        go.AddComponent<CorazonVida>();
    }

    private static void Colinas(Transform padre, string nombre, float factor, Color color, int orden,
        float radioMin, float radioMax, float y, float paso, float desde, float hasta, int semilla)
    {
        var capa = new GameObject(nombre);
        capa.transform.SetParent(padre, false);
        var p = capa.AddComponent<Parallax>();
        p.factor = factor;
        p.factorVertical = factor;

        Random.InitState(semilla);
        int i = 0;
        for (float x = desde; x <= hasta; x += paso)
        {
            float radio = Random.Range(radioMin, radioMax);
            var c = Sprite2D(capa.transform, "Colina " + (++i), circulo, color, orden);
            c.position = new Vector3(x + Random.Range(-2f, 2f), y + Random.Range(-1f, 1f), 0f);
            c.localScale = new Vector3(radio * 2f, radio * 2f, 1f);
        }
    }

    private static void AjustarCamara()
    {
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
        {
            if (mb == null || mb.GetType().Name != "CinemachinePositionComposer") continue;
            var so = new SerializedObject(mb);
            var p = so.FindProperty("Damping");
            if (p != null) p.vector3Value = new Vector3(0.8f, 0.3f, 1f);
            p = so.FindProperty("Lookahead.Time");
            if (p != null) p.floatValue = 0.5f;
            p = so.FindProperty("Composition.DeadZone.Size");
            if (p != null) p.vector2Value = new Vector2(0.1f, 0.1f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static HudJuego ArmarHud(GameObject canvasGO, GameManagerNivel gm)
    {
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.sortingLayerID = SortingLayer.NameToID("Bloqueo");
        canvas.sortingOrder = 100;
        Transform c = canvasGO.transform;

        foreach (var nombre in new[] { "Vidas", "Barra Progreso", "Banner", "Destello" }) Borrar(c, nombre);

        // ---- barra superior
        PintarImagen(c, "Fondo Stats", Paleta.ConAlfa(Paleta.Ciruela, 0.45f));
        ((RectTransform)c.Find("Fondo Stats")).sizeDelta = new Vector2(360f, 185f);
        PintarImagen(c, "Fondo Tiempo", Paleta.ConAlfa(Paleta.Ciruela, 0.45f));
        PintarBoton(c.Find("Boton Pausa"), Paleta.Lila, Paleta.Crema);
        Texto(c, "Texto Contador").color = Paleta.Crema;
        Texto(c, "Texto Tiempo").color = Paleta.Crema;

        var barra = UI("Barra Progreso", c, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(145f, -104f), new Vector2(190f, 16f));
        Imagen(barra, uiSprite, Paleta.ConAlfa(Paleta.Ciruela, 0.6f), true);
        var relleno = Estirar("Relleno", barra, 2f);
        var imgRelleno = Imagen(relleno, uiSprite, Paleta.Limon, false);
        imgRelleno.type = Image.Type.Filled;
        imgRelleno.fillMethod = Image.FillMethod.Horizontal;
        imgRelleno.fillOrigin = (int)Image.OriginHorizontal.Left;
        imgRelleno.fillAmount = 0f;

        var vidas = UI("Vidas", c, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(50f, -128f), new Vector2(170f, 44f));
        var corazones = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var rt = UI("Corazon " + (i + 1), vidas, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * 56f, 0f), new Vector2(46f, 42f));
            corazones[i] = Imagen(rt, corazon, Paleta.Fresa, false);
        }

        // ---- cartel de inicio
        var banner = UI("Banner", c, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(1100f, 230f));
        Imagen(banner, uiSprite, Paleta.ConAlfa(Paleta.Ciruela, 0.85f), true);
        var textoBanner = NuevoTexto(UI("Titulo", banner, Centro, Centro, new Vector2(0f, 28f), new Vector2(1060f, 100f)), "¡A RECOGER CHUPETAS!", 76f, Paleta.Limon);
        var subBanner = NuevoTexto(UI("Subtitulo", banner, Centro, Centro, new Vector2(0f, -55f), new Vector2(1060f, 50f)), "", 32f, Paleta.Crema);
        banner.gameObject.SetActive(false);

        // ---- paneles
        var pausa = c.Find("Panel Pausa");
        PintarImagen(c, "Panel Pausa", Paleta.ConAlfa(Paleta.Ciruela, 0.8f));
        Borrar(pausa, "Control Musica");
        Borrar(pausa, "Control Efectos");
        Mover(pausa, "Titulo", 0f, 300f);
        Texto(pausa, "Titulo").color = Paleta.Crema;
        Deslizador(pausa, "Control Musica", "MUSICA", true, 180f);
        Deslizador(pausa, "Control Efectos", "EFECTOS", false, 105f);
        Mover(pausa, "Boton Continuar", 0f, -10f);
        Mover(pausa, "Boton Reiniciar", 0f, -130f);
        Mover(pausa, "Boton Menu", 0f, -250f);
        PintarBoton(pausa.Find("Boton Continuar"), Paleta.Menta, Paleta.Ciruela);
        PintarBoton(pausa.Find("Boton Reiniciar"), Paleta.Limon, Paleta.Ciruela);
        PintarBoton(pausa.Find("Boton Menu"), Paleta.Lila, Paleta.Crema);

        var victoria = c.Find("Panel Victoria");
        PintarImagen(c, "Panel Victoria", Paleta.ConAlfa(Paleta.Ciruela, 0.85f));
        foreach (var nombre in new[] { "Estrellas", "Texto Estadisticas", "Texto Record" }) Borrar(victoria, nombre);
        Mover(victoria, "Titulo", 0f, 330f);
        Texto(victoria, "Titulo").color = Paleta.Limon;
        var resumen = victoria.Find("Resumen");
        if (resumen != null) resumen.gameObject.SetActive(false);
        var filaEstrellas = UI("Estrellas", victoria, Centro, Centro, new Vector2(0f, 185f), new Vector2(600f, 180f));
        var estrellas = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            bool medio = i == 1;
            var rt = UI("Estrella " + (i + 1), filaEstrellas, Centro, Centro, new Vector2((i - 1) * 180f, medio ? 15f : 0f), Vector2.one * (medio ? 160f : 130f));
            estrellas[i] = Imagen(rt, estrella, Paleta.Limon, false);
        }
        var estadisticas = NuevoTexto(UI("Texto Estadisticas", victoria, Centro, Centro, new Vector2(0f, 25f), new Vector2(900f, 180f)), "", 40f, Paleta.Crema);
        var record = NuevoTexto(UI("Texto Record", victoria, Centro, Centro, new Vector2(0f, -85f), new Vector2(900f, 60f)), "", 44f, Paleta.Limon);
        Mover(victoria, "Boton Jugar de Nuevo", 0f, -190f);
        Mover(victoria, "Boton Menu", 0f, -310f);
        PintarBoton(victoria.Find("Boton Jugar de Nuevo"), Paleta.Limon, Paleta.Ciruela);
        PintarBoton(victoria.Find("Boton Menu"), Paleta.Lila, Paleta.Crema);

        var derrota = c.Find("Panel Derrota");
        PintarImagen(c, "Panel Derrota", Paleta.ConAlfa(Paleta.Ciruela, 0.85f));
        Borrar(derrota, "Resumen Derrota");
        Mover(derrota, "Titulo", 0f, 220f);
        Texto(derrota, "Titulo").color = Paleta.Rosa;
        var resumenDerrota = NuevoTexto(UI("Resumen Derrota", derrota, Centro, Centro, new Vector2(0f, 85f), new Vector2(1100f, 140f)), "", 38f, Paleta.Crema);
        Mover(derrota, "Boton Reintentar", 0f, -50f);
        Mover(derrota, "Boton Menu", 0f, -170f);
        PintarBoton(derrota.Find("Boton Reintentar"), Paleta.Rosa, Paleta.Crema);
        PintarBoton(derrota.Find("Boton Menu"), Paleta.Lila, Paleta.Crema);

        foreach (var p in new[] { pausa, victoria, derrota }) Asegurar<CanvasGroup>(p.gameObject);

        // ---- destello de pantalla completa, siempre encima
        var destello = Estirar("Destello", c, 0f);
        var imgDestello = Imagen(destello, null, Paleta.ConAlfa(Color.white, 0f), false);
        destello.SetAsLastSibling();

        var hud = Asegurar<HudJuego>(canvasGO);
        hud.icono = (RectTransform)c.Find("Icono Chupeta");
        hud.textoContador = Texto(c, "Texto Contador");
        hud.barraProgreso = imgRelleno;
        hud.corazones = corazones;
        hud.banner = banner;
        hud.textoBanner = textoBanner;
        hud.subtextoBanner = subBanner;
        hud.destello = imgDestello;
        hud.tituloVictoria = (RectTransform)victoria.Find("Titulo");
        hud.estrellas = estrellas;
        hud.textoEstadisticas = estadisticas;
        hud.textoRecord = record;
        hud.botonesVictoria = new[] { (RectTransform)victoria.Find("Boton Jugar de Nuevo"), (RectTransform)victoria.Find("Boton Menu") };
        hud.tituloDerrota = (RectTransform)derrota.Find("Titulo");
        hud.textoResumenDerrota = resumenDerrota;
        hud.botonesDerrota = new[] { (RectTransform)derrota.Find("Boton Reintentar"), (RectTransform)derrota.Find("Boton Menu") };
        EditorUtility.SetDirty(hud);
        return hud;
    }

    // ================================================================ inicio

    private static void PulirInicio(List<string> registro)
    {
        var canvasGO = GameObject.Find("Canvas");
        var control = Object.FindAnyObjectByType<ControlEscenas>(FindObjectsInactive.Include);
        var camara = Camera.main;
        if (canvasGO == null || control == null || camara == null)
        {
            registro.Add("ERROR: no se encontro el Canvas, el Control de Escenas o la camara en inicio");
            return;
        }

        CrearFondoInicio(camara);
        registro.Add("Inicio: paisaje con cielo, colinas, suelo de chocolate, nubes y dulces; la camara lo recorre despacio");

        Transform c = canvasGO.transform;
        var menu = Asegurar<MenuInicio>(control.gameObject);
        menu.camara = camara.transform;
        menu.controlEscenas = control;

        PintarImagen(c, "Fondo Base", Paleta.ConAlfa(Paleta.Ciruela, 0.2f));
        var fondoViejo = c.Find("Fondo Inicio");
        if (fondoViejo != null) fondoViejo.gameObject.SetActive(false);

        foreach (var nombre in new[] { "Titulo", "Titulo Sombra" })
        {
            var t = c.Find(nombre);
            if (t == null) continue;
            t.GetComponent<TextMeshProUGUI>().color = nombre == "Titulo" ? Paleta.Crema : Paleta.Ciruela;
            var a = Asegurar<AparecerUI>(t.gameObject);
            a.modo = AparecerUI.Modo.Escala;
            a.retraso = 0.1f;
            a.duracion = 0.6f;
            var d = Asegurar<DecoracionMenu>(t.gameObject);
            d.amplitud = 8f;
            d.velocidad = 1.2f;
            d.rotacionMaxima = 2f;
            d.desfase = 0f;
        }
        var sub = c.Find("Subtitulo");
        if (sub != null)
        {
            var a = Asegurar<AparecerUI>(sub.gameObject);
            a.modo = AparecerUI.Modo.Escala;
            a.retraso = 0.35f;
        }

        // ---- botones: JUGAR, OPCIONES (nuevo) y SALIR
        var jugar = c.Find("Button (1)");
        var salir = c.Find("Button (2)");
        Borrar(c, "Boton Opciones");
        var opciones = Object.Instantiate(salir.gameObject, c).transform;
        opciones.name = "Boton Opciones";
        opciones.SetSiblingIndex(salir.GetSiblingIndex());
        var botonOpciones = opciones.GetComponent<Button>();
        while (botonOpciones.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(botonOpciones.onClick, 0);
        UnityEventTools.AddPersistentListener(botonOpciones.onClick, menu.AbrirOpciones);
        var etiqueta = opciones.GetComponentInChildren<TextMeshProUGUI>();
        if (etiqueta != null) etiqueta.text = "OPCIONES";

        ConfigurarBotonMenu(jugar, Paleta.Rosa, new Vector2(0f, -10f), 0.45f, 0.035f);
        ConfigurarBotonMenu(opciones, Paleta.Lila, new Vector2(0f, -130f), 0.55f, 0f);
        ConfigurarBotonMenu(salir, Paleta.Ciruela, new Vector2(0f, -250f), 0.65f, 0f);
        registro.Add("Inicio: titulo y botones entran animados, JUGAR late, nuevo boton OPCIONES");

        // ---- textos de ayuda y record
        Borrar(c, "Barra Inferior");
        Borrar(c, "Texto Controles");
        Borrar(c, "Texto Record");
        var barraInferior = UI("Barra Inferior", c, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 140f));
        barraInferior.anchorMin = new Vector2(0f, 0f);
        barraInferior.anchorMax = new Vector2(1f, 0f);
        Imagen(barraInferior, null, Paleta.ConAlfa(Paleta.Ciruela, 0.6f), false);
        var controles = NuevoTexto(UI("Texto Controles", c, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1700f, 50f)),
            "MOVERSE  A/D o flechas      SALTAR  espacio      CORRER  shift      PAUSA  esc", 28f, Paleta.ConAlfa(Paleta.Crema, 0.9f));
        Asegurar<AparecerUI>(controles.gameObject).modo = AparecerUI.Modo.DesdeAbajo;
        controles.GetComponent<AparecerUI>().retraso = 0.8f;
        controles.GetComponent<AparecerUI>().distancia = 120f;
        var record = NuevoTexto(UI("Texto Record", c, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(1400f, 50f)), "", 32f, Paleta.Limon);
        menu.textoRecord = record;

        // ---- panel de opciones
        Borrar(c, "Panel Opciones");
        var panel = Estirar("Panel Opciones", c, 0f);
        Imagen(panel, null, Paleta.ConAlfa(Paleta.Ciruela, 0.8f), false).raycastTarget = true;
        var tarjeta = UI("Tarjeta", panel, Centro, Centro, Vector2.zero, new Vector2(720f, 520f));
        Imagen(tarjeta, uiSprite, Paleta.ConAlfa(Paleta.Ciruela, 0.95f), true);
        Asegurar<AparecerUI>(tarjeta.gameObject).duracion = 0.35f;
        NuevoTexto(UI("Titulo", tarjeta, Centro, Centro, new Vector2(0f, 180f), new Vector2(640f, 90f)), "OPCIONES", 70f, Paleta.Limon);
        Deslizador(tarjeta, "Control Musica", "MUSICA", true, 60f);
        Deslizador(tarjeta, "Control Efectos", "EFECTOS", false, -30f);
        CrearBoton(tarjeta, "Boton Volver", "VOLVER", Paleta.Menta, Paleta.Ciruela, new Vector2(0f, -170f), new Vector2(360f, 90f), menu.CerrarOpciones);
        panel.SetAsLastSibling();
        panel.gameObject.SetActive(false);
        menu.panelOpciones = panel.gameObject;
        EditorUtility.SetDirty(menu);
        registro.Add("Inicio: panel de opciones con volumen de musica y efectos, controles y record en pantalla");
    }

    private static void ConfigurarBotonMenu(Transform boton, Color color, Vector2 posicion, float retraso, float pulso)
    {
        ((RectTransform)boton).anchoredPosition = posicion;
        PintarBoton(boton, color, Paleta.Crema);
        boton.GetComponent<BotonHover>().pulso = pulso;
        var a = Asegurar<AparecerUI>(boton.gameObject);
        a.modo = AparecerUI.Modo.DesdeAbajo;
        a.retraso = retraso;
        a.distancia = 500f;
    }

    private static void CrearFondoInicio(Camera camara)
    {
        var viejo = GameObject.Find(NombreFondoInicio);
        if (viejo != null) Object.DestroyImmediate(viejo);
        Borrar(camara.transform, NombreCielo);
        camara.backgroundColor = Color.Lerp(Paleta.Agua, Paleta.Crema, 0.5f);

        if (cielo != null)
        {
            var c = Sprite2D(camara.transform, NombreCielo, cielo, new Color(1f, 0.9f, 0.97f), -20);
            c.localPosition = new Vector3(0f, 0f, 10f);
            Vector2 tam = cielo.bounds.size;
            c.localScale = new Vector3(22f / tam.x, 12f / tam.y, 1f);
        }

        var raiz = new GameObject(NombreFondoInicio).transform;

        var lejanas = Capa(raiz, "Colinas Lejanas", 0.7f);
        var cercanas = Capa(raiz, "Colinas Cercanas", 0.4f);
        Random.InitState(11);
        for (float x = -22f; x <= 22f; x += 4.5f)
        {
            var h = Sprite2D(lejanas, "Colina", circulo, Color.Lerp(Paleta.Lila, Paleta.Crema, 0.45f), -14);
            h.position = new Vector3(x + Random.Range(-1f, 1f), -5.6f + Random.Range(-0.5f, 0.5f), 0f);
            h.localScale = Vector3.one * Random.Range(6f, 9f);
        }
        for (float x = -20f; x <= 20f; x += 3.5f)
        {
            var h = Sprite2D(cercanas, "Colina", circulo, Color.Lerp(Paleta.Rosa, Paleta.Crema, 0.55f), -13);
            h.position = new Vector3(x + Random.Range(-1f, 1f), -5.3f + Random.Range(-0.4f, 0.4f), 0f);
            h.localScale = Vector3.one * Random.Range(4.5f, 6.5f);
        }

        var suelo = Sprite2D(raiz, "Suelo", cuadrado, Paleta.Chocolate, -10);
        suelo.position = new Vector3(0f, -4.6f, 0f);
        suelo.localScale = new Vector3(60f, 1.4f, 1f);
        var glaseado = Sprite2D(raiz, "Glaseado", cuadrado, Color.Lerp(Paleta.Rosa, Paleta.Crema, 0.3f), -9);
        glaseado.position = new Vector3(0f, -3.92f, 0f);
        glaseado.localScale = new Vector3(60f, 0.22f, 1f);

        var nubes = new GameObject("Nubes").transform;
        nubes.SetParent(raiz, false);
        var posNubes = new[] { new Vector2(-14f, 3.5f), new Vector2(-7f, 2.6f), new Vector2(0f, 3.9f), new Vector2(7f, 2.9f), new Vector2(14f, 3.6f), new Vector2(20f, 2.4f) };
        var partes = new[] { new Vector3(0f, 0f, 1.6f), new Vector3(-0.95f, -0.25f, 1.15f), new Vector3(0.95f, -0.3f, 1.1f), new Vector3(0.35f, 0.45f, 1f) };
        for (int i = 0; i < posNubes.Length; i++)
        {
            var nube = new GameObject("Nube " + (i + 1)).transform;
            nube.SetParent(nubes, false);
            nube.position = posNubes[i];
            nube.localScale = Vector3.one * Random.Range(0.7f, 1.1f);
            foreach (var p in partes)
            {
                var b = Sprite2D(nube, "Bola", circulo, Paleta.ConAlfa(Color.white, 0.9f), -12);
                b.localPosition = new Vector3(p.x, p.y, 0f);
                b.localScale = new Vector3(p.z, p.z, 1f);
            }
            var mov = nube.gameObject.AddComponent<NubeFlotante>();
            mov.velocidad = Random.Range(0.2f, 0.4f);
            mov.limiteIzquierdo = -25f;
            mov.limiteDerecho = 25f;
        }

        var dulces = new GameObject("Dulces").transform;
        dulces.SetParent(raiz, false);
        if (chupeta != null)
        {
            // solo a los lados: el centro queda libre para el titulo y los botones
            var posChupetas = new[]
            {
                new Vector3(-16f, -3.1f, -5f), new Vector3(-12f, -3.2f, 8f), new Vector3(-8.8f, -3.1f, -6f),
                new Vector3(8.8f, -3.1f, 5f), new Vector3(12.5f, -3.2f, -8f), new Vector3(16.5f, -3.1f, 6f)
            };
            foreach (var p in posChupetas)
            {
                var ch = Sprite2D(dulces, "Chupeta Gigante", chupeta, Color.white, -8);
                ch.position = new Vector3(p.x, p.y, 0f);
                ch.rotation = Quaternion.Euler(0f, 0f, p.z);
                ch.localScale = Vector3.one * 0.38f;
                var anim = ch.gameObject.AddComponent<ChupetaAnimada>();
                anim.altura = 0.08f;
                anim.velocidad = 1f;
                anim.giro = 6f;
            }
        }
        PlataformaDecorativa(dulces, new Vector2(-6.8f, 0.6f), 2.4f, Paleta.Rosa);
        PlataformaDecorativa(dulces, new Vector2(7.2f, 1.4f), 2.2f, Paleta.Menta);
    }

    private static void PlataformaDecorativa(Transform padre, Vector2 posicion, float ancho, Color color)
    {
        var p = Sprite2D(padre, "Plataforma", cuadrado, color, -8);
        p.position = posicion;
        p.localScale = new Vector3(ancho, 0.4f, 1f);
        var g = Sprite2D(p, "Glaseado", cuadrado, Color.Lerp(color, Paleta.Crema, 0.75f), -7);
        g.localPosition = new Vector3(0f, 0.33f, 0f);
        g.localScale = new Vector3(1.03f, 0.4f, 1f);
        if (chupeta == null) return;
        var ch = Sprite2D(padre, "Chupeta", chupeta, Color.white, -7);
        ch.position = posicion + Vector2.up * 0.9f;
        ch.localScale = Vector3.one * 0.3f;
        ch.gameObject.AddComponent<ChupetaAnimada>();
    }

    private static Transform Capa(Transform padre, string nombre, float factor)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var p = go.AddComponent<Parallax>();
        p.factor = factor;
        p.factorVertical = 0f;
        return go.transform;
    }

    // ================================================================ utilidades UI

    private static readonly Vector2 Centro = new Vector2(0.5f, 0.5f);

    private static RectTransform UI(string nombre, Transform padre, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano)
    {
        var go = new GameObject(nombre, typeof(RectTransform)) { layer = 5 };
        var rt = (RectTransform)go.transform;
        rt.SetParent(padre, false);
        rt.anchorMin = rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
        return rt;
    }

    private static RectTransform Estirar(string nombre, Transform padre, float margen)
    {
        var rt = UI(nombre, padre, Vector2.zero, Centro, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(margen, margen);
        rt.offsetMax = new Vector2(-margen, -margen);
        return rt;
    }

    private static Image Imagen(RectTransform rt, Sprite sprite, Color color, bool deslizada)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        img.type = deslizada ? Image.Type.Sliced : Image.Type.Simple;
        img.preserveAspect = !deslizada && sprite != null && sprite != uiSprite;
        return img;
    }

    private static TextMeshProUGUI NuevoTexto(RectTransform rt, string texto, float tamano, Color color)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (fuente != null) t.font = fuente;
        t.text = texto;
        t.fontSize = tamano;
        t.color = color;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    private static TextMeshProUGUI Texto(Transform padre, string nombre)
    {
        var t = padre.Find(nombre);
        return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
    }

    private static void Mover(Transform padre, string nombre, float x, float y)
    {
        var t = padre.Find(nombre) as RectTransform;
        if (t != null) t.anchoredPosition = new Vector2(x, y);
    }

    private static void PintarImagen(Transform padre, string nombre, Color color)
    {
        var t = padre.Find(nombre);
        if (t != null && t.TryGetComponent(out Image img))
        {
            img.color = color;
            EditorUtility.SetDirty(img);
        }
    }

    private static void PintarBoton(Transform boton, Color fondo, Color texto)
    {
        if (boton == null) return;
        if (boton.TryGetComponent(out Image img)) img.color = fondo;
        foreach (var t in boton.GetComponentsInChildren<TextMeshProUGUI>(true)) t.color = texto;
        Asegurar<BotonHover>(boton.gameObject);
    }

    private static void CrearBoton(Transform padre, string nombre, string texto, Color fondo, Color colorTexto,
        Vector2 posicion, Vector2 tamano, UnityAction accion)
    {
        var rt = UI(nombre, padre, Centro, Centro, posicion, tamano);
        var img = Imagen(rt, uiSprite, fondo, true);
        img.raycastTarget = true;
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        NuevoTexto(Estirar("Texto", rt, 0f), texto, 40f, colorTexto);
        rt.gameObject.AddComponent<BotonHover>();
        UnityEventTools.AddPersistentListener(b.onClick, accion);
    }

    private static void Deslizador(Transform padre, string nombre, string etiqueta, bool esMusica, float y)
    {
        var fila = UI(nombre, padre, Centro, Centro, new Vector2(0f, y), new Vector2(640f, 56f));
        var texto = NuevoTexto(UI("Etiqueta", fila, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(220f, 56f)), etiqueta, 34f, Paleta.Crema);
        texto.alignment = TextAlignmentOptions.Left;

        var recursos = new DefaultControls.Resources { standard = uiSprite, background = uiFondo, knob = uiPerilla };
        var go = DefaultControls.CreateSlider(recursos);
        go.name = "Slider";
        foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 5;
        var srt = (RectTransform)go.transform;
        srt.SetParent(fila, false);
        srt.anchorMin = srt.anchorMax = new Vector2(1f, 0.5f);
        srt.pivot = new Vector2(1f, 0.5f);
        srt.anchoredPosition = Vector2.zero;
        srt.sizeDelta = new Vector2(400f, 28f);
        go.transform.Find("Background").GetComponent<Image>().color = Paleta.ConAlfa(Paleta.Crema, 0.25f);
        go.transform.Find("Fill Area/Fill").GetComponent<Image>().color = Paleta.Limon;
        var manija = go.transform.Find("Handle Slide Area/Handle");
        manija.GetComponent<Image>().color = Paleta.Crema;
        ((RectTransform)manija).sizeDelta = new Vector2(40f, 14f);
        go.GetComponent<Slider>().value = 0.8f;
        go.AddComponent<ControlVolumen>().esMusica = esMusica;
    }

    private static Transform Sprite2D(Transform padre, string nombre, Sprite sprite, Color color, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = orden;
        return go.transform;
    }

    private static void Borrar(Transform padre, string nombre)
    {
        if (padre == null) return;
        for (int i = padre.childCount - 1; i >= 0; i--)
            if (padre.GetChild(i).name == nombre)
                Object.DestroyImmediate(padre.GetChild(i).gameObject);
    }

    private static T Asegurar<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }
}
