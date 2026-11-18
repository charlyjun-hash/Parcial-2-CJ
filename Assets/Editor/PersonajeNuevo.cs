using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Menu "Gum Adventure > Cambiar personaje y recolectables": dibuja por codigo un
// personaje original ("Chicle") y dos recolectables (dona y cupcake),
// crea sus animaciones y Animator Controllers, y los pone en el juego en lugar de
// las imagenes anteriores. Se puede ejecutar varias veces.
public static class PersonajeNuevo
{
    private const string CarpetaPersonaje = "Assets/Personaje Chicle";
    private const string CarpetaRecolectables = "Assets/Recolectables Dulces";
    private const string CarpetaDecoracion = "Assets/Decoracion Chicle";
    private const string NombreJuego = "GUM ADVENTURE";
    private const string HojaVieja = "Assets/imagenes/recolectables.png";
    private static readonly KeyValuePair<string, Color>[] ColoresBola =
    {
        new("rosa", Paleta.Rosa), new("menta", Paleta.Menta), new("lila", Paleta.Lila),
        new("limon", Paleta.Limon), new("agua", Paleta.Agua)
    };
    private const string PrefabChupeta1 = "Assets/Prefabricados/Recolectable _ Chupeta1.prefab";
    private const string PrefabChupeta2 = "Assets/Prefabricados/Recolectable _ Chupeta2.prefab";
    private static readonly string[] Escenas = { "Assets/escenas/nivel2.unity", "Assets/escenas/inicio.unity", "Assets/escenas/nivel.unity" };

    private static readonly Color Contorno = Paleta.Ciruela;

    [MenuItem("Gum Adventure/Cambiar personaje y recolectables")]
    public static void Cambiar()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var registro = new List<string>();

