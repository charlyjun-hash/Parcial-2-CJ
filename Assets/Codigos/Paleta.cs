using UnityEngine;

// Guia de arte "Dulcelandia": todos los colores nuevos del juego salen de aqui.
// Estilo: mundo de dulces pastel 2D, formas redondeadas planas con glaseado y sombras suaves.
// Ambiente: tarde soleada de feria; las sombras son ciruela, nunca negras.
public static class Paleta
{
    public static readonly Color Rosa = Hex(0xF878A8);      // plataformas, boton principal
    public static readonly Color Menta = Hex(0x5CD6B0);     // plataformas, exito
    public static readonly Color Limon = Hex(0xFFC754);     // acento: estrellas, progreso, record
    public static readonly Color Lila = Hex(0xB08CEE);      // plataformas, botones secundarios
    public static readonly Color Chocolate = Hex(0x6B402C); // suelo
    public static readonly Color Crema = Hex(0xFFF1D6);     // texto claro, glaseado
    public static readonly Color Ciruela = Hex(0x251633);   // sombras, fondos de paneles
    public static readonly Color Agua = Hex(0x4DA6FF);      // agua, salpicaduras
    public static readonly Color Fresa = Hex(0xE84A5F);     // danio, corazones

    public static readonly Color[] Dulces = { Rosa, Menta, Limon, Lila, Crema };

    public static Color ConAlfa(Color c, float a)
    {
        c.a = a;
        return c;
    }

    private static Color Hex(int rgb)
    {
        return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
