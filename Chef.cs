using System;
using System.Collections.Generic;

namespace CrazySnackRush
{
    // =========================================================
    //  CHEF.CS  — Personajes y sistema de movimiento
    //  Soporta modo un jugador (WASD + TAB) y multijugador local
    //  (Jugador 1: WASD + Espacio | Jugador 2: Flechas + Enter)
    // =========================================================

    /// <summary>
    /// Direcciones posibles de movimiento dentro del mapa.
    /// </summary>
    public enum Direccion
    {
        Arriba,
        Abajo,
        Izquierda,
        Derecha,
        Ninguna
    }

    /// <summary>
    /// Modos de juego disponibles.
    /// </summary>
    public enum ModoJuego
    {
        UnJugador,       // Un jugador controla ambos chefs con WASD; TAB para alternar
        MultiJugadorLocal // Jugador 1 = WASD + Espacio | Jugador 2 = Flechas + Enter
    }

    /// <summary>
    /// Esquema de controles asignado a un chef.
    /// Cada instancia define que teclas usa ese jugador.
    /// </summary>
    public class EsquemaControles
    {
        public ConsoleKey Arriba   { get; private set; }
        public ConsoleKey Abajo    { get; private set; }
        public ConsoleKey Izquierda{ get; private set; }
        public ConsoleKey Derecha  { get; private set; }
        public ConsoleKey Accion   { get; private set; }

        public EsquemaControles(ConsoleKey arriba, ConsoleKey abajo,
                                ConsoleKey izquierda, ConsoleKey derecha,
                                ConsoleKey accion)
        {
            Arriba    = arriba;
            Abajo     = abajo;
            Izquierda = izquierda;
            Derecha   = derecha;
            Accion    = accion;
        }

        // ── Esquemas predefinidos ──────────────────────────────

        /// <summary>WASD + Espacio — Jugador 1.</summary>
        public static EsquemaControles Jugador1()
        {
            return new EsquemaControles(
                ConsoleKey.W,
                ConsoleKey.S,
                ConsoleKey.A,
                ConsoleKey.D,
                ConsoleKey.Spacebar
            );
        }

        /// <summary>Flechas + Enter — Jugador 2.</summary>
        public static EsquemaControles Jugador2()
        {
            return new EsquemaControles(
                ConsoleKey.UpArrow,
                ConsoleKey.DownArrow,
                ConsoleKey.LeftArrow,
                ConsoleKey.RightArrow,
                ConsoleKey.Enter
            );
        }

        /// <summary>
        /// Interpreta una tecla segun este esquema.
        /// Retorna la direccion si es movimiento, o Ninguna si es accion u otra tecla.
        /// </summary>
        public Direccion InterpretarMovimiento(ConsoleKey tecla)
        {
            if (tecla == Arriba)    return Direccion.Arriba;
            if (tecla == Abajo)     return Direccion.Abajo;
            if (tecla == Izquierda) return Direccion.Izquierda;
            if (tecla == Derecha)   return Direccion.Derecha;
            return Direccion.Ninguna;
        }

        public bool EsAccion(ConsoleKey tecla)
        {
            return tecla == Accion;
        }

        public override string ToString()
        {
            return "Mov:[" + Arriba + "/" + Abajo + "/" + Izquierda + "/" + Derecha + "]  Accion:[" + Accion + "]";
        }
    }

    // =========================================================
    //  CHEF
    // =========================================================
    /// <summary>
    /// Representa a un chef dentro del juego.
    /// Puede moverse por el mapa, sostener un ingrediente
    /// e interactuar con las estaciones de trabajo.
    /// </summary>
    public class Chef
    {
        // ── Atributos ──────────────────────────────────────────
        public string Nombre { get; private set; }
        public int    Puntos { get; private set; }

        // Posicion actual en la cuadricula
        public int FilaActual { get; private set; }
        public int ColActual  { get; private set; }

        // Un chef solo puede sostener UN ingrediente a la vez
        public Ingrediente IngredienteEnMano { get; private set; }

        // En modo un jugador se usa EstaActivo para saber quien tiene el control
        public bool EstaActivo { get; private set; }

        // Esquema de controles propio de este chef (null en modo un jugador)
        public EsquemaControles Controles { get; private set; }

        // Ultima direccion de movimiento (util para ObtenerCeldaFrente)
        public Direccion UltimaDireccion { get; private set; }

