using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu "Lollipops > Mejorar nivel 2": ajusta la camara Cinemachine, cambia los
// sonidos y le da un estilo de dulce al nivel. Se puede ejecutar varias veces:
// borra y vuelve a crear sus propias decoraciones.
public static class DecoradorNivel
{
    private const string RutaEscena = "Assets/escenas/nivel2.unity";
    private const string RutaSonidoRecoger = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-41.wav";
    private const string RutaSonidoDanio = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-02.wav";
    private const string RutaChupeta = "Assets/Prefabricados/Recolectable _ Chupeta1.prefab";

    private const string NombreNubes = "Nubes (decoracion)";
    private const string NombreGigantes = "Chupetas Gigantes (decoracion)";
    private const string NombreAmpliacion = "Ampliacion (nivel 2)";
    private const float LimiteDerechoNivel = 113f;

    private const string RutaClipEspuma = "Assets/Animaciones/Espuma - Ola.anim";
    private const string RutaControlEspuma = "Assets/Animaciones/Espuma.controller";
    private const float LargoOla = 4f;          // la forma de la ola se repite cada 4 unidades
    private const float SeparacionEspuma = 0.5f; // 8 bolitas por ola
    private const float DuracionOla = 2f;

    [MenuItem("Lollipops/Mejorar nivel 2")]
    public static void Mejorar()
    {
        var escena = EditorSceneManager.GetActiveScene();
        if (escena.path != RutaEscena)
            escena = EditorSceneManager.OpenScene(RutaEscena);

        var registro = new List<string>();
        AmpliarNivel(registro);
        AjustarCamara(registro);
        AjustarSonidos(registro);
        AjustarDiseno(registro);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("Nivel 2 mejorado y guardado:\n- " + string.Join("\n- ", registro));
    }

    // ---------------------------------------------------------------- ampliacion

