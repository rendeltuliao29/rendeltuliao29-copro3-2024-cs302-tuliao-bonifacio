using CJR_Racing;
using System;
using System.Threading;

namespace CJR_Racing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DatabaseHelper.EnsureDatabase();

            Thread.Sleep(2000);

            while (true)
            {
                ShowMainMenu();
            }
        }

        public static void ShowMainMenu()
        {
                string[] options =
                {
                "New Game",
                "Load Game",
                "Campaign",
                "Credits",
                "Exit"
                };

            int selectedIndex = 0;
            ConsoleKey key;

                do
                {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("===== CJR RACING =====\n");
                        Console.ResetColor();

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex < 0)
                    selectedIndex = options.Length - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length)
                    selectedIndex = 0;
                }

            } while (key != ConsoleKey.Enter);

            HandleMenuSelection(selectedIndex);
        }

        public static void HandleMenuSelection(int index)
        {
            switch (index)
            {
            case 0:
                StartNewGame();
                break;

            case 1:
               var loaded = DatabaseHelper.LoadModules();
                if (loaded == null || loaded.Length == 0)
                {
                    Console.WriteLine("No saved game found. Press any key to return.");
                    Console.ReadKey();
                    return;
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("===== LOADED CONFIGURATION =====\n");
                Console.ResetColor();

                foreach (var module in loaded)
                {
                    module.Show();
                    Console.WriteLine();
                }

                Console.WriteLine("\nPress any key to return to the main menu...");
                Console.ReadKey();
                break;

            case 2:
                CampaignMode();
                break;

            case 3:
                ShowCredits();
                break;

            case 4:
                Console.WriteLine("Exiting... See you on the track!");
                Environment.Exit(0);
                break;
            }
        }



        static void StartNewGame()
        {
            Console.Clear();
            Console.WriteLine("--- NEW GAME: F1 CAR SETUP ---\n");
            CarModule[] modules = new CarModule[]
            {
                new TeamLivery(),
                new Aerodynamics(),
                new EnginePowertrain(),
                new WheelAndTire(),
                new SuspensionHandling(),
                new BrakingSystem()
            };

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("========== F1 CAR CONFIGURATION SUMMARY ==========\n");
            Console.ResetColor();

            foreach (var module in modules)
            {
                // module.Show() prints the user-facing summary for each module.
                module.Show();
                Console.WriteLine();

                
            }

            // save data to database
            DatabaseHelper.SaveModules(modules);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("==================================================");
            Console.ResetColor();

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public static void CampaignMode()
        {
            Console.Clear();
            string paragraph1 =
                "Nowadays, racing isn’t just about speed... it’s about life itself. " +
                "You grew up watching others pass you by, always staring at the backs of those ahead of you. " +
                "But this time, everything changes—you’re finally getting your chance. " +
                "A chance to blow smoke in their faces as they watch you speed past. " +
                "You get to choose a team, a team that will carve a path for you—one that leads to the horizon at the world’s end. " +
                "This might not be the start of your life’s race, but it is the beginning of a new one—your race toward success.";

            string paragraph2 =
                "While you’re feeling excited and optimistic about this new journey, others are already training, pushing themselves until the wind nearly tears their helmets off. " +
                "You realize you’re at the back of the pack. This may be a new chapter, but it doesn’t mean you’re already catching up. " +
                "The pressure hits you—heavy, sharp, almost like riding a spacecraft at light speed. " +
                "But as a driver, panic is your enemy. You need to stay calm. Every corner, every braking point, every burst of acceleration decides whether you’ll win or fall behind. " +
                "You carry your team’s hopes, and you cannot let them down. When the time comes, you must show them what you’re made of—slam that gas pedal like there’s no tomorrow.";

            string paragraph3 =
                "And now, as the engine rumbles beneath you and the world holds its breath, the lights on the starting grid begin to glow red one by one. " +
                "Your heart syncs with each beep, your focus narrowing into a single line stretching endlessly before you. " +
                "This is no longer just a race—it is your proving ground, your battleground, your story waiting to be written. " +
                "Every rival beside you carries their own dreams, but today, you refuse to let anyone outrun yours. " +
                "The lights go out, the roar erupts, and in that explosive moment, your true journey begins.";

            TypeText(paragraph1);
            Console.WriteLine("\n\n");
            TypeText(paragraph2);
            Console.WriteLine("\n\n");
            TypeText(paragraph3);

            Console.WriteLine("\n\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public static void TypeText(string text)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(15);
            }
        }

        public static void ShowCredits()
        {
            Console.Clear();
            string credits =
                "CJR Racing was born thanks to the utak-ng-utak na teamwork nina Rendel V. Tuliao " +
                "at Chriz John Bonifacio, ang aming fearless main programmers. " +
                "Shoutout din sa mga friends na nag-test, nag-cheer, at minsan, sigaw lang sa computer sa frustration. " +
                "Super special thanks kay Cyril Alvarez, sa pagtitiis na okupahin namin ang bahay niya, " +
                "kainin ang snacks niya, at gawing coding HQ ang living room niya. " +
                "Kung wala siya (at ang Wi-Fi niya), baka hanggang sketch lang ang project na 'to!\n";

            TypeText(credits);
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }
    }
}