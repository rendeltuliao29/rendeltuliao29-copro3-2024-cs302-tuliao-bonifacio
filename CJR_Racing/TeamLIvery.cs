using CJR_Racing;
using Microsoft.Data.Sqlite;

class TeamLivery : CarModule
{
    public struct DriverData
    {
        public string Name { get; set; }
        public byte Age { get; set; }
        public string Experience { get; set; }
    }

    private DriverData Data;
    public TeamLivery(string name = "", byte age = 0, byte experience = 0)
    {
        Data = new DriverData
        {
            Name = name,
            Age = age,
            Experience = ""
        };

        if (string.IsNullOrWhiteSpace(name) || age == 0 || experience == 0)
            Configure();
    }

    // Constructor used when loading from DB
    public TeamLivery(string name, byte age, string experience)
    {
        Data = new DriverData
        {
            Name = name,
            Age = age,
            Experience = experience
        };
    }

    public override void Configure()
    {
        string name;
        byte age = 0, experience = 0;

        while (true)
        {
            Console.Write("Driver Name: ");
            name = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty!");
                continue;
            }

            if (name.Length < 3)
            {
                Console.WriteLine("Name must be at least 3 characters long!");
                continue;
            }
            if (!char.IsLetter(name[0]))
            {
                Console.WriteLine("First character must be a letter (A–Z)!");
                continue;
            }

            break;
        }

        Data.Name = name;

        while (true)
        {
            Console.Write("Driver Age: ");
            if (byte.TryParse(Console.ReadLine(), out age) && age >= 16 && age <= 60)
                break;
            Console.WriteLine("Invalid age! Must be 16–60.");
        }
        Data.Age = age;

        while (true)
        {
            Console.WriteLine("Experience Level:\n[1] Rookie\n[2] Intermediate\n[3] Pro");
            if (byte.TryParse(Console.ReadLine(), out experience) && experience >= 1 && experience <= 3)
            {
                string[] choices = { "Rookie", "Intermediate", "Pro" };
                Data.Experience = choices[experience - 1];
                break;
            }
            Console.WriteLine("Invalid choice! Must be 1, 2, or 3.");
            
        }

        Console.Clear();

    }

    public override void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== DRIVER INFO ===");
        Console.ResetColor();
        Console.WriteLine($"Name       : {Data.Name}");
        Console.WriteLine($"Age        : {Data.Age}");
        Console.WriteLine($"Experience : {Data.Experience}");
        

    }

    public override object GetData() => Data;
}
