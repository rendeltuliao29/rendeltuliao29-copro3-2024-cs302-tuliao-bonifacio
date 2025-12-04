using CJR_Racing;

class BrakingSystem : CarModule
{
    public struct BrakeData
    {
        public string BrakeType { get; set; }
        public string BrakeLevel { get; set; }
        public string ABSEnabled { get; set; }
    }

    private BrakeData Data;

    public BrakingSystem(byte brakeType = 0, byte brakeLevel = 0, byte absEnabled = 0)
    {
        Data = new BrakeData
        {
            BrakeType = "",
            BrakeLevel = "",
            ABSEnabled = ""
        };

        if (brakeType == 0 || brakeLevel == 0 || absEnabled == 0)
            Configure();
    }

    // Constructor used when loading from DB
    public BrakingSystem(string brakeType, string brakeLevel, string absEnabled)
    {
        Data = new BrakeData
        {
            BrakeType = brakeType,
            BrakeLevel = brakeLevel,
            ABSEnabled = absEnabled
        };
    }

    public override void Configure()
    {
        this.Data = new BrakeData();

        while (true)
        {
            Console.WriteLine("--- BRAKING SYSTEM ---");
            string[] brakeTypeChoices = { "Standard", "Carbon-Ceramic", "Carbon-Carbon", "Performance-Ventilated" };
            Console.WriteLine("Brake Type:");
            for (int i = 0; i < brakeTypeChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {brakeTypeChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte bt) && bt >= 1 && bt <= 4)
            {
                this.Data.BrakeType = brakeTypeChoices[bt - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] levelChoices = { "Low", "Medium", "High", "Track-Max" };
            Console.WriteLine("Brake Sensitivity:");
            for (int i = 0; i < levelChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {levelChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte bl) && bl >= 1 && bl <= 4)
            {
                this.Data.BrakeLevel = levelChoices[bl - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] yesNo = { "Yes", "No" };
            Console.WriteLine("Enable ABS (Anti-lock Braking System)?:");
            for (int i = 0; i < yesNo.Length; i++)
                Console.WriteLine($"[{i + 1}] {yesNo[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte ab) && ab >= 1 && ab <= 2)
            {
                this.Data.ABSEnabled = yesNo[ab - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1 or 2.");
        }
    }

    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== BRAKING SYSTEM ===");
        Console.ResetColor();

        Console.WriteLine("Brake Type  : " + this.Data.BrakeType);
        Console.WriteLine("Sensitivity : " + this.Data.BrakeLevel);
        Console.WriteLine("ABS Enabled : " + this.Data.ABSEnabled);
    }

    public override object GetData() => Data;
}
