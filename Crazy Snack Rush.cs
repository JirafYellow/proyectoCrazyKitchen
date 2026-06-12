// CLASE BASE (ABSTRACTA)

public abstract class Ingrediente
{
    public string Nombre { get; set; }
    public string Estado { get; set; }
    
    public Ingrediente(string nombre)
    {
        Nombre = nombre;
        Estado = "Crudo";
    }
}

// HERENCIA (CLASES HIJAS)
// Vegetales y frutas
public class VegetalesYFrutas : Ingrediente
{
    public VegetalesYFrutas(string nombre) : base(nombre)
    {
    }
}

// Panes y bases
public class PanesYBases : Ingrediente
{
    public PanesYBases(string nombre) : base(nombre)
    {
    }
}

// Proteina
public class Proteina : Ingrediente
{
    public bool Cocinada { get; set; }
    
    public Proteina(string nombre) : base(nombre)
    {
        Cocinada = false;
    }
}

class Program
{
    static void Main(string[] args)
    {
        VegetalesYFrutas tomate = new VegetalesYFrutas("Tomate");
        PanesYBases pan = new PanesYBases("Pan de Hamburguesa");
        Proteina carne = new Proteina("Carne de Res");

        Console.WriteLine($"[Creado] {tomate.Nombre} -> Estado: {tomate.Estado}");
        Console.WriteLine($"[Creado] {pan.Nombre} -> Estado: {pan.Estado}");
        Console.WriteLine($"[Creado] {carne.Nombre} -> Estado: {carne.Estado}, ¿Cocinada?: {carne.Cocinada}");

        Console.WriteLine("\nPresione cualquier tecla para salir...");
        Console.ReadKey();
    }
}