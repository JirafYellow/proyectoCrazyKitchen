using System;
using System.Collections.Generic;

namespace CrazySnackRush
{
    // =========================================================
    //  MAPA.CS  — Cuadricula, estaciones y pantalla principal
    // =========================================================

    /// <summary>
    /// Tipos de celda que puede tener el mapa.
    /// </summary>
    public enum TipoCelda
    {
        Vacia,          // El chef puede caminar aqui
        Pared,          // Obstaculo; no se puede pasar
        Despensa,       // Estacion de ingredientes (ilimitada)
        Cocina,         // Prepara proteinas
        TablaPicar,     // Prepara vegetales/frutas
        Freidora,       // Prepara papas fritas
        EntregaPlatos   // Unica estacion de entrega
    }

    // =========================================================
    //  INGREDIENTE (jerarquia base)
    // =========================================================
    public abstract class Ingrediente
    {
        public string Nombre { get; protected set; }
        public string Estado { get; protected set; }
        public abstract void Preparar();
    }

    public class Vegetal : Ingrediente
    {
        public Vegetal(string nombre) { Nombre = nombre; Estado = "Crudo"; }
        public override void Preparar() { Estado = "Cortado"; }
    }

    public class PanYBase : Ingrediente
    {
        public PanYBase(string nombre) { Nombre = nombre; Estado = "Listo"; }
        public override void Preparar() { /* Los panes no necesitan preparacion */ }
    }

    public class Proteina : Ingrediente
    {
        public bool Cocinada { get; private set; }

        public Proteina(string nombre)
        {
            Nombre   = nombre;
            Estado   = "Cruda";
            Cocinada = false;
        }

        public override void Preparar()
        {
            Estado   = "Cocinada";
            Cocinada = true;
        }
    }

    // =========================================================
    //  ESTACIONES (jerarquia base)
    // =========================================================
    public abstract class Estacion
    {
        public string Nombre { get; protected set; }
        public abstract bool ProcesarIngrediente(Ingrediente ingrediente);
    }

    public class Despensa : Estacion
    {
        private Ingrediente tipoIngrediente;

        public Despensa(string nombre, Ingrediente tipo)
        {
            Nombre          = nombre;
            tipoIngrediente = tipo;
        }

        /// <summary>
        /// Retorna siempre el ingrediente asociado (despensa ilimitada).
        /// En el juego real se instanciaria una copia nueva cada vez.
        /// </summary>
        public Ingrediente ObtenerIngrediente()
        {
            Console.WriteLine("[Despensa] Entregando: " + tipoIngrediente.Nombre);
            return tipoIngrediente;
        }

        public override bool ProcesarIngrediente(Ingrediente ingrediente)
        {
            return false;   // La despensa no procesa, solo entrega
        }
    }

    public class EstacionTrabajo : Estacion
    {
        private TipoCelda tipoEstacion;

        public EstacionTrabajo(string nombre, TipoCelda tipo)
        {
            Nombre       = nombre;
            tipoEstacion = tipo;
        }

        public override bool ProcesarIngrediente(Ingrediente ingrediente)
        {
            bool valido = false;

            switch (tipoEstacion)
            {
                case TipoCelda.Cocina:
                    valido = (ingrediente is Proteina);
                    break;
                case TipoCelda.TablaPicar:
                    valido = (ingrediente is Vegetal);
                    break;
                case TipoCelda.Freidora:
                    valido = (ingrediente is Vegetal) && ingrediente.Nombre == "Papa";
                    break;
            }

            if (!valido)
            {
                Console.WriteLine("[" + Nombre + "] No puede preparar '" + ingrediente.Nombre + "'.");
                return false;
            }

            ingrediente.Preparar();
            Console.WriteLine("[" + Nombre + "] Preparó: " + ingrediente.Nombre + " → " + ingrediente.Estado);
            return true;
        }
    }

    public class EstacionEntrega : Estacion
    {
        public EstacionEntrega() { Nombre = "Estacion de Entrega"; }

        public override bool ProcesarIngrediente(Ingrediente ingrediente)
        {
            Console.WriteLine("[Entrega] Recibiendo ingrediente: " + ingrediente.Nombre);
            return true;
        }
    }

    // =========================================================
    //  CELDA
    // =========================================================
    /// <summary>
    /// Representa una celda individual dentro de la cuadricula del mapa.
    /// </summary>
    public class Celda
    {
        public TipoCelda Tipo    { get; private set; }
        public bool      Ocupada { get; set; }

