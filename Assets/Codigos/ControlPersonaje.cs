using UnityEngine;
class ControlPersonaje : MonoBehaviour
{
    public int vidasTotales = 2;
    public int vida = 100;
    public int danio = 20;
    public float velocidad = 5.5f;
    public bool EstaVivo = true;
    public string nombre = "Bob";
    public GameObject Arma;
    public AudioSource SonidoDisparo;
}