    private static void AmpliarNivel(List<string> registro)
    {
        var viejo = GameObject.Find(NombreAmpliacion);
        if (viejo != null) Object.DestroyImmediate(viejo);

        Sprite cuadrado = null;
        Sprite circulo = null;
        foreach (var s in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (cuadrado == null && s.name.StartsWith("Plataforma 1") && s.sprite != null) cuadrado = s.sprite;
            if (circulo == null && s.name == "Circle" && s.sprite != null) circulo = s.sprite;
        }
        if (circulo == null) circulo = cuadrado;
        var agua = GameObject.Find("Agua");
        var srAgua = agua != null ? agua.GetComponent<SpriteRenderer>() : null;
        var gm = Object.FindAnyObjectByType<GameManagerNivel>(FindObjectsInactive.Include);
        if (cuadrado == null || srAgua == null || gm == null)
        {
            registro.Add("ERROR: no se encontro la plataforma, el agua o el Game Manager");
            return;
        }

        var raiz = new GameObject(NombreAmpliacion).transform;

        // puente sobre el pozo original, para llegar a la rampa
        Bloque(raiz, "Plataforma 3 - Puente", new Vector2(39.5f, -1.2f), new Vector2(2.5f, 0.4f), cuadrado);

        // nueva seccion despues de la rampa
        Bloque(raiz, "Isla A", new Vector2(54.5f, -0.1f), new Vector2(7f, 2f), cuadrado);
        Movil(Bloque(raiz, "Plataforma 2 - Movil A", new Vector2(60.5f, 1.0f), new Vector2(2.8f, 0.4f), cuadrado), new Vector2(6f, 0f), 1f, 0f);
        Bloque(raiz, "Plataforma 1 - Mirador", new Vector2(64f, 3.8f), new Vector2(2.2f, 0.4f), cuadrado);
        Movil(Bloque(raiz, "Plataforma 3 - Movil Alto", new Vector2(67f, 6.4f), new Vector2(2.4f, 0.4f), cuadrado), new Vector2(7f, 0f), 0.8f, 2f);
        Bloque(raiz, "Isla B", new Vector2(71.5f, 0.2f), new Vector2(4f, 2f), cuadrado);
        Bloque(raiz, "Plataforma 3 - Escalon", new Vector2(76f, 2.8f), new Vector2(2.5f, 0.4f), cuadrado);
        Movil(Bloque(raiz, "Plataforma 1 - Ascensor", new Vector2(80.5f, 2.6f), new Vector2(2.4f, 0.4f), cuadrado), new Vector2(0f, 4.5f), 0.9f, 0f);
        Bloque(raiz, "Plataforma 2 - Alta", new Vector2(85f, 6.8f), new Vector2(3f, 0.4f), cuadrado);
        Bloque(raiz, "Plataforma 3 - Cielo", new Vector2(90f, 9.3f), new Vector2(2.5f, 0.4f), cuadrado);
        Bloque(raiz, "Isla C", new Vector2(91.5f, 0f), new Vector2(6f, 2f), cuadrado);
        Movil(Bloque(raiz, "Plataforma 1 - Movil B", new Vector2(97f, 1.8f), new Vector2(2.6f, 0.4f), cuadrado), new Vector2(5.5f, 0f), 1.2f, 1.5f);
        Bloque(raiz, "Isla Final", new Vector2(108f, 0.5f), new Vector2(6f, 3f), cuadrado);
        Bloque(raiz, "Muro Final", new Vector2(111.6f, 6f), new Vector2(1.2f, 14f), cuadrado);
        registro.Add("Seccion nueva: 13 plataformas e islas (4 moviles)");

        // agua que quita vidas
        var controlEspuma = AsegurarAnimacionEspuma();
        CrearEspuma(CrearAgua(raiz, srAgua, gm, "Lago", new Vector2(76.5f, -4.6f), new Vector2(73f, 2.6f)), controlEspuma, circulo, cuadrado, 0.1f);
        CrearEspuma(CrearAgua(raiz, srAgua, gm, "Charco 1", new Vector2(-3.5f, -3.25f), new Vector2(2.4f, 0.7f)), controlEspuma, circulo, cuadrado, 0.05f);
        CrearEspuma(CrearAgua(raiz, srAgua, gm, "Charco 2", new Vector2(13.5f, -3.25f), new Vector2(2.4f, 0.7f)), controlEspuma, circulo, cuadrado, 0.05f);
        registro.Add("Agua nueva: un lago bajo la seccion nueva y 2 charcos en el suelo");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaChupeta);
        var posiciones = new[]
        {
            new Vector2(39.5f, 0f), new Vector2(54.5f, 2.2f), new Vector2(63.5f, 2.6f),
            new Vector2(64f, 5.3f), new Vector2(72f, 8.2f), new Vector2(71.5f, 2.5f),
            new Vector2(80.5f, 8.7f), new Vector2(85f, 8.4f), new Vector2(90f, 10.9f),
            new Vector2(91.5f, 2.3f), new Vector2(99.5f, 3.4f), new Vector2(108f, 3.4f)
        };
        if (prefab != null)
        {
            for (int i = 0; i < posiciones.Length; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(prefab, raiz);
                c.name = "Chupeta Extra " + (i + 1);
                c.transform.position = new Vector3(posiciones[i].x, posiciones[i].y, 0f);
            }
            registro.Add(posiciones.Length + " chupetas nuevas");
        }

        // plataformas originales que ahora se mueven
        HacerMovil("Plataforma 3 (4)", new Vector2(-3f, 0f), 0.9f, 0.5f, registro);
        HacerMovil("Plataforma 2 (5)", new Vector2(3f, 0f), 0.8f, 1.2f, registro);
        HacerMovil("Plataforma 1 (2)", new Vector2(0f, 2.5f), 1f, 2.4f, registro);

        // el agua original se queda quieta y la espuma lleva la animacion
        if (agua.TryGetComponent(out Animator animador))
        {
            animador.enabled = false;
            EditorUtility.SetDirty(animador);
        }
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(agua);
        CrearEspuma(srAgua, controlEspuma, circulo, cuadrado, 0.1f);
        registro.Add("Espuma con Animator en el agua original, el lago y los charcos");

