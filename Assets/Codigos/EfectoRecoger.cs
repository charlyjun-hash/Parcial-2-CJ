using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

// Al recoger una chupeta: estallido de brillitos (sistema de particulas reutilizado)
// y un "+1" que sube y se desvanece. Los textos "+1" salen de un pool, no se crean
// ni se destruyen en cada recogida.
public class EfectoRecoger : MonoBehaviour
{
    private static EfectoRecoger instancia;
    private ObjectPool<TextMeshPro> textos;

    public static void Crear(Vector3 posicion)
    {
        Efectos.Estallido(posicion);
        if (instancia == null)
        {
            instancia = new GameObject("Textos Flotantes").AddComponent<EfectoRecoger>();
            instancia.textos = new ObjectPool<TextMeshPro>(CrearTexto, t => t.gameObject.SetActive(true),
                t => t.gameObject.SetActive(false), t => Destroy(t.gameObject), false, 8, 20);
        }
        instancia.StartCoroutine(instancia.Subir(posicion));
    }

    private void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    private static TextMeshPro CrearTexto()
    {
        var go = new GameObject("+1");
        go.transform.SetParent(instancia.transform, false);
        var t = go.AddComponent<TextMeshPro>();
        t.text = "+1";
        t.fontSize = 7;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.sortingLayerID = SortingLayer.NameToID("UI 3");
        t.sortingOrder = 60;
        return t;
    }

    private IEnumerator Subir(Vector3 posicion)
    {
        var t = textos.Get();
        Vector3 inicio = posicion + Vector3.up * 0.5f;
        yield return Ease.Tween(0.6f, k =>
        {
            t.transform.position = inicio + Vector3.up * (0.8f * k);
            t.transform.localScale = Vector3.one * (k < 0.2f ? Mathf.Lerp(0.6f, 1.2f, k / 0.2f) : Mathf.Lerp(1.2f, 1f, (k - 0.2f) / 0.8f));
            t.color = Paleta.ConAlfa(Paleta.Limon, 1f - k * k);
        }, Ease.Lineal, false);
        textos.Release(t);
    }
}
