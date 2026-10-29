using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Explosion de brillitos de colores y un "+1" que sube, al recoger una chupeta.
public class EfectoRecoger : MonoBehaviour
{
    private const float Duracion = 0.7f;
    private const int CantidadBrillos = 14;

    private static Sprite punto;
    private static readonly Color[] Colores =
    {
        new Color(1f, 0.45f, 0.65f),
        new Color(1f, 0.85f, 0.3f),
        new Color(0.4f, 0.9f, 0.75f),
        new Color(0.7f, 0.55f, 1f),
        Color.white
    };

    private readonly List<SpriteRenderer> brillos = new List<SpriteRenderer>();
    private readonly List<Vector2> velocidades = new List<Vector2>();
    private TextMeshPro texto;
    private float tiempo;

    public static void Crear(Vector3 posicion)
    {
        var go = new GameObject("Efecto Recoger");
        go.transform.position = posicion;
        go.AddComponent<EfectoRecoger>();
    }

    private void Start()
    {
        if (punto == null) punto = CrearPunto();

        for (int i = 0; i < CantidadBrillos; i++)
        {
            var b = new GameObject("Brillo");
            b.transform.SetParent(transform, false);
            b.transform.localScale = Vector3.one * Random.Range(0.25f, 0.45f);
            var sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = punto;
            sr.color = Colores[i % Colores.Length];
            sr.sortingOrder = 30;
            brillos.Add(sr);
            velocidades.Add(Random.insideUnitCircle.normalized * Random.Range(3f, 6f));
        }

        var t = new GameObject("+1");
        t.transform.SetParent(transform, false);
        t.transform.localPosition = new Vector3(0f, 0.6f, 0f);
        texto = t.AddComponent<TextMeshPro>();
        texto.text = "+1";
        texto.fontSize = 7;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(1f, 0.95f, 0.4f);
        texto.sortingOrder = 31;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        tiempo += dt;
        float k = Mathf.Clamp01(tiempo / Duracion);

        for (int i = 0; i < brillos.Count; i++)
        {
            velocidades[i] *= 1f - 4f * dt;
            brillos[i].transform.localPosition += (Vector3)(velocidades[i] * dt);
            Color c = brillos[i].color;
            c.a = 1f - k;
            brillos[i].color = c;
        }

        if (texto != null)
        {
            texto.transform.localPosition += Vector3.up * (1.5f * dt);
            Color c = texto.color;
            c.a = 1f - k * k;
            texto.color = c;
        }

        if (k >= 1f) Destroy(gameObject);
    }

    private static Sprite CrearPunto()
    {
        const int lado = 16;
        var tex = new Texture2D(lado, lado);
        float r = lado / 2f;
        for (int y = 0; y < lado; y++)
            for (int x = 0; x < lado; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(r - d)));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), lado);
    }
}
