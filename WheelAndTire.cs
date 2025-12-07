using CJR_Racing;

class WheelAndTire : CarModule
{
    public struct WheelData
    {
        public string TireCompound { get; set; }
        public byte TirePressure { get; set; }
    }

    private WheelData Data;
    public WheelAndTire(byte tireCompound = 0, byte tirePressure = 0)
    {
        Data = new WheelData
        {
            TireCompound = string.Empty,
            TirePressure = tirePressure
        };

        if (tireCompound == 0 || tirePressure == 0)
            Configure();
    }

    // Constructor used when loading from DB
    public WheelAndTire(string tireCompound, byte tirePressure)
    {
        Data = new WheelData
        {
            TireCompound = tireCompound,
            TirePressure = tirePressure
        };
    }

    public void Configure(byte tireCompound, byte tirePressure)
    {
        this.Data = new WheelData();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════╗");
            Console.WriteLine("║   Wheels and Tires   ║");
            Console.WriteLine("╚══════════════════════╝");
            string[] compounds = { "Soft", "Medium", "Hard" };
            Console.WriteLine("Tire Compound:");
            for (int i = 0; i < compounds.Length; i++)
                Console.WriteLine($"[{i + 1}] {compounds[i]}");
            if (byte.TryParse(Console.ReadLine(), out tireCompound) && tireCompound >= 1 && tireCompound <= 3)
            {
                this.Data.TireCompound = compounds[tireCompound - 1];
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid choice! Must be 1, 2, or 3.");
        }
        

        while (true)
        {
            Console.Write("Tire Pressure (10–25 PSI, lower = more grip, higher = more speed): ");
            if (byte.TryParse(Console.ReadLine(), out tirePressure) && tirePressure >= 10 && tirePressure <= 25)
                break;
            Console.Clear();
            Console.WriteLine("Invalid PSI! Must be between 10–25.");
        }
        this.Data.TirePressure = tirePressure;

        Console.Clear();
    }

    public override void Configure() => Configure(0, 0);

    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════╗");
        Console.WriteLine("║   Wheels and Tires   ║");
        Console.WriteLine("╚══════════════════════╝");
        Console.ResetColor();

        Console.WriteLine("Tire Compound : " + this.Data.TireCompound);
        Console.WriteLine("Tire Pressure : " + this.Data.TirePressure + " PSI");
    }

    public override object GetData() => Data;
}