        // ---- personaje
        var animPersonaje = new Dictionary<string, Sprite[]>
        {
            { "Ocio", FramesPersonaje("ocio", 6, PoseOcio) },
            { "Caminar", FramesPersonaje("caminar", 6, PoseCaminar) },
            { "Correr", FramesPersonaje("correr", 6, PoseCorrer) },
            { "Saltar", FramesPersonaje("saltar", 2, PoseSaltar) },
            { "Caer", FramesPersonaje("caer", 2, PoseCaer) },
            { "Agachar", FramesPersonaje("agachar", 2, PoseAgachar) }
        };
        var fps = new Dictionary<string, float> { { "Ocio", 7f }, { "Caminar", 12f }, { "Correr", 16f }, { "Saltar", 8f }, { "Caer", 8f }, { "Agachar", 4f } };
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var par in animPersonaje)
            clips[par.Key] = Clip(CarpetaPersonaje + "/Animaciones/Chicle - " + par.Key + ".anim", par.Value, fps[par.Key]);
        var controlPersonaje = ControladorPersonaje(clips);
        registro.Add("Personaje 'Chicle': 6 animaciones (Ocio, Caminar, Correr, Saltar, Caer, Agachar) en " + CarpetaPersonaje);

        // ---- recolectables: dona y cupcake (ya no son chupetas)
        var dona = FramesRecolectable("dona", 8, DibujarDona);
        var cupcake = FramesRecolectable("cupcake", 8, DibujarCupcake);
        var controlDona = ControladorSimple(CarpetaRecolectables + "/Animaciones/Dona.controller",
            Clip(CarpetaRecolectables + "/Animaciones/Dona - Girar.anim", dona, 12f), "Girar");
        var controlCupcake = ControladorSimple(CarpetaRecolectables + "/Animaciones/Cupcake.controller",
            Clip(CarpetaRecolectables + "/Animaciones/Cupcake - Rebotar.anim", cupcake, 10f), "Rebotar");
        ActualizarPrefab(PrefabChupeta1, "dona_0", controlDona);
        ActualizarPrefab(PrefabChupeta2, "cupcake_0", controlCupcake);

        foreach (var viejo in new[] { "Remolino.controller", "Remolino - Girar.anim", "Caramelo.controller", "Caramelo - Brillar.anim" })
            AssetDatabase.DeleteAsset(CarpetaRecolectables + "/Animaciones/" + viejo);
        for (int i = 1; i < 8; i++) AssetDatabase.DeleteAsset(CarpetaRecolectables + "/Sprites/remolino_" + i + ".png");
        for (int i = 0; i < 8; i++) AssetDatabase.DeleteAsset(CarpetaRecolectables + "/Sprites/caramelo_" + i + ".png");
        registro.Add("Recolectables: dona y cupcake, cada uno con su Animator, en " + CarpetaRecolectables);

        // ---- decoracion del fondo: bolas de chicle de colores (ya no hay chupetas)
        foreach (var par in ColoresBola)
        {
            var l = new Lienzo(LadoRecolectable, LadoRecolectable);
            DibujarBolaChicle(l, par.Value);
            Guardar(l, CarpetaDecoracion + "/bola_chicle_" + par.Key + ".png", 40f, SpriteAlignment.Center);
        }
        AssetDatabase.SaveAssets();
        registro.Add("Fondo: bolas de chicle de " + ColoresBola.Length + " colores en " + CarpetaDecoracion);

        // ---- escenas
        foreach (var ruta in Escenas)
        {
            if (!File.Exists(ruta)) continue;
            var escena = EditorSceneManager.OpenScene(ruta);
            // se recargan desde disco: las referencias de antes pueden quedar viejas tras tantas importaciones
            var bolas = new Sprite[ColoresBola.Length];
            for (int i = 0; i < bolas.Length; i++)
                bolas[i] = AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaDecoracion + "/bola_chicle_" + ColoresBola[i].Key + ".png");
            int cambios = ReemplazarEnEscena(
                bolas,
                AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaRecolectables + "/Sprites/dona_0.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaPersonaje + "/Sprites/ocio_0.png"),
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CarpetaPersonaje + "/Animaciones/Chicle.controller"));
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            registro.Add(Path.GetFileNameWithoutExtension(ruta) + ": " + cambios + " cambios");
        }
        // la chupeta remolino ya no se usa en ningun lado
        AssetDatabase.DeleteAsset(CarpetaRecolectables + "/Sprites/remolino_0.png");
        EditorSceneManager.OpenScene(Escenas[0]);
        Debug.Log("Personaje y recolectables nuevos:\n- " + string.Join("\n- ", registro));
    }

    // ================================================================ escenas y prefabs

    private static int ReemplazarEnEscena(Sprite[] bolas, Sprite iconoContador, Sprite personaje, RuntimeAnimatorController control)
    {
        int cambios = 0, siguienteBola = 0;
        // toda chupeta de decoracion (la vieja o la remolino) pasa a una bola de chicle, alternando colores
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (!EsChupetaDecorativa(sr.sprite)) continue;
            sr.sprite = bolas[siguienteBola++ % bolas.Length];
            sr.color = new Color(1f, 1f, 1f, sr.color.a);
            EditorUtility.SetDirty(sr);
            cambios++;
        }
        foreach (var img in Object.FindObjectsByType<Image>(FindObjectsInactive.Include))
        {
            // el icono del contador del HUD muestra lo que se recoge: la dona
            bool esContador = img.name == "Icono Chupeta";
            if (!esContador && !EsChupetaDecorativa(img.sprite)) continue;
            img.sprite = esContador ? iconoContador : bolas[siguienteBola++ % bolas.Length];
            img.color = new Color(1f, 1f, 1f, img.color.a);
            img.preserveAspect = true;
            EditorUtility.SetDirty(img);
            cambios++;
        }
        // titulo del menu
        foreach (var texto in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            if (texto.text != "LOLLIPOPS" && texto.text != NombreJuego) continue;
            texto.text = NombreJuego;
            // el nombre es mas largo: se achica solo si no cabe, en una sola linea
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            texto.fontSize = 112f; // un poco mas chico para no tapar la decoracion de los lados
            texto.fontSizeMax = 112f;
            texto.fontSizeMin = 40f;
            texto.enableAutoSizing = true;
            EditorUtility.SetDirty(texto);
            cambios++;
        }
        // textos que decian "chupetas" ahora dicen "dulces"
        foreach (var texto in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
        {
            string nuevo = texto.text.Replace("todas las chupetas", "todos los dulces").Replace("chupetas", "dulces").Replace("CHUPETAS", "DULCES").Replace("Chupetas", "Dulces");
            if (nuevo == texto.text) continue;
            texto.text = nuevo;
            EditorUtility.SetDirty(texto);
            cambios++;
        }

        foreach (var cp in Object.FindObjectsByType<ControlPersonaje>(FindObjectsInactive.Include))
        {
            if (cp.TryGetComponent(out SpriteRenderer sr))
            {
                sr.sprite = personaje;
                EditorUtility.SetDirty(sr);
            }
            if (cp.TryGetComponent(out Animator an))
            {
                an.runtimeAnimatorController = control;
                EditorUtility.SetDirty(an);
            }
            cambios++;
        }
        return cambios;
    }

    private static bool EsChupetaDecorativa(Sprite sprite)
    {
        if (sprite == null) return false;
        string ruta = AssetDatabase.GetAssetPath(sprite);
        return ruta == HojaVieja || ruta == CarpetaRecolectables + "/Sprites/remolino_0.png";
    }

    private static void ActualizarPrefab(string ruta, string archivoSprite, RuntimeAnimatorController control)
    {
        var raiz = PrefabUtility.LoadPrefabContents(ruta);
        raiz.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaRecolectables + "/Sprites/" + archivoSprite + ".png");
        if (!raiz.TryGetComponent(out Animator an)) an = raiz.AddComponent<Animator>();
        an.runtimeAnimatorController = control;
        PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        PrefabUtility.UnloadPrefabContents(raiz);
    }

    // ================================================================ animaciones

    private static void AsegurarCarpeta(string carpeta)
    {
        carpeta = carpeta.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(carpeta)) return;
        string padre = Path.GetDirectoryName(carpeta).Replace('\\', '/');
        AsegurarCarpeta(padre);
        AssetDatabase.CreateFolder(padre, Path.GetFileName(carpeta));
    }

    private static AnimationClip Clip(string ruta, Sprite[] frames, float fps)
    {
        AsegurarCarpeta(Path.GetDirectoryName(ruta));
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, ruta);
        }
        clip.ClearCurves();
        clip.frameRate = fps;

        var claves = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++) claves[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        // el ultimo cuadro se repite al final para que dure lo mismo que los demas
        claves[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), claves);

        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    // Mismos parametros que manda MovimientoJugador. Las transiciones salen de
    // "Any State" sin tiempo de salida, asi el cambio de animacion es inmediato.
    private static RuntimeAnimatorController ControladorPersonaje(Dictionary<string, AnimationClip> clips)
    {
        string ruta = CarpetaPersonaje + "/Animaciones/Chicle.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ruta) != null) AssetDatabase.DeleteAsset(ruta);
        var control = AnimatorController.CreateAnimatorControllerAtPath(ruta);

        foreach (var nombre in new[] { "Ocio", "Caminar", "Correr", "Agachar", "EnSuelo" })
            control.AddParameter(nombre, AnimatorControllerParameterType.Bool);
        foreach (var nombre in new[] { "VelocidadX", "VelocidadY" })
            control.AddParameter(nombre, AnimatorControllerParameterType.Float);

        var sm = control.layers[0].stateMachine;
        var estados = new Dictionary<string, AnimatorState>();
        var posiciones = new Dictionary<string, Vector3>
        {
            { "Ocio", new Vector3(300, 0) }, { "Caminar", new Vector3(300, 80) }, { "Correr", new Vector3(300, 160) },
            { "Agachar", new Vector3(560, 0) }, { "Saltar", new Vector3(560, 80) }, { "Caer", new Vector3(560, 160) }
        };
        foreach (var par in posiciones)
        {
            var e = sm.AddState(par.Key, par.Value);
            e.motion = clips[par.Key];
            estados[par.Key] = e;
        }
        sm.defaultState = estados["Ocio"];

        Regla(sm, estados["Ocio"], ("EnSuelo", true), ("Caminar", false), ("Agachar", false));
        Regla(sm, estados["Caminar"], ("EnSuelo", true), ("Caminar", true), ("Correr", false), ("Agachar", false));
        Regla(sm, estados["Correr"], ("EnSuelo", true), ("Correr", true), ("Agachar", false));
        Regla(sm, estados["Agachar"], ("EnSuelo", true), ("Agachar", true));
        var subir = Regla(sm, estados["Saltar"], ("EnSuelo", false));
        subir.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocidadY");
        var bajar = Regla(sm, estados["Caer"], ("EnSuelo", false));
        bajar.AddCondition(AnimatorConditionMode.Less, 0.1f, "VelocidadY");

        EditorUtility.SetDirty(control);
        return control;
    }

    private static AnimatorStateTransition Regla(AnimatorStateMachine sm, AnimatorState destino, params (string, bool)[] condiciones)
    {
        var t = sm.AddAnyStateTransition(destino);
        t.hasExitTime = false;
        t.duration = 0f;
        t.canTransitionToSelf = false;
        foreach (var (nombre, valor) in condiciones)
            t.AddCondition(valor ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, nombre);
        return t;
    }

    private static RuntimeAnimatorController ControladorSimple(string ruta, AnimationClip clip, string estado)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ruta) != null) AssetDatabase.DeleteAsset(ruta);
        var control = AnimatorController.CreateAnimatorControllerAtPath(ruta);
        var e = control.layers[0].stateMachine.AddState(estado, new Vector3(300, 0));
        e.motion = clip;
        control.layers[0].stateMachine.defaultState = e;
        EditorUtility.SetDirty(control);
        return control;
    }

    // ================================================================ personaje: poses

    private class Pose
    {
        public float bob, sx = 1f, sy = 1f, inclinacion;
        public Vector2 pieI, pieD;
        public float brazoI = 10f, brazoD = 10f;
        public float parpadeo, boca, antena;
    }

    private static Pose PoseOcio(int i, int n)
    {
        float s = Mathf.Sin(i / (float)n * Mathf.PI * 2f);
        return new Pose { sy = 1f + 0.03f * s, sx = 1f - 0.018f * s, brazoI = 8f + 5f * s, brazoD = 8f + 5f * s, parpadeo = i == 4 ? 1f : 0f, antena = 3f * s };
    }

    private static Pose PoseCaminar(int i, int n)
    {
        float f = i / (float)n * Mathf.PI * 2f;
        float s = Mathf.Sin(f), c = Mathf.Cos(f);
        return new Pose
        {
            bob = 2f + 2f * Mathf.Cos(2f * f), inclinacion = 5f,
            pieI = new Vector2(13f * s, Mathf.Max(0f, 9f * c)), pieD = new Vector2(-13f * s, Mathf.Max(0f, -9f * c)),
            brazoI = 10f - 28f * s, brazoD = 10f + 28f * s, antena = -4f * Mathf.Sin(2f * f)
        };
    }

    private static Pose PoseCorrer(int i, int n)
    {
        float f = i / (float)n * Mathf.PI * 2f;
        float s = Mathf.Sin(f), c = Mathf.Cos(f);
        return new Pose
        {
            bob = 3f + 4f * Mathf.Cos(2f * f), inclinacion = 14f, sx = 1.02f, sy = 0.98f,
            pieI = new Vector2(21f * s, Mathf.Max(0f, 14f * c)), pieD = new Vector2(-21f * s, Mathf.Max(0f, -14f * c)),
            brazoI = 15f - 50f * s, brazoD = 15f + 50f * s, boca = 0.6f, antena = -10f - 5f * Mathf.Sin(2f * f)
        };
    }

    private static Pose PoseSaltar(int i, int n)
    {
        return new Pose
        {
            sy = i == 0 ? 1.08f : 1.05f, sx = i == 0 ? 0.93f : 0.95f, bob = 4f,
            pieI = new Vector2(5f, 10f), pieD = new Vector2(-5f, 7f),
            brazoI = i == 0 ? 155f : 140f, brazoD = i == 0 ? 150f : 138f, boca = 0.7f, antena = -12f
        };
    }

    private static Pose PoseCaer(int i, int n)
    {
        return new Pose
        {
            sy = 0.98f, sx = 1.02f, bob = 3f,
            pieI = new Vector2(-3f, 3f), pieD = new Vector2(3f, 6f),
            brazoI = i == 0 ? 75f : 85f, brazoD = i == 0 ? 85f : 75f, boca = 0.45f, antena = i == 0 ? 10f : 14f
        };
    }

    private static Pose PoseAgachar(int i, int n)
    {
        return new Pose
        {
            sy = i == 0 ? 0.74f : 0.72f, sx = i == 0 ? 1.13f : 1.15f,
            pieI = new Vector2(-9f, 0f), pieD = new Vector2(9f, 0f),
            brazoI = 25f, brazoD = 25f, antena = i == 0 ? 2f : -2f
        };
    }

    // ================================================================ personaje: dibujo

    private const int LadoPersonaje = 256;
    private const float Suelo = 6f;

    private static Sprite[] FramesPersonaje(string nombre, int cantidad, Func<int, int, Pose> pose)
    {
        var frames = new Sprite[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            var l = new Lienzo(LadoPersonaje, LadoPersonaje);
            DibujarChicle(l, pose(i, cantidad));
            frames[i] = Guardar(l, CarpetaPersonaje + "/Sprites/" + nombre + "_" + i + ".png", 100f, SpriteAlignment.BottomCenter);
        }
        return frames;
    }

    private static void DibujarChicle(Lienzo l, Pose p)
    {
        Color cuerpo = Paleta.Rosa;
        Color brazos = Color.Lerp(Paleta.Rosa, Paleta.Fresa, 0.3f);
        Color panza = Color.Lerp(Paleta.Rosa, Paleta.Crema, 0.65f);
        Color pies = Paleta.Lila;
        const float borde = 4f;

        Vector2 T(float x, float y)
        {
            float nx = 128f + (x - 128f) * p.sx;
            float ny = Suelo + 20f + (y - Suelo - 20f) * p.sy + p.bob;
            nx += p.inclinacion * (ny - Suelo) / 180f;
            return new Vector2(nx, ny);
        }

        // pies (quedan en el suelo, no se aplastan con el cuerpo)
        foreach (var (baseX, off) in new[] { (104f, p.pieI), (152f, p.pieD) })
        {
            var c = new Vector2(128f + (baseX - 128f) * p.sx + off.x, Suelo + 13f + off.y);
            l.ElipseBorde(c, new Vector2(22f, 13f), pies, Contorno, borde);
        }

        // brazo de atras
        Brazo(l, T(84f, 112f), -1f, p.brazoI, brazos, borde);

        // antena con caramelo
        Vector2 baseAntena = T(128f, 176f);
        Vector2 punta = baseAntena + new Vector2(p.antena, 36f * p.sy);
        l.CapsulaBorde(baseAntena, punta, 3.5f, Paleta.Crema, Contorno, 3f);
        l.ElipseBorde(punta + new Vector2(0f, 8f), new Vector2(15f, 15f), Paleta.Limon, Contorno, 3.5f);
        l.Elipse(punta + new Vector2(0f, 8f), new Vector2(8f, 8f), Paleta.Rosa);
        l.Elipse(punta + new Vector2(0f, 8f), new Vector2(3.5f, 3.5f), Paleta.Limon);

        // cuerpo, panza y brillo
        l.ElipseBorde(T(128f, 108f), new Vector2(58f * p.sx, 76f * p.sy), cuerpo, Contorno, borde);
        l.Elipse(T(131f, 88f), new Vector2(38f * p.sx, 40f * p.sy), panza);
        l.Elipse(T(98f, 158f), new Vector2(10f, 17f), Paleta.ConAlfa(Color.white, 0.35f), 25f);

        // ojos
        foreach (var ox in new[] { 112f, 150f })
        {
            Vector2 ojo = T(ox, 140f);
            if (p.parpadeo > 0.5f)
            {
                l.Capsula(ojo + new Vector2(-10f, 0f), ojo + new Vector2(10f, 0f), 2.6f, Contorno);
                continue;
            }
            l.ElipseBorde(ojo, new Vector2(13f, 16f), Color.white, Contorno, 3f);
            Vector2 pupila = ojo + new Vector2(3.5f, -1f);
            l.Elipse(pupila, new Vector2(7.5f, 8.5f), Contorno);
            l.Elipse(pupila + new Vector2(-2.5f, 3.5f), new Vector2(2.8f, 2.8f), Color.white);
        }

        // cachetes
        l.Elipse(T(96f, 121f), new Vector2(9f, 6f), Paleta.ConAlfa(Paleta.Fresa, 0.45f));
        l.Elipse(T(166f, 121f), new Vector2(9f, 6f), Paleta.ConAlfa(Paleta.Fresa, 0.45f));

        // boca: sonrisa o boca abierta
        Vector2 boca = T(133f, 121f);
        if (p.boca < 0.3f)
        {
            Vector2 previo = boca + new Vector2(-9f, 0f);
            for (int k = 1; k <= 8; k++)
            {
                float a = Mathf.PI + k / 8f * Mathf.PI;
                var siguiente = boca + new Vector2(Mathf.Cos(a) * 9f, Mathf.Sin(a) * 6f);
                l.Capsula(previo, siguiente, 2.2f, Contorno);
                previo = siguiente;
            }
        }
        else
        {
            l.Elipse(boca + new Vector2(0f, -3f), new Vector2(6.5f, 7f * p.boca + 2f), Contorno);
            l.Elipse(boca + new Vector2(0f, -6f * p.boca), new Vector2(4f, 2.5f), Paleta.Fresa);
        }

        // brazo de adelante
        Brazo(l, T(172f, 112f), 1f, p.brazoD, brazos, borde);
    }

    private static void Brazo(Lienzo l, Vector2 hombro, float lado, float angulo, Color color, float borde)
    {
        float a = angulo * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Sin(a) * lado, -Mathf.Cos(a));
        Vector2 mano = hombro + dir * 26f;
        l.CapsulaBorde(hombro, mano, 9f, color, Contorno, borde);
        l.ElipseBorde(mano, new Vector2(10.5f, 10.5f), color, Contorno, borde);
    }

    // ================================================================ recolectables

    private const int LadoRecolectable = 128;

    private static Sprite[] FramesRecolectable(string nombre, int cantidad, Action<Lienzo, int, int> dibujar)
    {
        var frames = new Sprite[cantidad];
        for (int i = 0; i < cantidad; i++) frames[i] = GuardarRecolectable(nombre + "_" + i, dibujar, i, cantidad);
        return frames;
    }

    private static Sprite GuardarRecolectable(string archivo, Action<Lienzo, int, int> dibujar, int i, int n)
    {
        var l = new Lienzo(LadoRecolectable, LadoRecolectable);
        dibujar(l, i, n);
        return Guardar(l, CarpetaRecolectables + "/Sprites/" + archivo + ".png", 40f, SpriteAlignment.Center);
    }

    // bola de chicle brillante para el fondo: sombra abajo, brillos arriba y motitas de azucar
    private static void DibujarBolaChicle(Lienzo l, Color color)
    {
        var c = new Vector2(64f, 64f);
        const float r = 50f;
        l.ElipseBorde(c, new Vector2(r, r), color, Contorno, 4f);

        // sombra en media luna abajo a la derecha
        var luz = c + new Vector2(-14f, 14f);
        l.Forma(new Rect(c.x - r, c.y - r, r * 2f, r * 2f), (x, y) =>
        {
            var p = new Vector2(x, y);
            return (p - c).sqrMagnitude <= r * r && (p - luz).sqrMagnitude > 54f * 54f;
        }, Paleta.ConAlfa(Color.Lerp(color, Paleta.Ciruela, 0.35f), 0.75f));

        // motitas de azucar
        Vector2[] motas = { new(-24f, -18f), new(10f, -30f), new(28f, 4f), new(-6f, 22f), new(22f, 30f), new(-32f, 8f), new(4f, -6f) };
        foreach (var m in motas) l.Elipse(c + m, new Vector2(2.6f, 2.6f), Paleta.ConAlfa(Color.Lerp(color, Color.white, 0.6f), 0.9f));

        // brillos
        l.Elipse(c + new Vector2(-20f, 22f), new Vector2(17f, 10f), Paleta.ConAlfa(Color.white, 0.55f), 40f);
        l.Elipse(c + new Vector2(-24f, 26f), new Vector2(7f, 4f), Paleta.ConAlfa(Color.white, 0.95f), 40f);
        l.Elipse(c + new Vector2(-34f, 6f), new Vector2(3.5f, 3.5f), Paleta.ConAlfa(Color.white, 0.85f));
    }

    // dona glaseada: el glaseado y las chispas giran, la masa se queda quieta
    private static readonly Color Masa = new Color(0.91f, 0.66f, 0.38f);
    private static readonly Color MasaOscura = new Color(0.78f, 0.50f, 0.27f);

    private static void DibujarDona(Lienzo l, int i, int n)
    {
        var c = new Vector2(64f, 64f);
        // un cuarto de vuelta por ciclo: el glaseado (8 ondas) y las chispas (patron cada 3) quedan igual y la animacion empalma
        float giro = i / (float)n * Mathf.PI * 0.5f;
        Anillo(l, c, 13f, 48f, Contorno);
        Anillo(l, c, 16.5f, 44.5f, Masa);
        // sombrita de la masa en la parte de abajo
        l.Forma(new Rect(c.x - 45f, c.y - 45f, 90f, 90f), (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            return d > 16.5f && d < 44.5f && y < c.y - 18f;
        }, Paleta.ConAlfa(MasaOscura, 0.6f));

        // glaseado rosado con borde ondulado
        Func<float, float, bool> glaseado = (x, y) =>
        {
            Vector2 p = new Vector2(x, y) - c;
            float d = p.magnitude, a = Mathf.Atan2(p.y, p.x);
            return d > 19f && d < 37f + 4f * Mathf.Sin(a * 8f - giro * 8f);
        };
        l.Forma(new Rect(c.x - 45f, c.y - 45f, 90f, 90f), (x, y) =>
        {
            Vector2 p = new Vector2(x, y) - c;
            float d = p.magnitude, a = Mathf.Atan2(p.y, p.x);
            return d > 16.5f && d < 40f + 4f * Mathf.Sin(a * 8f - giro * 8f);
        }, Contorno);
        Anillo(l, c, 16.5f, 19.5f, Masa);
        l.Forma(new Rect(c.x - 45f, c.y - 45f, 90f, 90f), glaseado, Paleta.Rosa);
        Anillo(l, c, 13f, 16.5f, Contorno);

        // chispas de colores
        // todo depende de k % 3, asi el patron se repite cada cuarto de vuelta
        Color[] colores = { Paleta.Menta, Paleta.Limon, Paleta.Crema };
        for (int k = 0; k < 12; k++)
        {
            int m = k % 3;
            float a = k / 12f * Mathf.PI * 2f + giro + m * 0.12f;
            float r = 24f + m * 4.5f;
            Vector2 p = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            float ang = a + 0.9f + m * 1.3f;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 3.5f;
            l.Capsula(p - dir, p + dir, 2.2f, colores[m]);
        }
        l.Elipse(c + new Vector2(-20f, 22f), new Vector2(9f, 4f), Paleta.ConAlfa(Color.white, 0.6f), 40f);
        Destello(l, c + new Vector2(34f, 32f), i, n);
    }

    private static void Anillo(Lienzo l, Vector2 c, float dentro, float fuera, Color color)
    {
        l.Forma(new Rect(c.x - fuera, c.y - fuera, fuera * 2f, fuera * 2f), (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            return d >= dentro && d <= fuera;
        }, color);
    }

    // cupcake: el betun se aplasta y se estira, la cereza rebota
    private static void DibujarCupcake(Lienzo l, int i, int n)
    {
        float t = i / (float)n * Mathf.PI * 2f;
        float s = 1f + 0.07f * Mathf.Sin(t);           // estiramiento del betun
        float salto = 4f * Mathf.Max(0f, Mathf.Sin(t)); // la cereza sube

        // capacillo con rayas
        Func<float, float, float, bool> capacillo = (x, y, extra) =>
        {
            if (y < 8f - extra || y > 48f + extra) return false;
            float k = (y - 8f) / 40f;
            float medio = Mathf.Lerp(27f, 37f, k) + extra;
            return Mathf.Abs(x - 64f) <= medio;
        };
        l.Forma(new Rect(20f, 2f, 88f, 52f), (x, y) => capacillo(x, y, 3.5f), Contorno);
        l.Forma(new Rect(20f, 2f, 88f, 52f), (x, y) => capacillo(x, y, 0f), Paleta.Lila);
        l.Forma(new Rect(20f, 2f, 88f, 52f), (x, y) =>
        {
            if (!capacillo(x, y, 0f)) return false;
            float k = (y - 8f) / 40f;
            float u = (x - 64f) / Mathf.Lerp(27f, 37f, k); // -1..1 sigue la forma del capacillo
            return Mathf.Repeat(u * 3.5f + 0.25f, 1f) < 0.32f;
        }, Paleta.ConAlfa(Paleta.Crema, 0.55f));

        // betun en capas
        float baseY = 50f;
        var capas = new[] { (y: 0f, rx: 44f, ry: 13f), (y: 13f, rx: 36f, ry: 12f), (y: 25f, rx: 27f, ry: 11f), (y: 36f, rx: 16f, ry: 9f) };
        for (int k = 0; k < capas.Length; k++)
        {
            var cp = capas[k];
            var centro = new Vector2(64f, baseY + cp.y / s);
            Color color = k % 2 == 0 ? Paleta.Rosa : Color.Lerp(Paleta.Rosa, Paleta.Crema, 0.45f);
            l.ElipseBorde(centro, new Vector2(cp.rx * s, cp.ry / s), color, Contorno, 3.5f);
        }
        float cima = baseY + 36f / s + 9f / s;

        // chispitas sobre el betun
        Color[] colores = { Paleta.Menta, Paleta.Limon, Paleta.Agua, Paleta.Lila };
        Vector2[] chispas = { new(-26f, 4f), new(18f, 6f), new(-8f, 16f), new(24f, 18f), new(-18f, 27f), new(8f, 30f), new(30f, 2f) };
        for (int k = 0; k < chispas.Length; k++)
        {
            Vector2 p = new Vector2(64f + chispas[k].x * s, baseY + chispas[k].y / s);
            Vector2 dir = new Vector2(Mathf.Cos(k * 1.7f), Mathf.Sin(k * 1.7f)) * 3f;
            l.Capsula(p - dir, p + dir, 2f, colores[k % colores.Length]);
        }

        // cereza
        var cereza = new Vector2(66f, cima + 3f + salto);
        l.CapsulaBorde(cereza + new Vector2(2f, 6f), cereza + new Vector2(7f, 13f), 1.6f, Paleta.Chocolate, Contorno, 2.5f);
        l.ElipseBorde(cereza, new Vector2(10f, 9.5f), Paleta.Fresa, Contorno, 3.5f);
        l.Elipse(cereza + new Vector2(-3.5f, 3.5f), new Vector2(3.5f, 2.5f), Paleta.ConAlfa(Color.white, 0.75f), 30f);
        l.Elipse(new Vector2(46f, baseY + 6f), new Vector2(9f, 3.5f), Paleta.ConAlfa(Color.white, 0.5f), 15f);
        Destello(l, new Vector2(98f, 76f), i, n);
    }

    // estrellita de 4 puntas que crece y se apaga en la mitad de la animacion
    private static void Destello(Lienzo l, Vector2 c, int i, int n)
    {
        float k = Mathf.Sin(Mathf.Clamp01((i - n * 0.25f) / (n * 0.5f)) * Mathf.PI);
        if (k <= 0.05f) return;
        var blanco = Paleta.ConAlfa(Color.white, 0.95f);
        l.Elipse(c, new Vector2(2.5f * k, 12f * k), blanco);
        l.Elipse(c, new Vector2(12f * k, 2.5f * k), blanco);
        l.Elipse(c, new Vector2(4f * k, 4f * k), Color.white);
    }

    // ================================================================ guardar sprites

    private static Sprite Guardar(Lienzo l, string ruta, float ppu, SpriteAlignment alineacion)
    {
        AsegurarCarpeta(Path.GetDirectoryName(ruta));
        var tex = l.Textura();
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);

        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.filterMode = FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.spritePixelsPerUnit = ppu;
        var ajustes = new TextureImporterSettings();
        imp.ReadTextureSettings(ajustes);
        ajustes.spriteAlignment = (int)alineacion;
        ajustes.spriteMeshType = SpriteMeshType.FullRect;
        imp.SetTextureSettings(ajustes);
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // ================================================================ lienzo con suavizado

    // Dibuja formas con 3x3 muestras por pixel para que los bordes salgan suaves.
    private class Lienzo
    {
        private const int Sub = 3;
        private readonly int ancho, alto, w, h;
        private readonly Color[] px;

        public Lienzo(int ancho, int alto)
        {
            this.ancho = ancho;
            this.alto = alto;
            w = ancho * Sub;
            h = alto * Sub;
            px = new Color[w * h];
        }

        public void Forma(Rect caja, Func<float, float, bool> dentro, Color color)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt(caja.xMin * Sub)), x1 = Mathf.Min(w - 1, Mathf.CeilToInt(caja.xMax * Sub));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(caja.yMin * Sub)), y1 = Mathf.Min(h - 1, Mathf.CeilToInt(caja.yMax * Sub));
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if (dentro((x + 0.5f) / Sub, (y + 0.5f) / Sub)) Mezclar(y * w + x, color);
        }

        private void Mezclar(int i, Color c)
        {
            Color d = px[i];
            float a = c.a + d.a * (1f - c.a);
            if (a <= 0f) return;
            Color r = (c * c.a + d * (d.a * (1f - c.a))) / a;
            r.a = a;
            px[i] = r;
        }

        public void Elipse(Vector2 c, Vector2 r, Color color, float rotacion = 0f)
        {
            float rad = rotacion * Mathf.Deg2Rad, cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            float m = Mathf.Max(r.x, r.y);
            Forma(new Rect(c.x - m, c.y - m, m * 2f, m * 2f), (x, y) =>
            {
                float dx = x - c.x, dy = y - c.y;
                float u = (dx * cos + dy * sin) / r.x, v = (-dx * sin + dy * cos) / r.y;
                return u * u + v * v <= 1f;
            }, color);
        }

        public void ElipseBorde(Vector2 c, Vector2 r, Color relleno, Color borde, float grosor, float rotacion = 0f)
        {
            Elipse(c, r + Vector2.one * grosor, borde, rotacion);
            Elipse(c, r, relleno, rotacion);
        }

        public void Capsula(Vector2 a, Vector2 b, float radio, Color color)
        {
            var caja = Rect.MinMaxRect(Mathf.Min(a.x, b.x) - radio, Mathf.Min(a.y, b.y) - radio, Mathf.Max(a.x, b.x) + radio, Mathf.Max(a.y, b.y) + radio);
            Vector2 ab = b - a;
            float largo2 = Mathf.Max(ab.sqrMagnitude, 0.0001f);
            Forma(caja, (x, y) =>
            {
                var p = new Vector2(x, y);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / largo2);
                return (p - (a + ab * t)).sqrMagnitude <= radio * radio;
            }, color);
        }

        public void CapsulaBorde(Vector2 a, Vector2 b, float radio, Color relleno, Color borde, float grosor)
        {
            Capsula(a, b, radio + grosor, borde);
            Capsula(a, b, radio, relleno);
        }

        public void TrianguloBorde(Vector2 a, Vector2 b, Vector2 c, Color relleno, Color borde, float grosor)
        {
            Vector2 centro = (a + b + c) / 3f;
            Triangulo(Crecer(a, centro, grosor), Crecer(b, centro, grosor), Crecer(c, centro, grosor), borde);
            Triangulo(a, b, c, relleno);
        }

        private static Vector2 Crecer(Vector2 p, Vector2 centro, float d)
        {
            return p + (p - centro).normalized * d * 1.6f;
        }

        private void Triangulo(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var caja = Rect.MinMaxRect(Mathf.Min(a.x, b.x, c.x), Mathf.Min(a.y, b.y, c.y), Mathf.Max(a.x, b.x, c.x), Mathf.Max(a.y, b.y, c.y));
            Forma(caja, (x, y) =>
            {
                var p = new Vector2(x, y);
                float d1 = Lado(p, a, b), d2 = Lado(p, b, c), d3 = Lado(p, c, a);
                bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
                return !(neg && pos);
            }, color);
        }

        private static float Lado(Vector2 p, Vector2 a, Vector2 b)
        {
            return (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        }

        // promedia las muestras de cada pixel (premultiplicado para no ensuciar los bordes)
        public Texture2D Textura()
        {
            var tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
            var salida = new Color[ancho * alto];
            for (int y = 0; y < alto; y++)
                for (int x = 0; x < ancho; x++)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    for (int sy = 0; sy < Sub; sy++)
                        for (int sx = 0; sx < Sub; sx++)
                        {
                            Color c = px[(y * Sub + sy) * w + x * Sub + sx];
                            r += c.r * c.a;
                            g += c.g * c.a;
                            b += c.b * c.a;
                            a += c.a;
                        }
                    salida[y * ancho + x] = a > 0f ? new Color(r / a, g / a, b / a, a / (Sub * Sub)) : Color.clear;
                }
            tex.SetPixels(salida);
            tex.Apply();
            return tex;
        }
    }
}