        // ── Constructor ────────────────────────────────────────
        public Chef(string nombre, int filaInicial, int colInicial,
                    EsquemaControles controles = null)
        {
            Nombre            = nombre;
            Puntos            = 0;
            FilaActual        = filaInicial;
            ColActual         = colInicial;
            IngredienteEnMano = null;
            EstaActivo        = false;
            Controles         = controles;
            UltimaDireccion   = Direccion.Abajo;   // cara inicial
        }

        // ── Activacion (solo modo un jugador) ─────────────────
        public void Activar()    { EstaActivo = true;  }
        public void Desactivar() { EstaActivo = false; }

        // ── Puntuacion ─────────────────────────────────────────
        public void AgregarPuntos(int cantidad)
        {
            Puntos = Math.Max(0, Puntos + cantidad);
        }

        public void DescontarPuntos(int cantidad)
        {
            Puntos = Math.Max(0, Puntos - cantidad);
        }

        // ── Inventario ─────────────────────────────────────────
        /// <summary>Recoge un ingrediente si las manos estan vacias.</summary>
        public bool RecogerIngrediente(Ingrediente ingrediente)
        {
            if (IngredienteEnMano != null)
            {
                Console.WriteLine("[" + Nombre + "] Ya tengo '" + IngredienteEnMano.Nombre + "' en la mano.");
                return false;
            }
            IngredienteEnMano = ingrediente;
            Console.WriteLine("[" + Nombre + "] Recogió: " + IngredienteEnMano.Nombre);
            return true;
        }

        /// <summary>Suelta el ingrediente actual y lo retorna.</summary>
        public Ingrediente SoltarIngrediente()
        {
            if (IngredienteEnMano == null)
            {
                Console.WriteLine("[" + Nombre + "] No llevo nada en la mano.");
                return null;
            }
            Ingrediente temp  = IngredienteEnMano;
            IngredienteEnMano = null;
            Console.WriteLine("[" + Nombre + "] Soltó: " + temp.Nombre);
            return temp;
        }

        // ── Movimiento ─────────────────────────────────────────
        /// <summary>
        /// Mueve al chef en la direccion indicada si la celda destino
        /// esta libre y dentro de los limites del mapa.
        /// </summary>
        public bool Mover(Direccion dir, Mapa mapa)
        {
            if (dir == Direccion.Ninguna) return false;

            int filaAnterior = FilaActual;
            int colAnterior  = ColActual;
            int nuevaFila    = FilaActual;
            int nuevaCol     = ColActual;

            switch (dir)
            {
                case Direccion.Arriba:    nuevaFila--; break;
                case Direccion.Abajo:     nuevaFila++; break;
                case Direccion.Izquierda: nuevaCol--;  break;
                case Direccion.Derecha:   nuevaCol++;  break;
            }

            // Guardar ultima direccion aunque el movimiento falle
            // (para que ObtenerCeldaFrente siempre apunte bien)
            UltimaDireccion = dir;

            if (!mapa.EsCeldaValida(nuevaFila, nuevaCol))
            {
                Console.WriteLine("[" + Nombre + "] Movimiento bloqueado (" + dir + ").");
                return false;
            }

            FilaActual = nuevaFila;
            ColActual  = nuevaCol;
            mapa.ActualizarPosicionChef(this, filaAnterior, colAnterior);
            Console.WriteLine("[" + Nombre + "] Se movio " + dir + " -> (" + FilaActual + "," + ColActual + ")");
            return true;
        }

        /// <summary>
        /// Retorna la celda frente al chef segun su UltimaDireccion.
        /// Util para detectar con que estacion interactua al presionar Accion.
        /// </summary>
        public int[] ObtenerCeldaFrente()
        {
            switch (UltimaDireccion)
            {
                case Direccion.Arriba:    return new int[] { FilaActual - 1, ColActual };
                case Direccion.Abajo:     return new int[] { FilaActual + 1, ColActual };
                case Direccion.Izquierda: return new int[] { FilaActual, ColActual - 1 };
                case Direccion.Derecha:   return new int[] { FilaActual, ColActual + 1 };
                default:                  return new int[] { FilaActual, ColActual };
            }
        }