        // Estacion de trabajo asociada a esta celda (puede ser null)
        public Estacion EstacionAsociada { get; set; }

        public Celda(TipoCelda tipo)
        {
            Tipo             = tipo;
            Ocupada          = false;
            EstacionAsociada = null;
        }

        /// <summary>
        /// Devuelve el caracter que representa visualmente esta celda en consola.
        /// </summary>
        public char ObtenerSimbolo()
        {
            switch (Tipo)
            {
                case TipoCelda.Vacia:         return '.';
                case TipoCelda.Pared:         return '#';
                case TipoCelda.Despensa:      return 'D';
                case TipoCelda.Cocina:        return 'C';
                case TipoCelda.TablaPicar:    return 'T';
                case TipoCelda.Freidora:      return 'F';
                case TipoCelda.EntregaPlatos: return 'E';
                default:                      return '?';
            }
        }
    }

    // =========================================================
    //  MAPA
    // =========================================================
    /// <summary>
    /// Cuadricula que representa la cocina. Almacena las celdas,
    /// las estaciones de trabajo y los chefs activos.
    /// </summary>
    public class Mapa
    {
        public int Filas { get; private set; }
        public int Cols  { get; private set; }

        private Celda[,] cuadricula;

        // ── Constructor ────────────────────────────────────────
        public Mapa(int filas, int cols)
        {
            Filas       = filas;
            Cols        = cols;
            cuadricula  = new Celda[filas, cols];

            // Inicializar todas las celdas como vacias
            for (int f = 0; f < filas; f++)
                for (int c = 0; c < cols; c++)
                    cuadricula[f, c] = new Celda(TipoCelda.Vacia);
        }

        // ── Configuracion de celdas ────────────────────────────
        public void EstablecerCelda(int fila, int col, TipoCelda tipo, Estacion estacion = null)
        {
            if (!CoordenadasValidas(fila, col)) return;
            cuadricula[fila, col] = new Celda(tipo);
            cuadricula[fila, col].EstacionAsociada = estacion;
        }

        public Celda ObtenerCelda(int fila, int col)
        {
            if (!CoordenadasValidas(fila, col)) return null;
            return cuadricula[fila, col];
        }

        // ── Validacion ─────────────────────────────────────────
        private bool CoordenadasValidas(int fila, int col)
        {
            return fila >= 0 && fila < Filas && col >= 0 && col < Cols;
        }

        /// <summary>
        /// Retorna true si la celda existe, no es pared
        /// y no hay otro chef ocupandola.
        /// </summary>
        public bool EsCeldaValida(int fila, int col)
        {
            if (!CoordenadasValidas(fila, col)) return false;
            Celda celda = cuadricula[fila, col];
            return celda.Tipo == TipoCelda.Vacia && !celda.Ocupada;
        }

        /// <summary>Actualiza la posicion de un chef en la cuadricula.</summary>
        public void ActualizarPosicionChef(Chef chef, int filaAnterior, int colAnterior)
        {
            if (CoordenadasValidas(filaAnterior, colAnterior))
                cuadricula[filaAnterior, colAnterior].Ocupada = false;

            if (CoordenadasValidas(chef.FilaActual, chef.ColActual))
                cuadricula[chef.FilaActual, chef.ColActual].Ocupada = true;
        }

        // ── Renderizado en consola ─────────────────────────────
        /// <summary>
        /// Dibuja el mapa en la consola, superponiendole los chefs.
        /// '*' = chef activo    'o' = chef inactivo
        /// </summary>
        public void Renderizar(List<Chef> chefs, Chef chefActivo)
        {
            // Encabezado de columnas
            Console.Write("   ");
            for (int c = 0; c < Cols; c++)
                Console.Write((c % 10) + " ");
            Console.WriteLine();

            for (int f = 0; f < Filas; f++)
            {
                Console.Write(f.ToString().PadLeft(2) + " ");
                for (int c = 0; c < Cols; c++)
                {
                    char simbolo = ObtenerSimboloConChefs(f, c, chefs, chefActivo);
                    Console.Write(simbolo + " ");
                }
                Console.WriteLine();
            }
        }

        private char ObtenerSimboloConChefs(int fila, int col, List<Chef> chefs, Chef chefActivo)
        {
            foreach (Chef chef in chefs)
            {
                if (chef.FilaActual == fila && chef.ColActual == col)
                    return (chef == chefActivo) ? '*' : 'o';
            }
            return cuadricula[fila, col].ObtenerSimbolo();
        }
    }

