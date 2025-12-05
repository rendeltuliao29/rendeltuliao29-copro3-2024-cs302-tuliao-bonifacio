using CJR_Racing;

class SuspensionHandling : CarModule
{
    public struct SuspensionData
    {
        public string SuspensionType { get; set; }
        public string SuspensionLevel { get; set; }
        public string SteeringLevel { get; set; }
        public string RideHeight { get; set; }
        public string CamberToeAngles { get; set; }
    }

    private SuspensionData Data;
    public SuspensionHandling(string suspensionType = "", string suspensionLevel = "", string steeringLevel = "", string rideHeight = "", string camberToeAngles = "")
    {
        Data = new SuspensionData
        {
            SuspensionType = suspensionType,
            SuspensionLevel = suspensionLevel,
            SteeringLevel = steeringLevel,
            RideHeight = rideHeight,
            CamberToeAngles = camberToeAngles
        };

        if (string.IsNullOrEmpty(suspensionType) || string.IsNullOrEmpty(suspensionLevel) || string.IsNullOrEmpty(steeringLevel) || string.IsNullOrEmpty(rideHeight) || string.IsNullOrEmpty(camberToeAngles))
            Configure();
    }



    public override void Configure()
    {
        this.Data = new SuspensionData();

        while (true)
        {
            Console.WriteLine("--- SUSPENSION & HANDLING ---");
            string[] suspensionTypeChoices = { "Push-rod", "Pull-rod", "Active", "Soft-ride" };
            Console.WriteLine("Suspension Type:");
            for (int i = 0; i < suspensionTypeChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {suspensionTypeChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte st) && st >= 1 && st <= 4)
            {
                this.Data.SuspensionType = suspensionTypeChoices[st - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] suspLevelChoices = { "Soft", "Medium", "Hard", "Track-Extreme" };
            Console.WriteLine("Suspension Stiffness:");
            for (int i = 0; i < suspLevelChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {suspLevelChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte sl) && sl >= 1 && sl <= 4)
            {
                this.Data.SuspensionLevel = suspLevelChoices[sl - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] steeringChoices = { "Low", "Medium", "High", "Extreme" };
            Console.WriteLine("Steering Sensitivity:");
            for (int i = 0; i < steeringChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {steeringChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte se) && se >= 1 && se <= 4)
            {
                this.Data.SteeringLevel = steeringChoices[se - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] rideChoices = { "Low", "Medium", "High", "Ultra-Low" };
            Console.WriteLine("Ride Height:");
            for (int i = 0; i < rideChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {rideChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte rh) && rh >= 1 && rh <= 4)
            {
                this.Data.RideHeight = rideChoices[rh - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
        }

        while (true)
        {
            string[] alignChoices = { "Neutral", "Negative Camber", "Positive Toe", "Aggressive Track" };
            Console.WriteLine("Alignment Setup:");
            for (int i = 0; i < alignChoices.Length; i++)
                Console.WriteLine($"[{i + 1}] {alignChoices[i]}");
            if (byte.TryParse(Console.ReadLine(), out byte ca) && ca >= 1 && ca <= 4)
            {
                this.Data.CamberToeAngles = alignChoices[ca - 1];
                break;
            }
            Console.WriteLine("Invalid input! Choose 1–4.");
            
        }
        Console.Clear();
    }

    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== SUSPENSION & HANDLING ===");
        Console.ResetColor();

        Console.WriteLine("Suspension Type  : " + this.Data.SuspensionType);
        Console.WriteLine("Suspension Level : " + this.Data.SuspensionLevel);
        Console.WriteLine("Steering Level   : " + this.Data.SteeringLevel);
        Console.WriteLine("Ride Height      : " + this.Data.RideHeight);
        Console.WriteLine("Alignment Setup  : " + this.Data.CamberToeAngles);
    }

    public override object GetData() => Data;
}