        // ── Representacion visual ──────────────────────────────
        public override string ToString()
        {
            string enMano = IngredienteEnMano != null ? IngredienteEnMano.Nombre : "nada";
            string estado = EstaActivo ? "[*]" : "[ ]";
            return estado + " " + Nombre + " | Pos:(" + FilaActual + "," + ColActual
                   + ") | Puntos:" + Puntos + " | Mano:" + enMano
                   + " | Cara:" + UltimaDireccion;
        }
    }

    // =========================================================
    //  RESULTADO DE TECLA
    //  Estructura simple para devolver lo que ocurrio al presionar
    //  una tecla, evitando usar object[] con casting manual.
    // =========================================================
    public class ResultadoTecla
    {
        public Chef      ChefAfectado    { get; set; }
        public Direccion Direccion       { get; set; }
        public bool      EsAccion        { get; set; }
        public bool      EsCambioChef    { get; set; }   // solo modo un jugador
        public bool      EsSalida        { get; set; }   // tecla Escape

        public ResultadoTecla()
        {
            Direccion    = Direccion.Ninguna;
            EsAccion     = false;
            EsCambioChef = false;
            EsSalida     = false;
        }
    }

    // =========================================================
    //  CONTROLADOR DE CHEFS
    //  Soporta ambos modos de juego en la misma clase.
    // =========================================================
    public class ControladorChefs
    {
        private List<Chef> chefs;
        private int        indiceActivo;    // solo relevante en modo un jugador
        private ModoJuego  modo;

        // ── Acceso publico ─────────────────────────────────────
        public ModoJuego Modo { get { return modo; } }

        /// <summary>Chef que tiene el control en modo un jugador.</summary>
        public Chef ChefActivo
        {
            get { return chefs[indiceActivo]; }
        }

        public List<Chef> TodosLosChefs { get { return chefs; } }

        // ── Constructor ────────────────────────────────────────
        public ControladorChefs(List<Chef> listaChefs, ModoJuego modoJuego)
        {
            if (listaChefs == null || listaChefs.Count == 0)
                throw new ArgumentException("Se necesita al menos un chef.");

            chefs        = listaChefs;
            modo         = modoJuego;
            indiceActivo = 0;

            if (modo == ModoJuego.UnJugador)
            {
                // Solo el primer chef empieza activo; los controles son compartidos
                chefs[0].Activar();
            }
            else
            {
                // Multijugador: todos activos siempre; cada uno tiene su esquema
                foreach (Chef chef in chefs)
                    chef.Activar();

                // Asignar controles predeterminados si no los tienen
                if (chefs.Count >= 1 && chefs[0].Controles == null)
                    AsignarControles(chefs[0], EsquemaControles.Jugador1());
                if (chefs.Count >= 2 && chefs[1].Controles == null)
                    AsignarControles(chefs[1], EsquemaControles.Jugador2());
            }
        }

        // ── Helpers privados ───────────────────────────────────
        private void AsignarControles(Chef chef, EsquemaControles esquema)
        {
            // Chef es inmutable en Controles por diseno; se usa reflexion
            // o se asigna en el constructor. Aqui se documenta el flujo esperado:
            // el desarrollador pasa el EsquemaControles directamente al crear el Chef.
            Console.WriteLine("[Control] " + chef.Nombre + " usa " + esquema);
        }

        // ── Cambio de personaje (solo modo un jugador) ─────────
        public void CambiarChef()
        {
            if (modo != ModoJuego.UnJugador)
            {
                Console.WriteLine("[Control] CambiarChef no aplica en multijugador local.");
                return;
            }
            chefs[indiceActivo].Desactivar();
            indiceActivo = (indiceActivo + 1) % chefs.Count;
            chefs[indiceActivo].Activar();
            Console.WriteLine("\n[Control] Ahora controlas a: " + ChefActivo.Nombre + "\n");
        }

        // ── Procesamiento de teclas ────────────────────────────

        /// <summary>
        /// Procesa una tecla presionada y determina a que chef afecta
        /// y que accion debe ejecutarse, segun el modo de juego activo.
        /// </summary>
        public ResultadoTecla ProcesarTecla(ConsoleKey tecla)
        {
            ResultadoTecla resultado = new ResultadoTecla();

            // Escape siempre cierra / pausa sin importar el modo
            if (tecla == ConsoleKey.Escape)
            {
                resultado.EsSalida = true;
                return resultado;
            }

            if (modo == ModoJuego.UnJugador)
                ProcesarTeclaUnJugador(tecla, resultado);
            else
                ProcesarTeclaMultijugador(tecla, resultado);

            return resultado;
        }