    // =========================================================
    //  FABRICA DE MAPAS — Crea los tres escenarios del juego
    // =========================================================
    public static class FabricaMapa
    {
        /// <summary>
        /// Escenario 1 — Cocina basica (8x10).
        /// Recetas sencillas con pocos ingredientes.
        /// </summary>
        public static Mapa CrearEscenario1()
        {
            Mapa mapa = new Mapa(8, 10);
            AgregarParedes(mapa);

            mapa.EstablecerCelda(1, 1, TipoCelda.Despensa,     new Despensa("Tomate",  new Vegetal("Tomate")));
            mapa.EstablecerCelda(1, 3, TipoCelda.Despensa,     new Despensa("Lechuga", new Vegetal("Lechuga")));
            mapa.EstablecerCelda(1, 5, TipoCelda.Despensa,     new Despensa("Pan",     new PanYBase("Pan")));
            mapa.EstablecerCelda(1, 7, TipoCelda.Despensa,     new Despensa("Carne",   new Proteina("Carne")));
            mapa.EstablecerCelda(3, 2, TipoCelda.TablaPicar,   new EstacionTrabajo("Tabla de Picar", TipoCelda.TablaPicar));
            mapa.EstablecerCelda(3, 5, TipoCelda.Cocina,       new EstacionTrabajo("Cocina",         TipoCelda.Cocina));
            mapa.EstablecerCelda(6, 5, TipoCelda.EntregaPlatos, new EstacionEntrega());

            Console.WriteLine("[Mapa] Escenario 1 cargado.");
            return mapa;
        }

        /// <summary>
        /// Escenario 2 — Cocina intermedia (9x12).
        /// Incluye freidora y mas despensas.
        /// </summary>
        public static Mapa CrearEscenario2()
        {
            Mapa mapa = new Mapa(9, 12);
            AgregarParedes(mapa);

            mapa.EstablecerCelda(1, 1,  TipoCelda.Despensa,     new Despensa("Papa",   new Vegetal("Papa")));
            mapa.EstablecerCelda(1, 3,  TipoCelda.Despensa,     new Despensa("Pollo",  new Proteina("Pollo")));
            mapa.EstablecerCelda(1, 5,  TipoCelda.Despensa,     new Despensa("Pan",    new PanYBase("Pan")));
            mapa.EstablecerCelda(1, 7,  TipoCelda.Despensa,     new Despensa("Tomate", new Vegetal("Tomate")));
            mapa.EstablecerCelda(1, 9,  TipoCelda.Despensa,     new Despensa("Queso",  new PanYBase("Queso")));
            mapa.EstablecerCelda(3, 2,  TipoCelda.TablaPicar,   new EstacionTrabajo("Tabla de Picar", TipoCelda.TablaPicar));
            mapa.EstablecerCelda(3, 5,  TipoCelda.Cocina,       new EstacionTrabajo("Cocina",         TipoCelda.Cocina));
            mapa.EstablecerCelda(3, 8,  TipoCelda.Freidora,     new EstacionTrabajo("Freidora",       TipoCelda.Freidora));
            mapa.EstablecerCelda(7, 6,  TipoCelda.EntregaPlatos, new EstacionEntrega());

            Console.WriteLine("[Mapa] Escenario 2 cargado.");
            return mapa;
        }

        /// <summary>
        /// Escenario 3 — Cocina avanzada (10x14).
        /// Mas ingredientes y estaciones; requiere coordinacion entre chefs.
        /// </summary>
        public static Mapa CrearEscenario3()
        {
            Mapa mapa = new Mapa(10, 14);
            AgregarParedes(mapa);

            mapa.EstablecerCelda(1, 1,  TipoCelda.Despensa,    new Despensa("Carne",   new Proteina("Carne")));
            mapa.EstablecerCelda(1, 3,  TipoCelda.Despensa,    new Despensa("Pollo",   new Proteina("Pollo")));
            mapa.EstablecerCelda(1, 5,  TipoCelda.Despensa,    new Despensa("Papa",    new Vegetal("Papa")));
            mapa.EstablecerCelda(1, 7,  TipoCelda.Despensa,    new Despensa("Tomate",  new Vegetal("Tomate")));
            mapa.EstablecerCelda(1, 9,  TipoCelda.Despensa,    new Despensa("Pan",     new PanYBase("Pan")));
            mapa.EstablecerCelda(1, 11, TipoCelda.Despensa,    new Despensa("Lechuga", new Vegetal("Lechuga")));
            mapa.EstablecerCelda(3, 2,  TipoCelda.TablaPicar,  new EstacionTrabajo("Tabla A",  TipoCelda.TablaPicar));
            mapa.EstablecerCelda(3, 6,  TipoCelda.TablaPicar,  new EstacionTrabajo("Tabla B",  TipoCelda.TablaPicar));
            mapa.EstablecerCelda(3, 4,  TipoCelda.Cocina,      new EstacionTrabajo("Cocina A", TipoCelda.Cocina));
            mapa.EstablecerCelda(3, 9,  TipoCelda.Cocina,      new EstacionTrabajo("Cocina B", TipoCelda.Cocina));
            mapa.EstablecerCelda(3, 11, TipoCelda.Freidora,    new EstacionTrabajo("Freidora", TipoCelda.Freidora));
            mapa.EstablecerCelda(8, 7,  TipoCelda.EntregaPlatos, new EstacionEntrega());

            Console.WriteLine("[Mapa] Escenario 3 cargado.");
            return mapa;
        }

