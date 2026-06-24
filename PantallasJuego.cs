using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

// ═══════════════════════════════════════════════════════════════════════════════
// MENU PRINCIPAL
// ═══════════════════════════════════════════════════════════════════════════════

public class MenuPrincipal : Form
{
    private int    cantidadJugadores = 1;
    private Button btnJugar;
    private Button btn1J;
    private Button btn2J;
    private Timer  timerAnim;
    private float  offsetOnda = 0f;
    private Panel  panelControles;

    public MenuPrincipal()
    {
        Text            = "Crazy Snack Rush TEC";
        Size            = new Size(800, 620);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(15, 10, 30);
        DoubleBuffered  = true;

        CrearUI();
        IniciarAnimacion();
    }

    private void CrearUI()
    {
        var lblTitulo = new Label
        {
            Text      = "CRAZY SNACK RUSH TEC",
            ForeColor = Color.FromArgb(255, 210, 60),
            Font      = new Font("Segoe UI", 26, FontStyle.Bold),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 70, 800, 60),
            BackColor = Color.Transparent
        };

        var lblSub = new Label
        {
            Text      = "La version didactica",
            ForeColor = Color.FromArgb(180, 150, 255),
            Font      = new Font("Segoe UI", 12, FontStyle.Italic),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 134, 800, 28),
            BackColor = Color.Transparent
        };

        var lblModo = new Label
        {
            Text      = "SELECCIONAR MODO",
            ForeColor = Color.FromArgb(180, 180, 180),
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 230, 800, 26),
            BackColor = Color.Transparent
        };

        btn1J = CrearBotonModo("1 JUGADOR",   new Rectangle(222, 265, 155, 52));
        btn2J = CrearBotonModo("2 JUGADORES", new Rectangle(425, 265, 155, 52));
        btn1J.Click += (s, e) => SeleccionarModo(1);
        btn2J.Click += (s, e) => SeleccionarModo(2);

        panelControles = new Panel
        {
            Bounds    = new Rectangle(140, 335, 520, 90),
            BackColor = Color.Transparent
        };
        panelControles.Paint += DibujarControles;

        btnJugar = new Button
        {
            Text      = "  JUGAR",
            Bounds    = new Rectangle(275, 448, 250, 55),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(55, 170, 85),
            Cursor    = Cursors.Hand
        };
        btnJugar.FlatAppearance.BorderSize = 0;
        btnJugar.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 200, 105);
        btnJugar.Click += (s, e) =>
        {
            timerAnim.Stop();
            var pj = new PantallaJuego(cantidadJugadores);
            pj.FormClosed += (s2, e2) => Close();
            Hide();
            pj.Show();
        };

        var btnSalir = new Button
        {
            Text      = "x  SALIR",
            Bounds    = new Rectangle(275, 515, 250, 36),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(160, 160, 160),
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand
        };
        btnSalir.FlatAppearance.BorderSize = 0;
        btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 80, 80);
        btnSalir.Click += (s, e) => Application.Exit();

        Controls.AddRange(new Control[]
        {
            lblTitulo, lblSub, lblModo,
            btn1J, btn2J, panelControles,
            btnJugar, btnSalir
        });

        SeleccionarModo(1);
    }

    private Button CrearBotonModo(string texto, Rectangle bounds)
    {
        var btn = new Button
        {
            Text      = texto,
            Bounds    = bounds,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(45, 35, 80),
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = Color.FromArgb(100, 80, 160);
        btn.FlatAppearance.BorderSize  = 2;
        return btn;
    }

    private void SeleccionarModo(int j)
    {
        cantidadJugadores = j;
        btn1J.BackColor = j == 1 ? Color.FromArgb(80, 50, 160) : Color.FromArgb(45, 35, 80);
        btn2J.BackColor = j == 2 ? Color.FromArgb(80, 50, 160) : Color.FromArgb(45, 35, 80);
        btn1J.FlatAppearance.BorderColor = j == 1 ? Color.FromArgb(180, 150, 255) : Color.FromArgb(100, 80, 160);
        btn2J.FlatAppearance.BorderColor = j == 2 ? Color.FromArgb(180, 150, 255) : Color.FromArgb(100, 80, 160);
        panelControles.Invalidate();
    }

    private void DibujarControles(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var path = RoundedRect(new Rectangle(0, 0, panelControles.Width, panelControles.Height), 10))
        using (var b = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
            g.FillPath(b, path);

        var ft  = new Font("Segoe UI", 8, FontStyle.Bold);
        var ftx = new Font("Consolas", 9);

        g.DrawString("CONTROLES", ft, Brushes.Gray, 10, 8);

        g.DrawString("Jugador 1:", ft, new SolidBrush(Color.FromArgb(100, 200, 255)), 10, 28);
        g.DrawString("W/A/S/D mover   E accion", ftx, Brushes.White, 88, 28);

        if (cantidadJugadores == 2)
        {
            g.DrawString("Jugador 2:", ft, new SolidBrush(Color.FromArgb(255, 160, 100)), 10, 55);
            g.DrawString("Flechas mover   Enter accion", ftx, Brushes.White, 88, 55);
        }
        else
        {
            g.DrawString("Jugador 2:", ft, new SolidBrush(Color.FromArgb(80, 80, 80)), 10, 55);
            g.DrawString("(desactivado - selecciona 2 Jugadores)", ftx, new SolidBrush(Color.FromArgb(80, 80, 80)), 88, 55);
        }
    }

    private void IniciarAnimacion()
    {
        timerAnim = new Timer { Interval = 40 };
        timerAnim.Tick += (s, e) =>
        {
            offsetOnda += 0.08f;
            Invalidate(new Rectangle(0, 0, 800, 175));
        };
        timerAnim.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        using (var grad = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(15, 10, 30), Color.FromArgb(30, 20, 60), LinearGradientMode.Vertical))
            g.FillRectangle(grad, ClientRectangle);

        DibujarOnda(g, 60,  offsetOnda,       Color.FromArgb(50, 255, 180, 50));
        DibujarOnda(g, 50,  offsetOnda + 1.2f, Color.FromArgb(30, 180, 100, 255));

        var ems = new[] { "🍔", "🍕", "🥗", "🍜", "🌮", "🍱" };
        using var fEm = new Font("Segoe UI Emoji", 20);
        for (int i = 0; i < ems.Length; i++)
        {
            float fy = (float)(175 + Math.Sin(offsetOnda + i * 1.1f) * 6);
            g.DrawString(ems[i], fEm, Brushes.White, 40 + i * 120f, fy);
        }
    }

    private void DibujarOnda(Graphics g, int y, float offset, Color color)
    {
        var pts = new List<PointF>();
        for (int i = 0; i <= 800; i += 4)
            pts.Add(new PointF(i, y + (float)Math.Sin(i * 0.02f + offset) * 12));
        if (pts.Count > 1)
            using (var p = new Pen(color, 2.5f))
                g.DrawLines(p, pts.ToArray());
    }

    private GraphicsPath RoundedRect(Rectangle r, int rad)
    {
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
        path.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
        path.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// PANTALLA DE JUEGO
// ═══════════════════════════════════════════════════════════════════════════════

public class PantallaJuego : Form
{
    // ── Dimensiones ─────────────────────────────────────────────────────────
    private const int TamCelda   = 52;
    private const int Columnas   = 15;
    private const int Filas      = 11;
    private const int AlturaHUD  = 100;
    private const int AlturaRecetas = 110;
    private const int TiempoPartida = 120;

    // ── Estado del juego ────────────────────────────────────────────────────
    private List<Chef>     chefs    = new List<Chef>();
    private int            chefActivo = 0;
    private int            segundosRestantes;
    private int            puntajeTotal = 0;
    private readonly Random rng = new Random();

    // ── Mapa e instancias de estaciones ─────────────────────────────────────
    // 0=piso, 1=pared, 2..N=indice en lista estaciones, -1=entrega
    private int[,]          mapaGrid;
    private List<Estacion>  estaciones = new List<Estacion>();
    // Posicion (fila,col) de cada estacion en el mapa
    private List<(int Fila, int Col)> posEstaciones = new List<(int, int)>();

    // Despensas: indice en estaciones -> Despensa
    private List<int> indicesDespensas = new List<int>();

    // Posiciones de entrega
    private List<(int Fila, int Col)> posEntregas = new List<(int, int)>();

    // ── Recetas activas ─────────────────────────────────────────────────────
    private List<Receta> recetasActivas  = new List<Receta>();
    private float        timerGenerarReceta = 0f;
    private const float  IntervaloReceta = 15f;
    private const int    MaxRecetasActivas = 3;

    // ── Panels y timer ──────────────────────────────────────────────────────
    private Panel panelHUD;
    private Panel panelMapa;
    private Panel panelRecetas;
    private Timer timerJuego;   // 1 segundo
    private Timer timerLogica;  // 100ms para preparacion de estaciones

    // ── Fuentes reutilizables ───────────────────────────────────────────────
    private Font fuenteEmoji;
    private Font fuenteChef;

    public PantallaJuego(int cantJugadores)
    {
        segundosRestantes = TiempoPartida;
        fuenteEmoji = new Font("Segoe UI Emoji", 14);
        fuenteChef  = new Font("Segoe UI Emoji", 16);

        ConfigurarVentana();
        ConstruirMapa();
        CrearChefs(cantJugadores);
        CrearPaneles();
        IniciarTimers();

        // Primera receta inmediata
        GenerarNuevaReceta();
    }

    // ── Setup ───────────────────────────────────────────────────────────────

    private void ConfigurarVentana()
    {
        Text            = "Crazy Snack Rush TEC";
        Size            = new Size(Columnas * TamCelda + 16,
                                   Filas * TamCelda + AlturaHUD + AlturaRecetas + 39);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(20, 15, 40);
        KeyPreview      = true;
        KeyDown        += OnKeyDown;
    }

    private void ConstruirMapa()
    {
        // Valores del grid: 0=piso libre, 1=pared, 2..=indice de estacion en lista, 99=entrega
        // Construimos el mapa y registramos estaciones en orden de aparicion
        //
        // Leyenda visual:
        //  D = Despensa (varios ingredientes distintos)
        //  C = Cocina
        //  T = Tabla de cortar
        //  F = Freidora
        //  E = Entrega

        // Primero definimos que despensa va en cada slot (fila1, cols 1,3,5,7,9,11,13)
        // Ingredientes disponibles para las despensas del mapa
        var ingredientesDespensa = new[]
        {
            "Lechuga",
            "Tomate",
            "Pan de Hamburguesa",
            "Pan Integral",
            "Carne de Res",
            "Pollo",
            "Papas Fritas"
        };

        // Crear instancias de estaciones en orden que aparecen en el grid
        // indice 2 = primera estacion (despensa Lechuga) etc.

        // Despensas (7) en fila 1: cols 1,3,5,7,9,11,13
        for (int i = 0; i < 7; i++)
        {
            string nombre = ingredientesDespensa[i];
            var d = new Despensa(nombre, CatalogoRecetas.IngredientesPorDespensa[nombre]);
            estaciones.Add(d);
            indicesDespensas.Add(estaciones.Count - 1);
            posEstaciones.Add((1, 1 + i * 2));
        }

        // Cocinas (2): fila 3 col 1, fila 4 col 1
        estaciones.Add(new Cocina()); posEstaciones.Add((3, 1));
        estaciones.Add(new Cocina()); posEstaciones.Add((4, 1));

        // Tablas (2): fila 3 col 13, fila 4 col 13
        estaciones.Add(new TablaDeCortar()); posEstaciones.Add((3, 13));
        estaciones.Add(new TablaDeCortar()); posEstaciones.Add((4, 13));

        // Freidora (1): fila 7 col 1
        estaciones.Add(new Freidora()); posEstaciones.Add((7, 1));

        // Tabla extra: fila 7 col 13
        estaciones.Add(new TablaDeCortar()); posEstaciones.Add((7, 13));

        // Entrega: fila 10, cols 5,6,7,8,9
        posEntregas = new List<(int, int)>
        {
            (10,5),(10,6),(10,7),(10,8),(10,9)
        };

        // Construir grid numerico
        mapaGrid = new int[Filas, Columnas];

        // Paredes perimetrales
        for (int f = 0; f < Filas; f++)
            for (int c = 0; c < Columnas; c++)
                mapaGrid[f, c] = (f == 0 || c == 0 || c == Columnas - 1) ? 1 : 0;

        // Fila inferior con paredes excepto zona entrega
        for (int c = 0; c < Columnas; c++)
            mapaGrid[Filas - 1, c] = 1;

        // Colocar estaciones en el grid (valor = indice + 2)
        for (int i = 0; i < estaciones.Count; i++)
        {
            var (f, c) = posEstaciones[i];
            mapaGrid[f, c] = i + 2;
        }

        // Zona de entrega (valor 99)
        foreach (var (f, c) in posEntregas)
            mapaGrid[f, c] = 99;
    }

    private void CrearChefs(int cantidad)
    {
        chefs.Add(new Chef(
            "Chef Azul", 4, 5,
            Color.FromArgb(80, 140, 255), "A",
            Keys.W, Keys.S, Keys.A, Keys.D, Keys.E));

        if (cantidad == 2)
            chefs.Add(new Chef(
                "Chef Naranja", 10, 5,
                Color.FromArgb(255, 140, 50), "B",
                Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Enter));
    }

    private void CrearPaneles()
    {
        int anchoMapa = Columnas * TamCelda;

        panelHUD = new Panel
        {
            Bounds    = new Rectangle(0, 0, anchoMapa, AlturaHUD),
            BackColor = Color.FromArgb(22, 18, 48)
        };
        panelHUD.Paint += PintarHUD;

        panelMapa = new Panel
        {
            Bounds    = new Rectangle(0, AlturaHUD, anchoMapa, Filas * TamCelda),
            BackColor = Color.FromArgb(30, 24, 55)
        };
        panelMapa.Paint += PintarMapa;

        panelRecetas = new Panel
        {
            Bounds    = new Rectangle(0, AlturaHUD + Filas * TamCelda, anchoMapa, AlturaRecetas),
            BackColor = Color.FromArgb(18, 14, 40)
        };
        panelRecetas.Paint += PintarRecetas;

        Controls.AddRange(new Control[] { panelHUD, panelMapa, panelRecetas });
    }

    private void IniciarTimers()
    {
        // Timer principal: 1 segundo, cuenta regresiva
        timerJuego = new Timer { Interval = 1000 };
        timerJuego.Tick += (s, e) =>
        {
            segundosRestantes--;
            panelHUD.Invalidate();
            if (segundosRestantes <= 0)
            {
                timerJuego.Stop();
                timerLogica.Stop();
                MostrarResultados();
            }
        };
        timerJuego.Start();

        // Timer de logica: 100ms, actualiza estaciones y recetas
        timerLogica = new Timer { Interval = 100 };
        timerLogica.Tick += (s, e) =>
        {
            float dt = 0.1f;

            // Actualizar estaciones de trabajo
            foreach (var est in estaciones)
                if (est is not Despensa && est is not EstacionEntrega)
                    est.Actualizar(dt);

            // Actualizar recetas
            bool cambioRecetas = false;
            for (int i = recetasActivas.Count - 1; i >= 0; i--)
            {
                bool expiro = recetasActivas[i].Actualizar(dt);
                if (expiro)
                {
                    puntajeTotal = Math.Max(0, puntajeTotal - recetasActivas[i].PuntosBase);
                    recetasActivas.RemoveAt(i);
                    cambioRecetas = true;
                }
            }

            // Generar nuevas recetas periodicamente
            timerGenerarReceta += dt;
            if (timerGenerarReceta >= IntervaloReceta)
            {
                timerGenerarReceta = 0f;
                GenerarNuevaReceta();
                cambioRecetas = true;
            }

            panelMapa.Invalidate();
            if (cambioRecetas) panelRecetas.Invalidate();
        };
        timerLogica.Start();
    }

    private void GenerarNuevaReceta()
    {
        if (recetasActivas.Count >= MaxRecetasActivas) return;
        recetasActivas.Add(CatalogoRecetas.GenerarRecetaAleatoria());
        panelRecetas.Invalidate();
    }

    // ── Dibujo HUD ──────────────────────────────────────────────────────────

    private void PintarHUD(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode   = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        int w = panelHUD.Width;

        using (var grad = new LinearGradientBrush(new Rectangle(0, 0, w, AlturaHUD),
            Color.FromArgb(32, 26, 62), Color.FromArgb(20, 16, 44), LinearGradientMode.Vertical))
            g.FillRectangle(grad, 0, 0, w, AlturaHUD);

        using (var p = new Pen(Color.FromArgb(70, 160, 140, 255), 1))
            g.DrawLine(p, 0, AlturaHUD - 1, w, AlturaHUD - 1);

        // Temporizador
        string tiempo = $"{segundosRestantes / 60:D2}:{segundosRestantes % 60:D2}";
        var colTiempo = segundosRestantes <= 15
            ? Color.FromArgb(255, 80, 80)
            : Color.FromArgb(255, 210, 50);

        using var fGrande = new Font("Segoe UI", 28, FontStyle.Bold);
        using var fMedia  = new Font("Segoe UI",  9, FontStyle.Bold);
        using var fChica  = new Font("Segoe UI",  8);

        var fmt = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        g.DrawString(tiempo, fGrande, new SolidBrush(colTiempo),
            new RectangleF(w / 2f - 75, 8, 150, 52), fmt);
        g.DrawString("TIEMPO", fMedia, new SolidBrush(Color.FromArgb(130, 130, 170)),
            new RectangleF(w / 2f - 40, 60, 80, 18), fmt);

        // Puntaje total
        g.DrawString($"Total: {puntajeTotal} pts", fMedia,
            new SolidBrush(Color.FromArgb(160, 255, 160)),
            new RectangleF(w / 2f - 60, 76, 120, 18), fmt);

        // Info chefs
        for (int i = 0; i < chefs.Count; i++)
        {
            float bx = i == 0 ? 8f : w - 258f;

            if (i == chefActivo)
            {
                using var bAct = new SolidBrush(Color.FromArgb(28, 140, 120, 255));
                g.FillRoundedRectangle(bAct, new Rectangle((int)bx - 4, 6, 250, 86), 8);
                using var pAct = new Pen(Color.FromArgb(100, 160, 140, 255), 1.5f);
                g.DrawRoundedRectangle(pAct, new Rectangle((int)bx - 4, 6, 250, 86), 8);
            }

            // Circulo de color
            using (var bCol = new SolidBrush(chefs[i].Color))
                g.FillEllipse(bCol, bx, 14, 20, 20);

            g.DrawString(chefs[i].Nombre, fMedia,
                new SolidBrush(chefs[i].Color), bx + 26, 12);

            g.DrawString($"Puntos: {chefs[i].Puntos}", fChica,
                new SolidBrush(Color.FromArgb(200, 200, 200)), bx + 26, 32);

            string itemTxt = chefs[i].IngredienteEnMano != null
                ? $"{chefs[i].IngredienteEnMano.Emoji} {chefs[i].IngredienteEnMano.Nombre} [{chefs[i].IngredienteEnMano.Estado}]"
                : "Manos vacias";
            var colItem = chefs[i].IngredienteEnMano?.EstaQuemado() == true
                ? Color.FromArgb(255, 100, 80)
                : Color.FromArgb(160, 240, 160);
            g.DrawString(itemTxt, fChica, new SolidBrush(colItem), bx + 26, 50);

            g.DrawString(chefs[i].ObtenerControles(),
                new Font("Segoe UI", 7), new SolidBrush(Color.FromArgb(90, 90, 120)),
                bx + 26, 68);

            if (i == chefActivo)
                g.DrawString("ACTIVO", new Font("Segoe UI", 7, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(100, 255, 170)), bx, 68);
        }
    }

    // ── Dibujo Mapa ─────────────────────────────────────────────────────────

    private void PintarMapa(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (int f = 0; f < Filas; f++)
        {
            for (int c = 0; c < Columnas; c++)
            {
                int x    = c * TamCelda;
                int y    = f * TamCelda;
                int val  = mapaGrid[f, c];
                var rect = new Rectangle(x, y, TamCelda, TamCelda);

                if (val == 0)
                {
                    // Piso
                    using var grad = new LinearGradientBrush(rect,
                        Color.FromArgb(42, 35, 75), Color.FromArgb(35, 28, 62),
                        LinearGradientMode.Vertical);
                    g.FillRectangle(grad, rect);
                    using var lp = new Pen(Color.FromArgb(25, 255, 255, 255), 0.5f);
                    g.DrawRectangle(lp, rect);
                }
                else if (val == 1)
                {
                    // Pared
                    using var b = new SolidBrush(Color.FromArgb(22, 18, 48));
                    g.FillRectangle(b, rect);
                    using var lp = new Pen(Color.FromArgb(55, 45, 90), 1);
                    g.DrawRectangle(lp, rect);
                }
                else if (val == 99)
                {
                    // Entrega
                    DibujarEstacion(g, rect, Color.FromArgb(60, 100, 200), "[]", "ENTREGA", null);
                }
                else if (val >= 2)
                {
                    int idx = val - 2;
                    if (idx < estaciones.Count)
                        DibujarEstacionObj(g, rect, estaciones[idx]);
                }
            }
        }

        // Chefs
        foreach (var ch in chefs)
            ch.Dibujar(g, TamCelda, fuenteChef);

        // Instruccion
        using var fInstr = new Font("Segoe UI", 7.5f);
        string cambio = chefs.Count > 1 ? "   Tab: cambiar chef" : "";
        g.DrawString($"Esc: menu{cambio}", fInstr,
            new SolidBrush(Color.FromArgb(70, 70, 90)), 4, Filas * TamCelda - 16);
    }

    private void DibujarEstacionObj(Graphics g, Rectangle rect, Estacion est)
    {
        // Leyenda del nombre del ingrediente para despensas
        string etiq = est is Despensa d ? d.NombreIngrediente : est.Nombre;
        DibujarEstacion(g, rect, est.ColorUI, est.Emoji, etiq, est);
    }

    private void DibujarEstacion(Graphics g, Rectangle rect, Color col, string emoji, string etiq, Estacion est)
    {
        using (var b = new SolidBrush(Color.FromArgb(55, col.R, col.G, col.B)))
            g.FillRectangle(b, rect);
        using (var lp = new Pen(col, 2))
            g.DrawRectangle(lp, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2));

        var fmt = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // Emoji de la estacion
        g.DrawString(emoji, fuenteEmoji, Brushes.White,
            new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - 20), fmt);

        // Si tiene ingrediente en proceso, mostrar su emoji
        if (est?.IngredienteActual != null)
        {
            var ing = est.IngredienteActual;
            var colIng = ing.EstaQuemado()
                ? Color.FromArgb(255, 80, 50)
                : ing.EstaListo()
                    ? Color.FromArgb(80, 255, 80)
                    : Color.FromArgb(255, 220, 80);

            g.DrawString(ing.Emoji,
                new Font("Segoe UI Emoji", 10),
                new SolidBrush(colIng),
                rect.Right - 18, rect.Y + 2);

            // Barra de progreso
            if (ing.EnPreparacion && !ing.EstaListo())
            {
                int bw = (int)((rect.Width - 6) * ing.ProgresoPreparacion);
                g.FillRectangle(Brushes.DimGray,
                    rect.X + 3, rect.Bottom - 8, rect.Width - 6, 5);
                using var bProg = new SolidBrush(
                    ing.ProgresoPreparacion > 0.8f ? Color.FromArgb(255, 80, 80) : Color.FromArgb(80, 200, 80));
                g.FillRectangle(bProg, rect.X + 3, rect.Bottom - 8, bw, 5);
            }
            else if (ing.EstaQuemado())
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 255, 50, 50)),
                    rect.X + 3, rect.Bottom - 8, rect.Width - 6, 5);
            }
            else if (ing.EstaListo())
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 50, 220, 80)),
                    rect.X + 3, rect.Bottom - 8, rect.Width - 6, 5);
            }
        }

        // Etiqueta inferior del nombre
        using var fEt = new Font("Segoe UI", 5f, FontStyle.Bold);
        g.DrawString(etiq, fEt,
            new SolidBrush(Color.FromArgb(210, 255, 255, 255)),
            new RectangleF(rect.X, rect.Bottom - 20, rect.Width, 14), fmt);
    }

    // ── Dibujo panel recetas ─────────────────────────────────────────────────

    private void PintarRecetas(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode   = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        int w = panelRecetas.Width;

        using (var grad = new LinearGradientBrush(new Rectangle(0, 0, w, AlturaRecetas),
            Color.FromArgb(18, 14, 40), Color.FromArgb(24, 18, 50), LinearGradientMode.Vertical))
            g.FillRectangle(grad, 0, 0, w, AlturaRecetas);

        using var fTit = new Font("Segoe UI", 8, FontStyle.Bold);
        using var fNom = new Font("Segoe UI", 9, FontStyle.Bold);
        using var fIng = new Font("Segoe UI", 7);

        g.DrawString("ORDENES ACTIVAS", fTit,
            new SolidBrush(Color.FromArgb(130, 130, 170)), 8, 4);

        if (recetasActivas.Count == 0)
        {
            g.DrawString("Esperando ordenes...", fNom,
                new SolidBrush(Color.FromArgb(80, 80, 100)),
                new RectangleF(0, 40, w, 40),
                new StringFormat { Alignment = StringAlignment.Center });
            return;
        }

        int cardW = (w - 16) / Math.Max(recetasActivas.Count, 1);
        cardW = Math.Min(cardW, 245);

        for (int i = 0; i < recetasActivas.Count; i++)
        {
            var rec = recetasActivas[i];
            int cx  = 8 + i * (cardW + 4);
            int cy  = 18;

            // Color de urgencia
            float urgencia = rec.ProgresoTiempo;
            var colCard = urgencia > 0.75f
                ? Color.FromArgb(140, 60, 40)
                : Color.FromArgb(40, 35, 80);

            g.FillRoundedRectangle(new SolidBrush(colCard),
                new Rectangle(cx, cy, cardW, AlturaRecetas - cy - 4), 8);
            g.DrawRoundedRectangle(
                new Pen(urgencia > 0.75f
                    ? Color.FromArgb(220, 80, 60)
                    : Color.FromArgb(80, 70, 140), 1.5f),
                new Rectangle(cx, cy, cardW, AlturaRecetas - cy - 4), 8);

            // Nombre y emoji
            g.DrawString($"{rec.Emoji} {rec.Nombre}", fNom,
                new SolidBrush(Color.FromArgb(240, 220, 120)),
                cx + 6, cy + 4);

            // Puntos y progreso de tiempo
            g.DrawString($"{rec.PuntosActual} pts", fIng,
                new SolidBrush(Color.FromArgb(160, 255, 160)),
                cx + 6, cy + 20);

            // Barra de tiempo
            int bw = cardW - 12;
            int lleno = (int)(bw * (1f - rec.ProgresoTiempo));
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 80)),
                cx + 6, cy + 34, bw, 5);
            if (lleno > 0)
            {
                var colBarra = urgencia > 0.75f
                    ? Color.FromArgb(255, 80, 60)
                    : Color.FromArgb(80, 200, 100);
                g.FillRectangle(new SolidBrush(colBarra), cx + 6, cy + 34, lleno, 5);
            }

            // Lista de ingredientes requeridos
            int iy = cy + 44;
            foreach (var req in rec.Ingredientes)
            {
                string tick = rec.IngredientesEntregados > 0 ? "v" : "o";
                g.DrawString($"{tick} {req.Nombre} ({req.EstadoRequerido})", fIng,
                    new SolidBrush(Color.FromArgb(190, 190, 210)),
                    cx + 4, iy);
                iy += 12;
                if (iy > AlturaRecetas - 8) break;
            }
        }
    }

    // ── Input ───────────────────────────────────────────────────────────────

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            timerJuego.Stop();
            timerLogica.Stop();
            if (MessageBox.Show("Volver al menu principal?", "Crazy Snack Rush",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close(); return;
            }
            timerJuego.Start();
            timerLogica.Start();
            return;
        }

        if (e.KeyCode == Keys.Tab && chefs.Count > 1)
        {
            chefActivo = (chefActivo + 1) % chefs.Count;
            panelHUD.Invalidate();
            e.Handled = true;
            return;
        }

        bool accion = false;
        bool proc = chefs[chefActivo].ProcesarTecla(
            e.KeyCode, Columnas, Filas, mapaGrid, ref accion);

        if (proc)
        {
            if (accion) ProcesarAccion(chefs[chefActivo]);
            panelMapa.Invalidate();
            panelHUD.Invalidate();
            e.Handled = true;
        }
    }

    // ── Logica de accion ────────────────────────────────────────────────────

    private void ProcesarAccion(Chef chef)
    {
        int colF  = chef.Col + chef.DirX;
        int filaF = chef.Fila + chef.DirY;

        if (colF < 0 || colF >= Columnas || filaF < 0 || filaF >= Filas) return;

        int val = mapaGrid[filaF, colF];

        // Zona de entrega
        if (val == 99)
        {
            ProcesarEntrega(chef);
            return;
        }

        if (val < 2) return;
        int idx = val - 2;
        if (idx >= estaciones.Count) return;

        var est = estaciones[idx];

        // Despensa
        if (est is Despensa desp)
        {
            if (chef.IngredienteEnMano == null)
            {
                chef.IngredienteEnMano = desp.EntregarIngrediente();
                panelHUD.Invalidate();
            }
            return;
        }

        // Estaciones de trabajo
        if (chef.IngredienteEnMano != null && est.IngredienteActual == null)
        {
            // Depositar ingrediente en la estacion
            bool ok = est.IntentarDepositar(chef.IngredienteEnMano);
            if (ok)
            {
                chef.IngredienteEnMano = null;
                panelHUD.Invalidate();
                panelMapa.Invalidate();
            }
            // Si no acepta el ingrediente, no pasa nada
            return;
        }

        if (chef.IngredienteEnMano == null && est.IngredienteActual != null)
        {
            // Retirar ingrediente de la estacion si ya esta listo o quemado
            var ing = est.IngredienteActual;
            if (ing.EstaListo() || ing.EstaQuemado() || !ing.EnPreparacion)
            {
                chef.IngredienteEnMano = est.Retirar();
                panelHUD.Invalidate();
                panelMapa.Invalidate();
            }
            return;
        }
    }

    private void ProcesarEntrega(Chef chef)
    {
        if (chef.IngredienteEnMano == null) return;

        var ing = chef.IngredienteEnMano;

        if (ing.EstaQuemado())
        {
            // Ingrediente quemado: no vale nada, se descarta
            chef.IngredienteEnMano = null;
            panelHUD.Invalidate();
            return;
        }

        // Intentar agregar a alguna receta activa
        for (int i = 0; i < recetasActivas.Count; i++)
        {
            bool agregado = recetasActivas[i].AgregarIngrediente(ing);
            if (agregado)
            {
                chef.IngredienteEnMano = null;
                panelHUD.Invalidate();

                if (recetasActivas[i].EstaCompleta())
                {
                    int pts = recetasActivas[i].PuntosActual;
                    chef.Puntos   += pts;
                    puntajeTotal  += pts;
                    recetasActivas.RemoveAt(i);
                    panelRecetas.Invalidate();
                    panelHUD.Invalidate();
                }
                return;
            }
        }

        // No coincidio con ninguna receta: descartado
        chef.IngredienteEnMano = null;
        panelHUD.Invalidate();
    }

    // ── Resultados ──────────────────────────────────────────────────────────

    private void MostrarResultados()
    {
        var r = new PantallaResultados(chefs, puntajeTotal);
        r.FormClosed += (s, e) => Close();
        Hide();
        r.Show();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        fuenteEmoji?.Dispose();
        fuenteChef?.Dispose();
        base.OnFormClosed(e);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// PANTALLA DE RESULTADOS
// ═══════════════════════════════════════════════════════════════════════════════

public class PantallaResultados : Form
{
    private readonly List<Chef> chefs;
    private readonly int        puntajeTotal;

    public PantallaResultados(List<Chef> chefs, int puntajeTotal)
    {
        this.chefs        = chefs;
        this.puntajeTotal = puntajeTotal;

        Text            = "Crazy Snack Rush TEC - Resultados";
        Size            = new Size(540, 420);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(15, 10, 30);
        DoubleBuffered  = true;

        var canvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        canvas.Paint += Pintar;

        var btnMenu = new Button
        {
            Text      = "  MENU PRINCIPAL",
            Bounds    = new Rectangle(120, 330, 300, 50),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(55, 45, 120),
            Cursor    = Cursors.Hand
        };
        btnMenu.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 220);
        btnMenu.Click += (s, e) => Close();

        var btnSalir = new Button
        {
            Text      = "x  SALIR",
            Bounds    = new Rectangle(120, 385, 300, 28),
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(140, 140, 140),
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand
        };
        btnSalir.FlatAppearance.BorderSize = 0;
        btnSalir.Click += (s, e) => Application.Exit();

        Controls.AddRange(new Control[] { canvas, btnMenu, btnSalir });
    }

    private void Pintar(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode   = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        using (var grad = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(15, 10, 30), Color.FromArgb(35, 24, 68), LinearGradientMode.Vertical))
            g.FillRectangle(grad, ClientRectangle);

        var fmt = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        g.DrawString("TIEMPO AGOTADO",
            new Font("Segoe UI", 22, FontStyle.Bold),
            new SolidBrush(Color.FromArgb(255, 210, 60)),
            new RectangleF(0, 28, 540, 50), fmt);

        g.DrawString("Resultados de la partida",
            new Font("Segoe UI", 10),
            new SolidBrush(Color.FromArgb(150, 130, 200)),
            new RectangleF(0, 76, 540, 24), fmt);

        using (var lp = new Pen(Color.FromArgb(55, 90, 75, 160), 1))
            g.DrawLine(lp, 80, 106, 460, 106);

        for (int i = 0; i < chefs.Count; i++)
        {
            float by = 120 + i * 88;

            g.FillRoundedRectangle(
                new SolidBrush(Color.FromArgb(32, chefs[i].Color.R, chefs[i].Color.G, chefs[i].Color.B)),
                new Rectangle(80, (int)by, 380, 74), 10);
            g.DrawRoundedRectangle(
                new Pen(Color.FromArgb(75, chefs[i].Color.R, chefs[i].Color.G, chefs[i].Color.B), 1.5f),
                new Rectangle(80, (int)by, 380, 74), 10);

            using (var bc = new SolidBrush(chefs[i].Color))
                g.FillEllipse(bc, 96, by + 18, 22, 22);

            g.DrawString(chefs[i].Nombre,
                new Font("Segoe UI", 13, FontStyle.Bold),
                new SolidBrush(chefs[i].Color), 126, by + 10);

            g.DrawString($"{chefs[i].Puntos} puntos entregados",
                new Font("Segoe UI", 10),
                new SolidBrush(Color.FromArgb(210, 210, 210)), 126, by + 36);
        }

        float yTotal = 120 + chefs.Count * 88 + 10;
        g.DrawString($"Puntaje total del equipo:  {puntajeTotal} pts",
            new Font("Segoe UI", 11, FontStyle.Bold),
            new SolidBrush(Color.FromArgb(170, 255, 170)),
            new RectangleF(0, yTotal, 540, 28), fmt);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// EXTENSIONES DE GRAPHICS
// ═══════════════════════════════════════════════════════════════════════════════

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush b, Rectangle r, int rad)
    {
        using var path = MakePath(r, rad);
        g.FillPath(b, path);
    }

    public static void DrawRoundedRectangle(this Graphics g, Pen p, Rectangle r, int rad)
    {
        using var path = MakePath(r, rad);
        g.DrawPath(p, path);
    }

    private static GraphicsPath MakePath(Rectangle r, int rad)
    {
        var path = new GraphicsPath();
        path.AddArc(r.X,             r.Y,              rad * 2, rad * 2, 180, 90);
        path.AddArc(r.Right - rad*2, r.Y,              rad * 2, rad * 2, 270, 90);
        path.AddArc(r.Right - rad*2, r.Bottom - rad*2, rad * 2, rad * 2,   0, 90);
        path.AddArc(r.X,             r.Bottom - rad*2, rad * 2, rad * 2,  90, 90);
        path.CloseFigure();
        return path;
    }
}