        // ── Modo un jugador ────────────────────────────────────
        private void ProcesarTeclaUnJugador(ConsoleKey tecla, ResultadoTecla resultado)
        {
            resultado.ChefAfectado = ChefActivo;

            switch (tecla)
            {
                case ConsoleKey.W:        resultado.Direccion = Direccion.Arriba;    break;
                case ConsoleKey.S:        resultado.Direccion = Direccion.Abajo;     break;
                case ConsoleKey.A:        resultado.Direccion = Direccion.Izquierda; break;
                case ConsoleKey.D:        resultado.Direccion = Direccion.Derecha;   break;
                case ConsoleKey.Spacebar: resultado.EsAccion  = true;                break;
                case ConsoleKey.Tab:      resultado.EsCambioChef = true;             break;
                // Tambien se aceptan flechas en modo un jugador para comodidad
                case ConsoleKey.UpArrow:    resultado.Direccion = Direccion.Arriba;    break;
                case ConsoleKey.DownArrow:  resultado.Direccion = Direccion.Abajo;     break;
                case ConsoleKey.LeftArrow:  resultado.Direccion = Direccion.Izquierda; break;
                case ConsoleKey.RightArrow: resultado.Direccion = Direccion.Derecha;   break;
            }
        }

        // ── Modo multijugador local ────────────────────────────
        private void ProcesarTeclaMultijugador(ConsoleKey tecla, ResultadoTecla resultado)
        {
            // Recorrer cada chef y verificar si la tecla pertenece a su esquema
            foreach (Chef chef in chefs)
            {
                if (chef.Controles == null) continue;

                Direccion dir = chef.Controles.InterpretarMovimiento(tecla);
                if (dir != Direccion.Ninguna)
                {
                    resultado.ChefAfectado = chef;
                    resultado.Direccion    = dir;
                    return;
                }

                if (chef.Controles.EsAccion(tecla))
                {
                    resultado.ChefAfectado = chef;
                    resultado.EsAccion     = true;
                    return;
                }
            }
            // Tecla no reconocida: resultado queda con ChefAfectado = null
        }

        // ── Resumen de controles para la pantalla de instrucciones ──
        public void ImprimirControles()
        {
            if (modo == ModoJuego.UnJugador)
            {
                Console.WriteLine("  Modo: UN JUGADOR");
                Console.WriteLine("  Movimiento : W A S D  (o flechas)");
                Console.WriteLine("  Accion     : Espacio");
                Console.WriteLine("  Cambiar chef: TAB");
            }
            else
            {
                Console.WriteLine("  Modo: MULTIJUGADOR LOCAL");
                for (int i = 0; i < chefs.Count; i++)
                {
                    Chef chef = chefs[i];
                    string esquema = chef.Controles != null ? chef.Controles.ToString() : "sin asignar";
                    Console.WriteLine("  " + chef.Nombre + " -> " + esquema);
                }
            }
        }
    }

    // =========================================================
    //  FABRICA DE CHEFS
    //  Crea los chefs ya configurados segun el modo elegido.
    // =========================================================
    public static class FabricaChefs
    {
        /// <summary>
        /// Crea dos chefs listos para modo un jugador.
        /// Ambos usan WASD; el jugador alterna entre ellos con TAB.
        /// </summary>
        public static List<Chef> CrearModoUnJugador()
        {
            List<Chef> lista = new List<Chef>();
            lista.Add(new Chef("Chef 1", 4, 2));   // sin esquema; controlador comparte WASD
            lista.Add(new Chef("Chef 2", 4, 7));
            return lista;
        }

        /// <summary>
        /// Crea dos chefs para multijugador local con controles independientes.
        /// Chef 1 -> WASD + Espacio
        /// Chef 2 -> Flechas + Enter
        /// </summary>
        public static List<Chef> CrearModoMultijugador()
        {
            List<Chef> lista = new List<Chef>();
            lista.Add(new Chef("Chef 1", 4, 2, EsquemaControles.Jugador1()));
            lista.Add(new Chef("Chef 2", 4, 7, EsquemaControles.Jugador2()));
            return lista;
        }
    }
}
