using System;
using System.Drawing;
using System.Windows.Forms;

// ═══════════════════════════════════════════════════════════════════════════════
// ENUM TIPO DE ESTACION
// ═══════════════════════════════════════════════════════════════════════════════

public enum TipoEstacion
{
    Despensa,
    Cocina,
    TablaDeCortar,
    Freidora,
    Entrega
}

// ═══════════════════════════════════════════════════════════════════════════════
// ENUM ESTADO DE INGREDIENTE
// ═══════════════════════════════════════════════════════════════════════════════

public enum EstadoIngrediente
{
    Crudo,
    Cortado,
    Cocinado,
    Frito,
    Quemado,
    Listo
}

// ═══════════════════════════════════════════════════════════════════════════════
// CLASE BASE INGREDIENTE (ABSTRACTA)
// ═══════════════════════════════════════════════════════════════════════════════

public abstract class Ingrediente
{
    public string           Nombre   { get; set; }
    public EstadoIngrediente Estado  { get; set; }
    public string           Emoji    { get; protected set; }

    // Progreso de preparacion en estacion (0.0 a 1.0)
    public float ProgresoPreparacion { get; set; } = 0f;
    public bool  EnPreparacion       { get; set; } = false;

    protected Ingrediente(string nombre, string emoji)
    {
        Nombre = nombre;
        Emoji  = emoji;
        Estado = EstadoIngrediente.Crudo;
    }

    // Tipo de estacion que acepta este ingrediente para prepararlo
    public abstract TipoEstacion    EstacionRequerida { get; }

    // Estado final valido para usar en receta
    public abstract EstadoIngrediente EstadoListo { get; }

    public bool EstaListo()   => Estado == EstadoListo;
    public bool EstaQuemado() => Estado == EstadoIngrediente.Quemado;

    public override string ToString() => $"{Nombre} [{Estado}]";
}

// ═══════════════════════════════════════════════════════════════════════════════
// CLASES HIJAS DE INGREDIENTE (HERENCIA)
// ═══════════════════════════════════════════════════════════════════════════════

public class VegetalesYFrutas : Ingrediente
{
    public VegetalesYFrutas(string nombre, string emoji = "🥬") : base(nombre, emoji) { }

    public override TipoEstacion     EstacionRequerida => TipoEstacion.TablaDeCortar;
    public override EstadoIngrediente EstadoListo      => EstadoIngrediente.Cortado;
}

public class PanesYBases : Ingrediente
{
    // El pan llega listo directamente de la despensa
    public PanesYBases(string nombre, string emoji = "🍞") : base(nombre, emoji)
    {
        Estado = EstadoIngrediente.Listo;
    }

    public override TipoEstacion     EstacionRequerida => TipoEstacion.Despensa;
    public override EstadoIngrediente EstadoListo      => EstadoIngrediente.Listo;
}

public class Proteina : Ingrediente
{
    public bool Cocinada { get; set; } = false;

    public Proteina(string nombre, string emoji = "🥩") : base(nombre, emoji) { }

    public override TipoEstacion     EstacionRequerida => TipoEstacion.Cocina;
    public override EstadoIngrediente EstadoListo      => EstadoIngrediente.Cocinado;
}

public class PapasFritas : Ingrediente
{
    public PapasFritas() : base("Papas Fritas", "🍟") { }

    public override TipoEstacion     EstacionRequerida => TipoEstacion.Freidora;
    public override EstadoIngrediente EstadoListo      => EstadoIngrediente.Frito;
}

// ═══════════════════════════════════════════════════════════════════════════════
// CLASE CHEF
// ═══════════════════════════════════════════════════════════════════════════════

public class Chef
{
    public string     Nombre           { get; set; }
    public int        Puntos           { get; set; }
    public Color      Color            { get; }
    public string     Emoji            { get; }
    public int        Col              { get; set; }
    public int        Fila             { get; set; }
    public int        DirX             { get; set; } = 0;
    public int        DirY             { get; set; } = 1;
    public Ingrediente IngredienteEnMano { get; set; }

    private readonly Keys teclaArriba;
    private readonly Keys teclaAbajo;
    private readonly Keys teclaIzquierda;
    private readonly Keys teclaDerecha;
    private readonly Keys teclaAccion;

    public Chef(string nombre, int colInicial, int filaInicial,
                Color color, string emoji,
                Keys arriba, Keys abajo, Keys izquierda, Keys derecha, Keys accion)
    {
        Nombre         = nombre;
        Col            = colInicial;
        Fila           = filaInicial;
        Color          = color;
        Emoji          = emoji;
        Puntos         = 0;
        IngredienteEnMano = null;
        teclaArriba    = arriba;
        teclaAbajo     = abajo;
        teclaIzquierda = izquierda;
        teclaDerecha   = derecha;
        teclaAccion    = accion;
    }

    // Devuelve true si la tecla le pertenece a este chef
    public bool ProcesarTecla(Keys tecla, int maxCols, int maxFilas,
                               int[,] mapa, ref bool accionActivada)
    {
        int nuevaCol  = Col;
        int nuevaFila = Fila;

        if      (tecla == teclaArriba)    { nuevaFila--; DirX = 0;  DirY = -1; }
        else if (tecla == teclaAbajo)     { nuevaFila++; DirX = 0;  DirY =  1; }
        else if (tecla == teclaIzquierda) { nuevaCol--;  DirX = -1; DirY =  0; }
        else if (tecla == teclaDerecha)   { nuevaCol++;  DirX =  1; DirY =  0; }
        else if (tecla == teclaAccion)    { accionActivada = true; return true; }
        else return false;

        // Solo mover si la celda destino es piso (valor 0)
        if (nuevaCol >= 0 && nuevaCol < maxCols &&
            nuevaFila >= 0 && nuevaFila < maxFilas &&
            mapa[nuevaFila, nuevaCol] == 0)
        {
            Col  = nuevaCol;
            Fila = nuevaFila;
        }

        return true;
    }

    public void Dibujar(Graphics g, int tamCelda, Font fuente)
    {
        int x = Col * tamCelda;
        int y = Fila * tamCelda;

        // Sombra
        using (var s = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            g.FillEllipse(s, x + 6, y + tamCelda - 10, tamCelda - 12, 8);

        // Cuerpo
        using (var p = new SolidBrush(Color))
            g.FillEllipse(p, x + 5, y + 5, tamCelda - 10, tamCelda - 10);

        // Borde blanco
        using (var l = new Pen(System.Drawing.Color.White, 2))
            g.DrawEllipse(l, x + 5, y + 5, tamCelda - 10, tamCelda - 10);

        // Emoji del chef
        var fmt = new StringFormat
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(Emoji, fuente, Brushes.White,
            new RectangleF(x, y, tamCelda, tamCelda), fmt);

        // Indicador de item en mano (esquina superior derecha)
        if (IngredienteEnMano != null)
        {
            using var fItem = new Font("Segoe UI Emoji", 9);
            g.DrawString(IngredienteEnMano.Emoji, fItem, Brushes.White, x + tamCelda - 18, y);
        }
    }

    public string ObtenerControles()
    {
        bool esWASD = teclaArriba == Keys.W;
        return esWASD
            ? "W/A/S/D mover  |  E accion"
            : "Flechas mover  |  Enter accion";
    }
}