        var personaje = Object.FindAnyObjectByType<ControlPersonaje>(FindObjectsInactive.Include);
        if (personaje != null)
        {
            personaje.vidasTotales = 3;
            EditorUtility.SetDirty(personaje);
            registro.Add("Jugador con 3 vidas");
        }
    }

    private static GameObject Bloque(Transform padre, string nombre, Vector2 centro, Vector2 tamano, Sprite sprite)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(centro.x, centro.y, 0f);
        go.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        go.AddComponent<BoxCollider2D>();
        return go;
    }

    private static void Movil(GameObject go, Vector2 desplazamiento, float velocidad, float desfase)
    {
        if (!go.TryGetComponent(out Rigidbody2D rb)) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        if (!go.TryGetComponent(out PlataformaMovil pm)) pm = go.AddComponent<PlataformaMovil>();
        pm.desplazamiento = desplazamiento;
        pm.velocidad = velocidad;
        pm.desfase = desfase;
        EditorUtility.SetDirty(rb);
        EditorUtility.SetDirty(pm);
    }

    private static void HacerMovil(string nombre, Vector2 desplazamiento, float velocidad, float desfase, List<string> registro)
    {
        var go = GameObject.Find(nombre);
        if (go == null) return;
        Movil(go, desplazamiento, velocidad, desfase);
        registro.Add("Ahora se mueve: " + nombre);
    }

    private static SpriteRenderer CrearAgua(Transform padre, SpriteRenderer modelo, GameManagerNivel gm, string nombre,
        Vector2 centro, Vector2 tamano)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = new Vector3(centro.x, centro.y, 0f);
        go.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = modelo.sprite;
        sr.color = new Color(0.3f, 0.65f, 1f, 0.8f);
        sr.sortingLayerID = modelo.sortingLayerID;
        sr.sortingOrder = modelo.sortingOrder;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        var zona = go.AddComponent<ZonaPeligro>();
        zona.gestor = gm;
        return sr;
    }

    // Clip "Espuma - Ola": mueve la fila de espuma un largo de ola hacia la derecha
    // en bucle. Como la forma se repite cada LargoOla, el salto al reiniciar no se nota.
    private static RuntimeAnimatorController AsegurarAnimacionEspuma()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(RutaClipEspuma);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, RutaClipEspuma);
        }
        clip.ClearCurves();

        var avance = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(DuracionOla, LargoOla));
        for (int i = 0; i < avance.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(avance, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(avance, i, AnimationUtility.TangentMode.Linear);
        }
        var subeBaja = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(DuracionOla * 0.25f, 0.05f), new Keyframe(DuracionOla * 0.5f, 0f),
            new Keyframe(DuracionOla * 0.75f, -0.05f), new Keyframe(DuracionOla, 0f));
        var fijo = AnimationCurve.Constant(0f, DuracionOla, 0f);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Ola", typeof(Transform), "m_LocalPosition.x"), avance);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Ola", typeof(Transform), "m_LocalPosition.y"), subeBaja);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Ola", typeof(Transform), "m_LocalPosition.z"), fijo);

        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);
        EditorUtility.SetDirty(clip);

        var control = AssetDatabase.LoadAssetAtPath<AnimatorController>(RutaControlEspuma);
        if (control == null) control = AnimatorController.CreateAnimatorControllerAtPathWithClip(RutaControlEspuma, clip);
        AssetDatabase.SaveAssets();
        return control;
    }

    // Crea "Espuma (nombre del agua)": un Animator, una fila de bolitas en "Ola" y
    // una mascara para que las bolitas solo se vean sobre el agua.
    private static void CrearEspuma(SpriteRenderer agua, RuntimeAnimatorController control, Sprite circulo, Sprite cuadrado, float amplitud)
    {
        string nombre = "Espuma (" + agua.name + ")";
        Transform padre = agua.transform.parent;
        if (padre != null) BorrarHijo(padre, nombre);
        else
        {
            var vieja = GameObject.Find(nombre);
            if (vieja != null) Object.DestroyImmediate(vieja);
        }

        Bounds b = agua.bounds;
        var raiz = new GameObject(nombre).transform;
        raiz.SetParent(padre, false);
        raiz.position = new Vector3(b.min.x, b.max.y, 0f);
        raiz.gameObject.AddComponent<Animator>().runtimeAnimatorController = control;

        var ola = new GameObject("Ola").transform;
        ola.SetParent(raiz, false);
        int cantidad = Mathf.CeilToInt((b.size.x + LargoOla) / SeparacionEspuma) + 1;
        for (int i = 0; i < cantidad; i++)
        {
            float x = i * SeparacionEspuma;
            var bola = CrearHijo(ola, "Bola " + (i + 1), circulo, new Color(0.85f, 0.96f, 1f, 0.9f),
                agua.sortingLayerID, agua.sortingOrder + 1);
            bola.localPosition = new Vector3(x - LargoOla, Mathf.Sin(x / LargoOla * Mathf.PI * 2f) * amplitud - 0.1f, 0f);
            bola.localScale = new Vector3(0.45f, 0.45f, 1f);
            bola.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        var mascara = new GameObject("Mascara").transform;
        mascara.SetParent(raiz, false);
        const float sobreAgua = 0.6f;
        mascara.localPosition = new Vector3(b.size.x * 0.5f, (sobreAgua - b.size.y) * 0.5f, 0f);
        mascara.localScale = new Vector3(b.size.x, b.size.y + sobreAgua, 1f);
        mascara.gameObject.AddComponent<SpriteMask>().sprite = cuadrado;
    }

    // ---------------------------------------------------------------- camara

    private static void AjustarCamara(List<string> registro)
    {
        foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
        {
            if (c == null) continue;
            string tipo = c.GetType().Name;
            var so = new SerializedObject(c);

            if (tipo == "CinemachineCamera")
            {
                PonerFloat(so, "Lens.OrthographicSize", 6.5f);
                registro.Add("Camara: zoom 6.5 siguiendo a " + ObjetoDe(so, "Target.TrackingTarget"));
            }
            else if (tipo == "CinemachinePositionComposer")
            {
                PonerVector3(so, "Damping", new Vector3(0.6f, 0.5f, 1f));
                PonerBool(so, "Lookahead.Enabled", true);
                PonerFloat(so, "Lookahead.Time", 0.3f);
                PonerFloat(so, "Lookahead.Smoothing", 5f);
                PonerBool(so, "Lookahead.IgnoreY", true);
                PonerBool(so, "Composition.DeadZone.Enabled", true);
                PonerVector2(so, "Composition.DeadZone.Size", new Vector2(0.12f, 0.18f));
                registro.Add("Camara: suavizado y anticipacion activados");
            }
            else if (tipo == "CinemachineConfiner2D")
            {
                PonerFloat(so, "Damping", 0.3f);
            }
            else continue;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var limites = GameObject.Find("Limites de Camara");
        if (limites != null && limites.TryGetComponent(out PolygonCollider2D poly))
        {
            poly.SetPath(0, new[]
            {
                new Vector2(-20f, -7f), new Vector2(LimiteDerechoNivel, -7f),
                new Vector2(LimiteDerechoNivel, 18f), new Vector2(-20f, 18f)
            });
            EditorUtility.SetDirty(poly);
            registro.Add("Camara: limites ampliados a todo el nivel");
        }

        var principal = Camera.main;
        if (principal != null)
        {
            principal.orthographicSize = 6.5f;
            EditorUtility.SetDirty(principal);
        }
    }

    // ---------------------------------------------------------------- sonidos

    private static void AjustarSonidos(List<string> registro)
    {
        foreach (var r in Object.FindObjectsByType<RecolectableChupeta1_Controlador>(FindObjectsInactive.Include))
        {
            var fuentes = r.GetComponents<AudioSource>();
            if (fuentes.Length == 0 && r.SonidoOcio == null && r.SonidoRecolectado == null) continue;

            r.SonidoOcio = null;
            r.SonidoRecolectado = null;
            EditorUtility.SetDirty(r);
            foreach (var a in fuentes)
            {
                if (PrefabUtility.IsAddedComponentOverride(a))
                    PrefabUtility.RevertAddedComponent(a, InteractionMode.AutomatedAction);
                else
                    Object.DestroyImmediate(a);
            }
            registro.Add("Sonido en bucle quitado de " + r.name);
        }

        var gm = Object.FindAnyObjectByType<GameManagerNivel>(FindObjectsInactive.Include);
        if (gm == null) return;

        var clipRecoger = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaSonidoRecoger);
        if (gm.contadorChupetas != null && gm.contadorChupetas.sonidoAumentar != null && clipRecoger != null)
        {
            var a = gm.contadorChupetas.sonidoAumentar;
            a.clip = clipRecoger;
            a.volume = 0.8f;
            a.playOnAwake = false;
            a.loop = false;
            EditorUtility.SetDirty(a);
            registro.Add("Sonido al recoger chupeta: DM-CGS-41");
        }

        var clipDanio = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaSonidoDanio);
        if (gm.sonidoDanio == null) gm.sonidoDanio = gm.gameObject.AddComponent<AudioSource>();
        gm.sonidoDanio.clip = clipDanio;
        gm.sonidoDanio.volume = 0.8f;
        gm.sonidoDanio.playOnAwake = false;
        gm.sonidoDanio.loop = false;
        EditorUtility.SetDirty(gm.sonidoDanio);
        EditorUtility.SetDirty(gm);
        registro.Add("Sonido al perder vida: DM-CGS-02");
    }

    // ---------------------------------------------------------------- diseno

    private static void AjustarDiseno(List<string> registro)
    {
        Sprite cuadrado = null;
        Sprite circulo = null;
        var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
        foreach (var sr in renderers)
        {
            if (sr == null) continue;
            if (cuadrado == null && sr.name.StartsWith("Plataforma 1")) cuadrado = sr.sprite;
            if (circulo == null && sr.name == "Circle") circulo = sr.sprite;
        }

        int decoradas = 0;
        foreach (var sr in renderers)
        {
            // los glaseados viejos se borran al redecorar su plataforma
            if (sr == null || sr.name == "Glaseado" || sr.name == "Sombra") continue;

            if (sr.name == "Plataforma Movil" && cuadrado != null) sr.sprite = cuadrado;

            if (!Paleta(sr.name, out Color baseC, out Color glaseado)) continue;
            Decorar(sr, baseC, glaseado);
            decoradas++;
        }
        registro.Add("Plataformas pintadas con glaseado y sombra: " + decoradas);

        var agua = GameObject.Find("Agua");
        if (agua != null && agua.TryGetComponent(out SpriteRenderer srAgua))
        {
            srAgua.color = new Color(0.3f, 0.65f, 1f, 0.8f);
            EditorUtility.SetDirty(srAgua);
        }

        var fondo = GameObject.Find("Fondo - 001_0");
        if (fondo != null && fondo.TryGetComponent(out SpriteRenderer srFondo))
        {
            srFondo.color = new Color(1f, 0.9f, 0.97f);
            srFondo.sortingOrder = -10;
            EditorUtility.SetDirty(srFondo);
        }

        CrearNubes(circulo ?? cuadrado);
        registro.Add("Nubes flotantes agregadas");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaChupeta);
        var spriteChupeta = prefab != null ? prefab.GetComponent<SpriteRenderer>().sprite : null;
        if (spriteChupeta != null)
        {
            CrearChupetasGigantes(spriteChupeta);
            registro.Add("Chupetas gigantes de fondo agregadas");
        }
    }

    private static bool Paleta(string nombre, out Color baseC, out Color glaseado)
    {
        baseC = glaseado = Color.white;
        if (nombre.StartsWith("Plataforma 1")) { baseC = Hex(0xF8, 0x78, 0xA8); glaseado = Hex(0xFF, 0xE4, 0xEE); }
        else if (nombre.StartsWith("Plataforma 2")) { baseC = Hex(0x5C, 0xD6, 0xB0); glaseado = Hex(0xE0, 0xFF, 0xF4); }
        else if (nombre.StartsWith("Plataforma 3")) { baseC = Hex(0xFF, 0xC7, 0x54); glaseado = Hex(0xFF, 0xF6, 0xD8); }
        else if (nombre == "Plataforma compuesta") { baseC = Hex(0xB0, 0x8C, 0xEE); glaseado = Hex(0xF1, 0xE8, 0xFF); }
        else if (nombre == "Plataforma Movil") { baseC = Hex(0xE8, 0x4A, 0x5F); glaseado = Hex(0xFF, 0xF0, 0xF0); }
        else if (nombre == "Imagen" || nombre.StartsWith("Borde Derecho") || nombre.StartsWith("Isla") || nombre == "Muro Final") { baseC = Hex(0x6B, 0x40, 0x2C); glaseado = Hex(0xFF, 0x9C, 0xC2); }
        else if (nombre == "Muro 1") { baseC = Hex(0xD9, 0x8C, 0x40); glaseado = Hex(0xFF, 0xF1, 0xD6); }
        else return false;
        return true;
    }

    private static void Decorar(SpriteRenderer sr, Color baseC, Color colorGlaseado)
    {
        sr.color = baseC;
        EditorUtility.SetDirty(sr);

        Transform t = sr.transform;
        BorrarHijo(t, "Glaseado");
        BorrarHijo(t, "Sombra");
        if (sr.sprite == null) return;

        Vector3 escala = t.lossyScale;
        float alto = sr.sprite.bounds.size.y;
        float altoMundo = Mathf.Abs(escala.y) * alto;
        if (altoMundo < 0.01f || Mathf.Abs(escala.x) < 0.0001f) return;

        // capa clara arriba, como un glaseado de pastel
        float grosor = Mathf.Min(0.2f, altoMundo * 0.45f);
        float fraccion = grosor / altoMundo;
        var glaseado = CrearHijo(t, "Glaseado", sr.sprite, colorGlaseado, sr.sortingLayerID, sr.sortingOrder + 1);
        glaseado.localScale = new Vector3(1.03f, fraccion, 1f);
        glaseado.localPosition = new Vector3(0f, alto * 0.5f - alto * fraccion * 0.5f + 0.03f / Mathf.Abs(escala.y), 0f);

        // sombra suave desplazada hacia abajo
        var sombra = CrearHijo(t, "Sombra", sr.sprite, new Color(0.15f, 0.05f, 0.2f, 0.25f), sr.sortingLayerID, sr.sortingOrder - 1);
        sombra.localScale = Vector3.one;
        sombra.localPosition = new Vector3(0.1f / Mathf.Abs(escala.x), -0.14f / Mathf.Abs(escala.y), 0f);
    }

    private static void CrearNubes(Sprite sprite)
    {
        var viejo = GameObject.Find(NombreNubes);
        if (viejo != null) Object.DestroyImmediate(viejo);
        if (sprite == null) return;

        var padre = new GameObject(NombreNubes).transform;
        var posiciones = new[]
        {
            new Vector2(-16f, 15f), new Vector2(-6f, 11f), new Vector2(3f, 16f),
            new Vector2(11f, 12f), new Vector2(19f, 15.5f), new Vector2(27f, 11.5f),
            new Vector2(35f, 14f), new Vector2(42f, 9f), new Vector2(-12f, 5f),
            new Vector2(15f, 4f), new Vector2(31f, 6f), new Vector2(50f, 15f),
            new Vector2(58f, 10f), new Vector2(66f, 16f), new Vector2(74f, 12f),
            new Vector2(82f, 15.5f), new Vector2(96f, 13f), new Vector2(104f, 16f),
            new Vector2(110f, 10f), new Vector2(60f, 5f), new Vector2(100f, 6f)
        };
        var partes = new[]
        {
            new Vector3(0f, 0f, 1.6f), new Vector3(-0.95f, -0.25f, 1.15f),
            new Vector3(0.95f, -0.3f, 1.1f), new Vector3(0.35f, 0.45f, 1.0f)
        };

        Random.InitState(7);
        for (int i = 0; i < posiciones.Length; i++)
        {
            var nube = new GameObject("Nube " + (i + 1)).transform;
            nube.SetParent(padre, false);
            nube.position = new Vector3(posiciones[i].x, posiciones[i].y, 0f);
            float s = Random.Range(0.9f, 1.6f);
            nube.localScale = new Vector3(s, s, 1f);

            float alfa = posiciones[i].y < 8f ? 0.55f : 0.9f;
            foreach (var p in partes)
            {
                var bola = CrearHijo(nube, "Bola", sprite, new Color(1f, 1f, 1f, alfa), 0, -6);
                bola.localPosition = new Vector3(p.x, p.y, 0f);
                bola.localScale = new Vector3(p.z, p.z, 1f);
            }

            var mov = nube.gameObject.AddComponent<NubeFlotante>();
            mov.velocidad = Random.Range(0.15f, 0.45f);
            mov.limiteIzquierdo = -30f;
            mov.limiteDerecho = LimiteDerechoNivel + 10f;
        }
    }

    private static void CrearChupetasGigantes(Sprite sprite)
    {
        var viejo = GameObject.Find(NombreGigantes);
        if (viejo != null) Object.DestroyImmediate(viejo);

        var padre = new GameObject(NombreGigantes).transform;
        var datos = new[]
        {
            new Vector4(-15f, 0.5f, 0.55f, 8f), new Vector4(-4f, -1.8f, 0.45f, -6f),
            new Vector4(6f, 9.5f, 0.5f, 10f), new Vector4(14f, -1.2f, 0.6f, -9f),
            new Vector4(23f, 10f, 0.45f, 7f), new Vector4(30f, 1.5f, 0.55f, -5f),
            new Vector4(39f, 3f, 0.65f, 6f), new Vector4(-10f, 11f, 0.4f, -8f),
            new Vector4(48f, 4f, 0.55f, 6f), new Vector4(62f, 9.5f, 0.5f, -7f),
            new Vector4(78f, 11f, 0.6f, 8f), new Vector4(95f, 7f, 0.5f, -6f),
            new Vector4(104f, 9f, 0.65f, 5f)
        };

        for (int i = 0; i < datos.Length; i++)
        {
            var go = new GameObject("Chupeta Gigante " + (i + 1));
            go.transform.SetParent(padre, false);
            go.transform.position = new Vector3(datos[i].x, datos[i].y, 0f);
            go.transform.localScale = new Vector3(datos[i].z, datos[i].z, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, datos[i].w);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.35f);
            sr.sortingOrder = -4;
            var anim = go.AddComponent<ChupetaAnimada>();
            anim.altura = 0.25f;
            anim.velocidad = 0.8f;
            anim.giro = 5f;
        }
    }

    // ---------------------------------------------------------------- utilidades

    private static Transform CrearHijo(Transform padre, string nombre, Sprite sprite, Color color, int capa, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingLayerID = capa;
        sr.sortingOrder = orden;
        return go.transform;
    }

    private static void BorrarHijo(Transform padre, string nombre)
    {
        for (int i = padre.childCount - 1; i >= 0; i--)
            if (padre.GetChild(i).name == nombre)
                Object.DestroyImmediate(padre.GetChild(i).gameObject);
    }

    private static Color Hex(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);

    private static string ObjetoDe(SerializedObject so, string ruta)
    {
        var p = so.FindProperty(ruta);
        return p != null && p.objectReferenceValue != null ? p.objectReferenceValue.name : "(nadie)";
    }

    private static void PonerFloat(SerializedObject so, string ruta, float valor)
    {
        var p = so.FindProperty(ruta);
        if (p != null) p.floatValue = valor;
        else Debug.LogWarning("No se encontro la propiedad " + ruta);
    }

    private static void PonerBool(SerializedObject so, string ruta, bool valor)
    {
        var p = so.FindProperty(ruta);
        if (p != null) p.boolValue = valor;
        else Debug.LogWarning("No se encontro la propiedad " + ruta);
    }

    private static void PonerVector2(SerializedObject so, string ruta, Vector2 valor)
    {
        var p = so.FindProperty(ruta);
        if (p != null) p.vector2Value = valor;
        else Debug.LogWarning("No se encontro la propiedad " + ruta);
    }

    private static void PonerVector3(SerializedObject so, string ruta, Vector3 valor)
    {
        var p = so.FindProperty(ruta);
        if (p != null) p.vector3Value = valor;
        else Debug.LogWarning("No se encontro la propiedad " + ruta);
    }
}