        // ── Helpers ────────────────────────────────────────────
        private static void AgregarParedes(Mapa mapa)
        {
            for (int f = 0; f < mapa.Filas; f++)
            {
                mapa.EstablecerCelda(f, 0,           TipoCelda.Pared);
                mapa.EstablecerCelda(f, mapa.Cols-1, TipoCelda.Pared);
            }
            for (int c = 0; c < mapa.Cols; c++)
            {
                mapa.EstablecerCelda(0,            c, TipoCelda.Pared);
                mapa.EstablecerCelda(mapa.Filas-1, c, TipoCelda.Pared);
            }
        }
    }

    // =========================================================
    //  PANTALLA PRINCIPAL
    // =========================================================
    /// <summary>
    /// Gestiona los menus y la seleccion de escenario antes de
    /// iniciar la partida.
    /// </summary>
    public class PantallaPrincipal
    {
        // ── Pantalla de bienvenida ─────────────────────────────
        public void MostrarTitulo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
   ___  ____    __   _______   _____ _   _    _    ____ _  __
  / __)| __ )  / /  |  _  \ \ / / __| \ | |  / \  / ___| |/ /
 | (__ |  _ \ / /   | |_| |\ V /\__ \  \| | / _ \| |   | ' /
  \___)| |_) / /    |  _ <  | | |___/ |\  |/ ___ \ |___| . \
      |____/_/      |_| \_\ |_| |___/_| \_/_/   \_\____|_|\_\

            ==  TEC Edition  ==
