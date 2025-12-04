using CJR_Racing;

class EnginePowertrain : CarModule
{
    public struct EngineData
    {
        public string EngineType { get; set; }
        public string EnginePower { get; set; }
        public string Transmission { get; set; }
        public string ERSMode { get; set; }
        public string ERSBoost { get; set; }
    }

    private EngineData Data;
    public EnginePowertrain(byte engineType = 0, byte enginePower = 0, byte transmission = 0, byte ersMode = 0, byte ersBoost = 0)
    {
        Data = new EngineData
        {
            EngineType = "",
            EnginePower = "",
            Transmission = "",
            ERSMode = "",
            ERSBoost = ""
        };

        if (engineType == 0 || enginePower == 0 || transmission == 0 || ersMode == 0 || ersBoost == 0)
            Configure();
    }

    // Constructor used when loading from DB
    public EnginePowertrain(string engineType, string enginePower, string transmission, string ersMode, string ersBoost)
    {
        Data = new EngineData
        {
            EngineType = engineType,
            EnginePower = enginePower,
            Transmission = transmission,
            ERSMode = ersMode,
            ERSBoost = ersBoost
        };
    }

    public override void Configure()
    {
        this.Data = new EngineData();

        while (true)
        {
            Console.WriteLine("--- ENGINE & POWERTRAIN ---");
            string[] engineTypes = { "Inline-4", "V6", "V8", "V10" };
            Console.WriteLine("Engine Type:");
            for (int i = 0; i < engineTypes.Length; i++)
                Console.WriteLine($"[{i + 1}] {engineTypes[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte et) && et >= 1 && et <= 4)
            {
                this.Data.EngineType = engineTypes[et - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] powerChoices = { "Standard", "Sport", "Performance", "Extreme" };
            Console.WriteLine("Engine Tune:");
            for (int i = 0; i < powerChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {powerChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte ep) && ep >= 1 && ep <= 4)
            {
                this.Data.EnginePower = powerChoices[ep - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] transmission = { "Manual", "Automatic", "Sequential", "Paddle-Shift" };
            Console.WriteLine("Transmission:");
            for (int i = 0; i < transmission.Length; i++)
                Console.WriteLine($"[{i + 1}] {transmission[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte tr) && tr >= 1 && tr <= 4)
            {
                this.Data.Transmission = transmission[tr - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] ersModes = { "Charge", "Balanced", "Attack", "Overtake" };
            Console.WriteLine("ERS Mode (Energy Recovery System):");
            for (int i = 0; i < ersModes.Length; i++)
                Console.WriteLine($"[{i + 1}] {ersModes[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte em) && em >= 1 && em <= 4)
            {
                this.Data.ERSMode = ersModes[em - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] yesNo = { "Yes (gives a temporary speed or acceleration bonus.)", "No (your car is slightly slower but will GREATLY help with control.)" };
            Console.WriteLine("Activate ERS Boost? (Energy Recovery System):");
            for (int i = 0; i < yesNo.Length; i++)
                Console.WriteLine($"[{i + 1}] {yesNo[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte eb) && eb >= 1 && eb <= 2)
            {
                this.Data.ERSBoost = eb == 1 ? "Yes" : "No";
                break;
            }
            Console.WriteLine("Invalid input! Choose 1 or 2.");
        }
    }
        
    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== ENGINE & POWERTRAIN ===");
        Console.ResetColor();

        Console.WriteLine("Engine Type  : " + this.Data.EngineType);
        Console.WriteLine("Engine Power : " + this.Data.EnginePower);
        Console.WriteLine("Transmission : " + this.Data.Transmission);
        Console.WriteLine("ERS Mode     : " + this.Data.ERSMode);
        Console.WriteLine("ERS Boost    : " + this.Data.ERSBoost);
    }

    public override object GetData() => Data;
}
