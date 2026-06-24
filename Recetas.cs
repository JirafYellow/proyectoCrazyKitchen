using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

// ═══════════════════════════════════════════════════════════════════════════════
// CLASE RECETA
// ═══════════════════════════════════════════════════════════════════════════════

public class Receta
{
    public string Nombre { get; }
    public string Emoji  { get; }

    // Ingredientes requeridos: (nombre, tipo requerido)
    public List<(string Nombre, Type Tipo, EstadoIngrediente EstadoRequerido)> Ingredientes { get; }

    // Puntuacion base dependiente de la cantidad de ingredientes
    public int PuntosBase   { get; }
    public int PuntosActual { get; private set; }

    // Tiempo maximo de entrega en segundos
    public float TiempoMaxEntrega   { get; }
    public float TiempoTranscurrido { get; private set; } = 0f;

    // Cuantas veces ya se redujo la puntuacion
    private int vecesReducida = 0;

    // La receta fue eliminada por tiempo
    public bool Expirada { get; private set; } = false;

    // Ingredientes listos entregados para esta receta
    private readonly List<Ingrediente> ingredientesEntregados = new List<Ingrediente>();

    public Receta(string nombre, string emoji,
                  List<(string, Type, EstadoIngrediente)> ingredientes)
    {
        Nombre      = nombre;
        Emoji       = emoji;
        Ingredientes = ingredientes;
        PuntosBase  = ingredientes.Count * 30;
        PuntosActual = PuntosBase;
        // Tiempo maximo: 20 segundos base + 10 por ingrediente
        TiempoMaxEntrega = 20f + ingredientes.Count * 10f;
    }

    // Tick de tiempo; retorna true si la receta expiro definitivamente
    public bool Actualizar(float deltaTime)
    {
        if (Expirada) return true;

        TiempoTranscurrido += deltaTime;

        float intervalo = TiempoMaxEntrega;

        // Cada vez que pasa el tiempo maximo, reducir puntos a la mitad
        if (TiempoTranscurrido >= intervalo * (vecesReducida + 1))
        {
            vecesReducida++;
            PuntosActual = PuntosActual / 2;

            if (PuntosActual <= 0)
            {
                PuntosActual = 0;
                Expirada     = true;
                return true;
            }
        }

        return false;
    }

    // Progreso de tiempo para la barra (0=nuevo, 1=expirado)
    public float ProgresoTiempo =>
        Math.Min(TiempoTranscurrido / TiempoMaxEntrega, 1f);

    // Agregar un ingrediente entregado a la lista parcial
    public bool AgregarIngrediente(Ingrediente ing)
    {
        // Verificar si este ingrediente es requerido y no esta ya entregado
        foreach (var req in Ingredientes)
        {
            bool tipoCoincide   = ing.GetType() == req.Tipo;
            bool nombreCoincide = ing.Nombre == req.Nombre;
            bool estadoOk       = ing.Estado == req.EstadoRequerido;

            if (tipoCoincide && nombreCoincide && estadoOk)
            {
                // Verificar que no este ya en la lista
                int yaEntregados = ingredientesEntregados
                    .Count(x => x.GetType() == req.Tipo && x.Nombre == req.Nombre);
                int requeridos = Ingredientes
                    .Count(r => r.Tipo == req.Tipo && r.Nombre == req.Nombre);

                if (yaEntregados < requeridos)
                {
                    ingredientesEntregados.Add(ing);
                    return true;
                }
            }
        }
        return false;
    }

    // Verificar si la receta esta completa con los ingredientes entregados
    public bool EstaCompleta()
    {
        if (ingredientesEntregados.Count < Ingredientes.Count) return false;

        foreach (var req in Ingredientes)
        {
            int entregados = ingredientesEntregados
                .Count(x => x.GetType() == req.Tipo &&
                            x.Nombre    == req.Nombre &&
                            x.Estado    == req.EstadoRequerido);
            int requeridos = Ingredientes
                .Count(r => r.Tipo   == req.Tipo &&
                            r.Nombre == req.Nombre);

            if (entregados < requeridos) return false;
        }
        return true;
    }

    public int IngredientesEntregados => ingredientesEntregados.Count;
    public int IngredientesTotales    => Ingredientes.Count;
}

// ═══════════════════════════════════════════════════════════════════════════════
// CATALOGO DE RECETAS — define todas las recetas disponibles
// ═══════════════════════════════════════════════════════════════════════════════

public static class CatalogoRecetas
{
    private static readonly Random rng = new Random();

    // Escenario 1 — recetas simples
    private static readonly List<Func<Receta>> recetasEscenario1 = new List<Func<Receta>>
    {
        () => new Receta("Ensalada Simple", "🥗",
            new List<(string, Type, EstadoIngrediente)>
            {
                ("Lechuga",  typeof(VegetalesYFrutas), EstadoIngrediente.Cortado),
                ("Tomate",   typeof(VegetalesYFrutas), EstadoIngrediente.Cortado),
            }),

        () => new Receta("Hamburguesa", "🍔",
            new List<(string, Type, EstadoIngrediente)>
            {
                ("Pan de Hamburguesa", typeof(PanesYBases), EstadoIngrediente.Listo),
                ("Carne de Res",       typeof(Proteina),    EstadoIngrediente.Cocinado),
                ("Tomate",             typeof(VegetalesYFrutas), EstadoIngrediente.Cortado),
            }),

        () => new Receta("Pollo con Papas", "🍗",
            new List<(string, Type, EstadoIngrediente)>
            {
                ("Pollo",       typeof(Proteina),    EstadoIngrediente.Cocinado),
                ("Papas Fritas", typeof(PapasFritas), EstadoIngrediente.Frito),
            }),

        () => new Receta("Sandwich", "🥪",
            new List<(string, Type, EstadoIngrediente)>
            {
                ("Pan Integral",  typeof(PanesYBases),     EstadoIngrediente.Listo),
                ("Lechuga",       typeof(VegetalesYFrutas), EstadoIngrediente.Cortado),
                ("Pollo",         typeof(Proteina),         EstadoIngrediente.Cocinado),
            }),
    };

    // Retorna una receta aleatoria del escenario
    public static Receta GenerarRecetaAleatoria(int escenario = 1)
    {
        var lista = recetasEscenario1;
        return lista[rng.Next(lista.Count)]();
    }

    // Catalogo de que ingrediente vende cada despensa (nombre -> fabrica)
    public static Dictionary<string, Func<Ingrediente>> IngredientesPorDespensa =
        new Dictionary<string, Func<Ingrediente>>
    {
        { "Lechuga",            () => new VegetalesYFrutas("Lechuga",  "🥬") },
        { "Tomate",             () => new VegetalesYFrutas("Tomate",   "🍅") },
        { "Pan de Hamburguesa", () => new PanesYBases("Pan de Hamburguesa", "🍞") },
        { "Pan Integral",       () => new PanesYBases("Pan Integral",       "🫓") },
        { "Carne de Res",       () => new Proteina("Carne de Res",     "🥩") },
        { "Pollo",              () => new Proteina("Pollo",            "🍗") },
        { "Papas Fritas",       () => new PapasFritas() },
    };
}