");
            Console.ResetColor();
            Console.WriteLine("     Tecnologico de Costa Rica — Introduccion a la Programacion");
            Console.WriteLine();
        }

        // ── Menu principal ─────────────────────────────────────
        public int MostrarMenuPrincipal()
        {
            MostrarTitulo();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  +----------------------------+");
            Console.WriteLine("  |      MENU PRINCIPAL        |");
            Console.WriteLine("  +----------------------------+");
            Console.WriteLine("  |  [1] Jugar                 |");
            Console.WriteLine("  |  [2] Instrucciones         |");
            Console.WriteLine("  |  [3] Creditos              |");
            Console.WriteLine("  |  [4] Salir                 |");
            Console.WriteLine("  +----------------------------+");
            Console.ResetColor();
            Console.Write("\n  Elige una opcion: ");

            return LeerOpcion(1, 4);
        }

        // ── Seleccion de escenario ─────────────────────────────
        public int MostrarSeleccionEscenario()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  +------------------------------------+");
            Console.WriteLine("  |        SELECCION DE COCINA         |");
            Console.WriteLine("  +------------------------------------+");
            Console.WriteLine("  |  [1] Cocina Basica    -- Facil     |");
            Console.WriteLine("  |  [2] Cocina Gourmet   -- Medio     |");
            Console.WriteLine("  |  [3] Cocina Extrema   -- Dificil   |");
            Console.WriteLine("  +------------------------------------+");
            Console.ResetColor();
            Console.Write("\n  Elige un escenario: ");

            return LeerOpcion(1, 3);
        }

        // ── Instrucciones ──────────────────────────────────────
        public void MostrarInstrucciones()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("  ==========================================");
            Console.WriteLine("                COMO JUGAR");
            Console.WriteLine("  ==========================================");
            Console.WriteLine();
            Console.WriteLine("  MOVIMIENTO:");
            Console.WriteLine("    W  ->  Arriba         S  ->  Abajo");
            Console.WriteLine("    A  ->  Izquierda      D  ->  Derecha");
            Console.WriteLine();
            Console.WriteLine("  ACCIONES:");
            Console.WriteLine("    ESPACIO  ->  Recoger / Interactuar con estacion");
            Console.WriteLine("    TAB      ->  Cambiar de chef");
            Console.WriteLine();
            Console.WriteLine("  LEYENDA DEL MAPA:");
            Console.WriteLine("    D  Despensa       -- Obtén ingredientes");
            Console.WriteLine("    T  Tabla Picar    -- Prepara vegetales");
            Console.WriteLine("    C  Cocina         -- Cocina proteinas");
            Console.WriteLine("    F  Freidora       -- Frie papas");
            Console.WriteLine("    E  Entrega        -- Entrega el plato");
            Console.WriteLine("    *  Chef activo    o  Chef inactivo");
            Console.WriteLine("    #  Pared          .  Piso libre");
            Console.WriteLine();
            Console.WriteLine("  OBJETIVO:");
            Console.WriteLine("    Completa la mayor cantidad de recetas antes");
            Console.WriteLine("    de que se acabe el tiempo.");
            Console.WriteLine();
            Console.WriteLine("  ==========================================");
            Console.ResetColor();
            Console.Write("  Presiona cualquier tecla para volver...");
            Console.ReadKey(true);
        }

        // ── Creditos ───────────────────────────────────────────
        public void MostrarCreditos()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine("  ==========================================");
            Console.WriteLine("                  CREDITOS");
            Console.WriteLine("  ==========================================");
            Console.WriteLine();
            Console.WriteLine("  Proyecto 2 -- Introduccion a la Programacion");
            Console.WriteLine("  Tecnologico de Costa Rica");
            Console.WriteLine();
            Console.WriteLine("  Profesores:");
            Console.WriteLine("    - Leonardo Araya");
            Console.WriteLine("    - Santiago Ramirez");
            Console.WriteLine("    - Ellioth Ramirez");
            Console.WriteLine();
            Console.WriteLine("  Semestre 1, 2026");
            Console.WriteLine("  Inspirado en: Overcooked 2");
            Console.WriteLine("  ==========================================");
            Console.ResetColor();
            Console.Write("\n  Presiona cualquier tecla para volver...");
            Console.ReadKey(true);
        }

        // ── HUD durante la partida ─────────────────────────────
        /// <summary>
        /// Muestra el encabezado con tiempo restante, puntuacion
        /// del chef activo y su inventario.
        /// </summary>
        public void RenderizarHud(int tiempoRestante, Chef chefActivo, int escenario)
        {
            string enMano = chefActivo.IngredienteEnMano != null
                ? chefActivo.IngredienteEnMano.Nombre
                : "--";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  [ Escenario " + escenario + " ]  Tiempo: " + tiempoRestante + "s  |  Puntos: " + chefActivo.Puntos + "  |  " + chefActivo.Nombre + " lleva: " + enMano);
            Console.WriteLine("  " + new string('-', 60));
            Console.ResetColor();
        }

        // ── Pantalla de fin de juego ───────────────────────────
        public void MostrarFinJuego(List<Chef> chefs)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  +--------------------------------+");
            Console.WriteLine("  |       !TIEMPO AGOTADO!         |");
            Console.WriteLine("  +--------------------------------+");

            int totalPuntos = 0;
            foreach (Chef chef in chefs)
            {
                Console.WriteLine("  |  " + chef.Nombre.PadRight(15) + ": " + chef.Puntos.ToString().PadLeft(5) + " pts  |");
                totalPuntos += chef.Puntos;
            }

            Console.WriteLine("  +--------------------------------+");
            Console.WriteLine("  |  TOTAL          : " + totalPuntos.ToString().PadLeft(5) + " pts  |");
            Console.WriteLine("  +--------------------------------+");
            Console.ResetColor();
            Console.Write("\n  Presiona cualquier tecla para volver al menu...");
            Console.ReadKey(true);
        }

        // ── Helper de lectura ──────────────────────────────────
        private int LeerOpcion(int min, int max)
        {
            while (true)
            {
                string input = Console.ReadLine();
                int opcion;
                if (int.TryParse(input, out opcion) && opcion >= min && opcion <= max)
                    return opcion;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  Opcion invalida. Elige entre " + min + " y " + max + ": ");
                Console.ResetColor();
            }
        }
    }
}
