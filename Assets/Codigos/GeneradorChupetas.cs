using System.Collections.Generic;
using UnityEngine;

// Cada vez que empieza el nivel, pone las chupetas en lugares al azar
// encima del piso, los muros y las plataformas, sin que queden metidas
// dentro de un bloque ni pegadas unas con otras.
public class GeneradorChupetas : MonoBehaviour
{
    public List<Transform> chupetas = new List<Transform>();     // chupetas que se van a repartir
    public List<BoxCollider2D> superficies = new List<BoxCollider2D>();   // donde se pueden poner

    [Header("Ajustes")]
    public float alturaSobreSuperficie = 1.0f;   // a que altura queda la chupeta encima de la superficie
    public float margenBorde = 0.4f;             // distancia minima a los bordes de la superficie
    public float distanciaMinima = 1.3f;         // separacion minima entre chupetas
    public float radioLibre = 0.45f;             // espacio libre que necesita la chupeta para no quedar dentro de un bloque
    public float pesoMaximo = 5f;                // las superficies muy anchas (el piso) no se llevan casi todas las chupetas
    public float distanciaJugador = 3f;          // no aparecen tan cerca del jugador, asi no se recogen solas al empezar

    private void Start()
    {
        Repartir();     // en Start, cuando todos los colisionadores del nivel ya existen
    }

    [ContextMenu("Repartir chupetas")]
    public void Repartir()
    {
        var usadas = new List<Vector2>();
        var filtro = new ContactFilter2D();
        filtro.useTriggers = false;
        var choques = new List<Collider2D>();
        GameObject jugador = GameObject.FindWithTag("Personaje");

        // las superficies anchas tienen mas probabilidad, asi se reparten parejo
        float anchoTotal = 0f;
        foreach (var s in superficies)
            if (s != null) anchoTotal += Peso(s);

        foreach (Transform chupeta in chupetas)
        {
            if (chupeta == null) continue;

            Vector2 elegido = chupeta.position;
            for (int intento = 0; intento < 60; intento++)
            {
                BoxCollider2D superficie = ElegirSuperficie(anchoTotal);
                if (superficie == null) break;

                Bounds b = superficie.bounds;
                float x = Random.Range(b.min.x + margenBorde, b.max.x - margenBorde);
                Vector2 punto = new Vector2(x, b.max.y + alturaSobreSuperficie);

                // que no quede dentro de un muro o plataforma
                if (Physics2D.OverlapCircle(punto, radioLibre, filtro, choques) > 0) continue;

                // que no aparezca encima del jugador
                if (jugador != null && Vector2.Distance(jugador.transform.position, punto) < distanciaJugador) continue;

                // que no quede encima de otra chupeta
                bool muyCerca = false;
                foreach (var u in usadas)
                    if (Vector2.Distance(u, punto) < distanciaMinima) { muyCerca = true; break; }
                if (muyCerca) continue;

                elegido = punto;
                break;
            }

            usadas.Add(elegido);
            chupeta.position = new Vector3(elegido.x, elegido.y, chupeta.position.z);
        }
    }

    private float Peso(BoxCollider2D s)
    {
        return Mathf.Min(pesoMaximo, Mathf.Max(0f, s.bounds.size.x - margenBorde * 2f));
    }

    private BoxCollider2D ElegirSuperficie(float anchoTotal)
    {
        float r = Random.Range(0f, anchoTotal);
        foreach (var s in superficies)
        {
            if (s == null) continue;
            float peso = Peso(s);
            if (r <= peso) return s;
            r -= peso;
        }
        return superficies.Count > 0 ? superficies[superficies.Count - 1] : null;
    }
}
