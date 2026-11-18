using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Compila (publica) el juego para Windows en una carpeta del escritorio.
// Es lo mismo que File > Build Settings > Build con Windows seleccionado.
public static class CompilarJuego
{
    const string NombreJuego = "Gum Adventure";

    static readonly string[] Escenas =
    {
        "Assets/escenas/inicio.unity",
        "Assets/escenas/nivel2.unity",
    };

    [MenuItem("Gum Adventure/Compilar para Windows")]
    public static void Compilar()
    {
        Ejecutar();
    }

    // Para usar desde la linea de comandos con -executeMethod
    public static void CompilarBatch()
    {
        bool ok = Ejecutar();
        EditorApplication.Exit(ok ? 0 : 1);
    }

    static bool Ejecutar()
    {
        // Deja las escenas correctas en Build Settings (inicio primero)
        var lista = new EditorBuildSettingsScene[Escenas.Length];
        for (int i = 0; i < Escenas.Length; i++) lista[i] = new EditorBuildSettingsScene(Escenas[i], true);
        EditorBuildSettings.scenes = lista;

        PlayerSettings.productName = NombreJuego;

        string escritorio = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string carpeta = Path.Combine(escritorio, NombreJuego);
        Directory.CreateDirectory(carpeta);

        var opciones = new BuildPlayerOptions
        {
            scenes = Escenas,
            locationPathName = Path.Combine(carpeta, NombreJuego + ".exe"),
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None,
        };

        BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
        var r = reporte.summary;
        Debug.Log($"[Compilar] Resultado: {r.result}  errores: {r.totalErrors}  tamano: {r.totalSize / (1024 * 1024)} MB  ruta: {r.outputPath}");
        return r.result == BuildResult.Succeeded;
    }
}
