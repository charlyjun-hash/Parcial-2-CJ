using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Movimiento de plataforma 2D: caminar, correr, agacharse, saltar solo desde el suelo
// y mandarle al Animator los datos para cambiar entre animaciones.
// Controles: A/D o flechas para moverse, Shift para correr, S o flecha abajo para agacharse,
// Espacio, W o flecha arriba para saltar.
[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5.5f;              // si el objeto tiene ControlPersonaje, se usa la velocidad de ese script
    public float multiplicadorCorrer = 1.6f;    // correr = velocidad x este valor
    public float multiplicadorAgachado = 0.4f;  // agachado = velocidad x este valor
    public float fuerzaSalto = 14f;

    [Header("Estado")]
    public bool puedeMoverse = true;            // si se pone en false el personaje se queda quieto

    [Header("Deteccion de suelo")]
    public float distanciaSuelo = 0.05f;        // que tan cerca debe estar el piso para contar como "en el suelo"

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;
    private ControlPersonaje datosPersonaje;

    private float entradaHorizontal;
    private bool pidioSalto;
    private bool corriendo;
    private bool agachado;
    private bool enSuelo;

    private ContactFilter2D filtroSuelo;
    private readonly List<RaycastHit2D> golpes = new List<RaycastHit2D>();
    private readonly HashSet<string> parametrosAnimator = new HashSet<string>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        datosPersonaje = GetComponent<ControlPersonaje>();

        filtroSuelo = new ContactFilter2D();
        filtroSuelo.useTriggers = false;    // las chupetas no cuentan como piso
    }

    private void Start()
    {
        // guardar los parametros que existan en el Animator para no mandar parametros que no esten creados
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
                parametrosAnimator.Add(p.name);
        }
    }

    private void Update()
    {
        LeerControles();

        // voltear el sprite hacia donde camina
        if (sprite != null)
        {
            if (entradaHorizontal > 0.01f) sprite.flipX = false;
            else if (entradaHorizontal < -0.01f) sprite.flipX = true;
        }

        ActualizarAnimaciones();
    }

    private void FixedUpdate()
    {
        enSuelo = EstaEnElSuelo();

        bool activo = puedeMoverse && (datosPersonaje == null || datosPersonaje.EstaVivo);
        float velocidadActual = datosPersonaje != null ? datosPersonaje.velocidad : velocidad;

        if (agachado && enSuelo) velocidadActual *= multiplicadorAgachado;
        else if (corriendo) velocidadActual *= multiplicadorCorrer;

        Vector2 v = rb.linearVelocity;
        v.x = activo ? entradaHorizontal * velocidadActual : 0f;

        if (pidioSalto && enSuelo && activo && !agachado)
            v.y = fuerzaSalto;

        pidioSalto = false;
        rb.linearVelocity = v;
    }

    private void LeerControles()
    {
        entradaHorizontal = 0f;
        corriendo = false;
        agachado = false;

        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) entradaHorizontal -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) entradaHorizontal += 1f;

            corriendo = teclado.leftShiftKey.isPressed || teclado.rightShiftKey.isPressed;
            agachado = teclado.sKey.isPressed || teclado.downArrowKey.isPressed;

            if (teclado.spaceKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame)
                pidioSalto = true;
        }

        Gamepad control = Gamepad.current;
        if (control != null)
        {
            float x = control.leftStick.x.ReadValue();
            if (Mathf.Abs(x) > 0.2f) entradaHorizontal = x;
            if (control.leftStick.y.ReadValue() < -0.6f) agachado = true;
            if (control.buttonWest.isPressed || control.rightTrigger.isPressed) corriendo = true;
            if (control.buttonSouth.wasPressedThisFrame) pidioSalto = true;
        }

        entradaHorizontal = Mathf.Clamp(entradaHorizontal, -1f, 1f);
    }

    private bool EstaEnElSuelo()
    {
        // lanza la forma del colisionador un poquito hacia abajo y revisa si toca algo plano
        int cantidad = rb.Cast(Vector2.down, filtroSuelo, golpes, distanciaSuelo);
        for (int i = 0; i < cantidad; i++)
        {
            if (golpes[i].normal.y > 0.5f) return true;
        }
        return false;
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null || parametrosAnimator.Count == 0) return;

        float vx = Mathf.Abs(rb.linearVelocity.x);
        bool moviendose = vx > 0.1f;

        // estados del Animator del personaje: Ocio, Caminar, Correr, Agachar, Saltar
        PonerBool("Ocio", !moviendose);
        PonerBool("Caminar", moviendose);
        PonerBool("Correr", moviendose && corriendo && !agachado);
        PonerBool("Agachar", agachado && enSuelo);
        PonerBool("EnSuelo", enSuelo);
        PonerFloat("VelocidadX", vx);
        PonerFloat("Velocidad", vx);
        PonerFloat("VelocidadY", rb.linearVelocity.y);
    }

    private void PonerBool(string nombre, bool valor)
    {
        if (parametrosAnimator.Contains(nombre)) animator.SetBool(nombre, valor);
    }

    private void PonerFloat(string nombre, float valor)
    {
        if (parametrosAnimator.Contains(nombre)) animator.SetFloat(nombre, valor);
    }
}
