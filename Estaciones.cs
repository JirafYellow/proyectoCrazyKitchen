using System;
using System.Collections.Generic;
using System.Drawing;

// ═══════════════════════════════════════════════════════════════════════════════
// CLASE BASE ESTACION (ABSTRACTA)
// ═══════════════════════════════════════════════════════════════════════════════

public abstract class Estacion
{
    public string       Nombre   { get; protected set; }
    public TipoEstacion Tipo     { get; protected set; }
    public string       Emoji    { get; protected set; }
    public Color        ColorUI  { get; protected set; }

    // Ingrediente actualmente siendo procesado en la estacion
    public Ingrediente IngredienteActual { get; protected set; }

    // Tiempo en segundos que tarda en preparar (override en hijas)
    protected virtual float TiempoPreparacion  => 3f;
    protected virtual float TiempoMaxCoccion   => 7f; // solo para cocina/freidora

    private float timerPreparacion = 0f;
    private bool  enCoccion        = false;

    protected Estacion(string nombre, TipoEstacion tipo, string emoji, Color color)
    {
        Nombre  = nombre;
        Tipo    = tipo;
        Emoji   = emoji;
        ColorUI = color;
    }

    // Intentar depositar un ingrediente en la estacion
    // Retorna true si fue aceptado
    public virtual bool IntentarDepositar(Ingrediente ing)
    {
        if (IngredienteActual != null) return false;
        if (!AceptaIngrediente(ing))  return false;

        IngredienteActual = ing;
        IngredienteActual.ProgresoPreparacion = 0f;
        IngredienteActual.EnPreparacion       = true;
        timerPreparacion = 0f;
        enCoccion        = true;
        return true;
    }

    // Retirar el ingrediente procesado
    public virtual Ingrediente Retirar()
    {
        var ing = IngredienteActual;
        IngredienteActual = null;
        timerPreparacion  = 0f;
        enCoccion         = false;
        return ing;
    }

    // Verificar si acepta este tipo de ingrediente
    protected abstract bool AceptaIngrediente(Ingrediente ing);

    // Aplicar la preparacion al ingrediente cuando esta listo
    protected abstract void AplicarPreparacion(Ingrediente ing);

    // Tick del juego: deltaTime en segundos
    public void Actualizar(float deltaTime)
    {
        if (IngredienteActual == null || !enCoccion) return;
        if (IngredienteActual.EstaListo() || IngredienteActual.EstaQuemado()) return;

        timerPreparacion += deltaTime;
        IngredienteActual.ProgresoPreparacion =
            Math.Min(timerPreparacion / TiempoPreparacion, 1f);

        // Preparacion completada
        if (timerPreparacion >= TiempoPreparacion && !IngredienteActual.EstaListo())
        {
            AplicarPreparacion(IngredienteActual);
            IngredienteActual.EnPreparacion = false;
        }

        // Quemado (solo estaciones con calor)
        if (PuedeQuemarse() && timerPreparacion >= TiempoMaxCoccion)
        {
            IngredienteActual.Estado = EstadoIngrediente.Quemado;
            IngredienteActual.EnPreparacion = false;
        }
    }

    protected virtual bool PuedeQuemarse() => false;

    // Porcentaje de progreso para la barra visual (0-1)
    public float ObtenerProgreso()
    {
        if (IngredienteActual == null) return 0f;
        return IngredienteActual.ProgresoPreparacion;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// ESTACION: DESPENSA
// Entrega ingredientes ilimitados al chef
// ═══════════════════════════════════════════════════════════════════════════════

public class Despensa : Estacion
{
    // Funcion que fabrica el ingrediente de esta despensa
    private readonly Func<Ingrediente> fabricarIngrediente;
    public string NombreIngrediente { get; }

    public Despensa(string nombreIng, Func<Ingrediente> fabrica)
        : base("Despensa", TipoEstacion.Despensa, "📦", Color.FromArgb(60, 140, 80))
    {
        NombreIngrediente  = nombreIng;
        fabricarIngrediente = fabrica;
    }

    // La despensa nunca retiene: siempre entrega al instante
    public Ingrediente EntregarIngrediente() => fabricarIngrediente();

    // La despensa no "procesa" nada internamente
    protected override bool AceptaIngrediente(Ingrediente ing) => false;
    protected override void AplicarPreparacion(Ingrediente ing) { }

    public override bool IntentarDepositar(Ingrediente ing) => false;
}

// ═══════════════════════════════════════════════════════════════════════════════
// ESTACION: COCINA (SARTEN)
// Cocina proteinas; puede quemarlas si se pasa el tiempo
// ═══════════════════════════════════════════════════════════════════════════════

public class Cocina : Estacion
{
    protected override float TiempoPreparacion => 4f;
    protected override float TiempoMaxCoccion  => 9f;

    public Cocina()
        : base("Cocina", TipoEstacion.Cocina, "🍳", Color.FromArgb(180, 80, 40)) { }

    protected override bool AceptaIngrediente(Ingrediente ing)
        => ing is Proteina && ing.Estado == EstadoIngrediente.Crudo;

    protected override void AplicarPreparacion(Ingrediente ing)
    {
        ing.Estado = EstadoIngrediente.Cocinado;
        if (ing is Proteina p) p.Cocinada = true;
    }

    protected override bool PuedeQuemarse() => true;
}

// ═══════════════════════════════════════════════════════════════════════════════
// ESTACION: TABLA DE CORTAR
// Corta vegetales y frutas
// ═══════════════════════════════════════════════════════════════════════════════

public class TablaDeCortar : Estacion
{
    protected override float TiempoPreparacion => 3f;

    public TablaDeCortar()
        : base("Tabla", TipoEstacion.TablaDeCortar, "🔪", Color.FromArgb(140, 100, 40)) { }

    protected override bool AceptaIngrediente(Ingrediente ing)
        => ing is VegetalesYFrutas && ing.Estado == EstadoIngrediente.Crudo;

    protected override void AplicarPreparacion(Ingrediente ing)
        => ing.Estado = EstadoIngrediente.Cortado;
}

// ═══════════════════════════════════════════════════════════════════════════════
// ESTACION: FREIDORA
// Frie papas; puede quemarlas
// ═══════════════════════════════════════════════════════════════════════════════

public class Freidora : Estacion
{
    protected override float TiempoPreparacion => 5f;
    protected override float TiempoMaxCoccion  => 11f;

    public Freidora()
        : base("Freidora", TipoEstacion.Freidora, "🫕", Color.FromArgb(160, 130, 20)) { }

    protected override bool AceptaIngrediente(Ingrediente ing)
        => ing is PapasFritas && ing.Estado == EstadoIngrediente.Crudo;

    protected override void AplicarPreparacion(Ingrediente ing)
        => ing.Estado = EstadoIngrediente.Frito;

    protected override bool PuedeQuemarse() => true;
}

// ═══════════════════════════════════════════════════════════════════════════════
// ESTACION: ENTREGA
// Valida y acepta recetas completas
// ═══════════════════════════════════════════════════════════════════════════════

public class EstacionEntrega : Estacion
{
    public EstacionEntrega()
        : base("Entrega", TipoEstacion.Entrega, "🍽", Color.FromArgb(60, 100, 180)) { }

    protected override bool AceptaIngrediente(Ingrediente ing) => false;
    protected override void AplicarPreparacion(Ingrediente ing) { }
    public override bool IntentarDepositar(Ingrediente ing) => false;
}
