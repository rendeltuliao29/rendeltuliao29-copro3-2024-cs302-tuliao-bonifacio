using CJR_Racing;

class Aerodynamics : CarModule
{
    public struct AeroData
    {
        public string FrontWing { get; set; }
        public string RearWing { get; set; }
        public string DRSEnabled { get; set; }
        public string DownforceLevel { get; set; }
        public string WingAngle { get; set; }
    }

    private AeroData Data;
    public Aerodynamics(byte frontWing = 0, byte rearWing = 0, byte drsEnabled = 0, byte downforceLevel = 0, byte wingAngle = 0)
    {
        Data = new AeroData
        {
            FrontWing = "",
            RearWing = "",
            DRSEnabled = "",
            DownforceLevel = "",
            WingAngle = ""
        };

        if (frontWing == 0 || rearWing == 0 || drsEnabled == 0 || downforceLevel == 0 || wingAngle == 0)
            Configure();
    }

    // Constructor used when loading from DB
    public Aerodynamics(string frontWing, string rearWing, string drsEnabled, string downforce, string wingAngle)
    {
        Data = new AeroData
        {
            FrontWing = frontWing,
            RearWing = rearWing,
            DRSEnabled = drsEnabled,
            DownforceLevel = downforce,
            WingAngle = wingAngle
        };
    }

    public override void Configure()
    {
        this.Data = new AeroData();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════╗");
            Console.WriteLine("║   Aerodynamics   ║");
            Console.WriteLine("╚══════════════════╝");
            string[] frontChoices = { "Low", "High", "Balanced", "Heavy-load" };
            Console.WriteLine("Front Wing Type:");
            for (int i = 0; i < frontChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {frontChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte fw) && fw >= 1 && fw <= 4)
            {
                this.Data.FrontWing = frontChoices[fw - 1];
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] rearChoices = { "Low", "High", "DRS", "Balanced" };
            Console.WriteLine("Rear Wing Type:");
            for (int i = 0; i < rearChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {rearChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte rw) && rw >= 1 && rw <= 4)
            {
                this.Data.RearWing = rearChoices[rw - 1];
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] yesNo = { "Yes (Increases speed in straight line paths)", "No (Helps in having a smooth turn in curve paths)" };
            Console.WriteLine("Enable DRS? (Drag Reduction System):");
            for (int i = 0; i < yesNo.Length; i++)
                Console.WriteLine($"[{i + 1}] {yesNo[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte drs) && (drs == 1 || drs == 2))
            {
                this.Data.DRSEnabled = drs == 1 ? "Yes" : "No";
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid input! Choose 1 or 2.");
        }

        while (true)
        {
            string[] dfChoices = { "Low", "Medium", "High", "Extreme" };
            Console.WriteLine("Downforce Level:");
            for (int i = 0; i < dfChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {dfChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte df) && df >= 1 && df <= 4)
            {
                this.Data.DownforceLevel = dfChoices[df - 1];
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] waChoices = { "0–5°", "6–10°", "11–15°", "16–20°" };
            Console.WriteLine("Wing Angle Setup:");
            for (int i = 0; i < waChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {waChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte wa) && wa >= 1 && wa <= 4)
            {
                this.Data.WingAngle = waChoices[wa - 1];
                break;
            }
            Console.Clear();
            Console.WriteLine("Invalid input! Choose 1–4.");
           
        }
        Console.Clear();
    }

    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════╗");
        Console.WriteLine("║   Aerodynamics   ║");
        Console.WriteLine("╚══════════════════╝");
        Console.ResetColor();

        Console.WriteLine("Front Wing : " + this.Data.FrontWing);
        Console.WriteLine("Rear Wing  : " + this.Data.RearWing);
        Console.WriteLine("DRS Enabled: " + this.Data.DRSEnabled);
        Console.WriteLine("Downforce  : " + this.Data.DownforceLevel);
        Console.WriteLine("Wing Angle : " + this.Data.WingAngle);
    }

    public override object GetData() => Data;
}
