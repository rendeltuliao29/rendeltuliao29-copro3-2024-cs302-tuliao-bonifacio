using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace CJR_Racing
{
    public static class DatabaseHelper
    {
        private static readonly string DbFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "db.db");

        public static void path()
        {
            Console.WriteLine("DB Path: " + DbFile);
        }

        public static void EnsureDatabase()
        {
            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Drivers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    Name TEXT NOT NULL,
                    Age INTEGER,
                    Experience TEXT
                );

                CREATE TABLE IF NOT EXISTS Aerodynamics (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    FrontWing TEXT,
                    RearWing TEXT,
                    DRSEnabled TEXT,
                    DownforceLevel TEXT,
                    WingAngle TEXT
                );

                CREATE TABLE IF NOT EXISTS Engine (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    EngineType TEXT,
                    EnginePower TEXT,
                    Transmission TEXT,
                    ERSMode TEXT,
                    ERSBoost TEXT
                );

                CREATE TABLE IF NOT EXISTS Wheels (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    TireCompound TEXT,
                    TirePressure INTEGER
                );

                CREATE TABLE IF NOT EXISTS Suspension (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    SuspensionType TEXT,
                    SuspensionLevel TEXT,
                    SteeringLevel TEXT,
                    RideHeight TEXT,
                    CamberToeAngles TEXT
                );

                CREATE TABLE IF NOT EXISTS Brakes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    BrakeType TEXT,
                    BrakeLevel TEXT,
                    ABSEnabled TEXT
                );

                CREATE TABLE IF NOT EXISTS Sessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CreatedAt TEXT,
                    Name TEXT
                );
            ";
            cmd.ExecuteNonQuery();

            // Make sure older DB files get the SessionId column if they were created with an older schema
            EnsureColumnExists(conn, "Drivers", "SessionId", "INTEGER");
            EnsureColumnExists(conn, "Aerodynamics", "SessionId", "INTEGER");
            EnsureColumnExists(conn, "Engine", "SessionId", "INTEGER");
            EnsureColumnExists(conn, "Wheels", "SessionId", "INTEGER");
            EnsureColumnExists(conn, "Suspension", "SessionId", "INTEGER");
            EnsureColumnExists(conn, "Brakes", "SessionId", "INTEGER");
        }

        // Adds a column if it does not exist in the specified table
        private static void EnsureColumnExists(SqliteConnection conn, string table, string column, string definition)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = $"PRAGMA table_info(\"{table}\");";
            using var reader = checkCmd.ExecuteReader();
            bool found = false;
            while (reader.Read())
            {
                //table_info returns: cid, name, type, notnull, dflt_value, pk
                var colName = reader.GetString(1);
                if (string.Equals(colName, column, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                using var alterCmd = conn.CreateCommand();
                // ALTER TABLE ADD COLUMN is safe for adding nullable columns in SQLite
                alterCmd.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition};";
                alterCmd.ExecuteNonQuery();
            }
        }

        public static void SaveModules(CarModule[] modules)
        {
            EnsureDatabase();

            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();

            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            // create session row and get id
            Console.Write("Enter a name for this save (optional): ");
            var sessionName = Console.ReadLine() ?? string.Empty;

            cmd.CommandText = "INSERT INTO Sessions (CreatedAt, Name) VALUES ($now, $name);";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("$name", sessionName);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT last_insert_rowid();";

            // Use safe conversion in case returns null.
            var scalar = cmd.ExecuteScalar();
            var sessionId = Convert.ToInt64(scalar ?? 0L);

            foreach (var module in modules)
            {
                var data = module.GetData();
                if (data is TeamLivery.DriverData d)
                {
                    cmd.CommandText = "INSERT INTO Drivers (SessionId, Name, Age, Experience) VALUES ($sid, $name, $age, $exp);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$name", d.Name);
                    cmd.Parameters.AddWithValue("$age", d.Age);
                    cmd.Parameters.AddWithValue("$exp", d.Experience);
                    cmd.ExecuteNonQuery();
                }
                else if (data is Aerodynamics.AeroData a)
                {
                    cmd.CommandText = "INSERT INTO Aerodynamics (SessionId, FrontWing, RearWing, DRSEnabled, DownforceLevel, WingAngle) VALUES ($sid,$fw,$rw,$drs,$df,$wa);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$fw", a.FrontWing);
                    cmd.Parameters.AddWithValue("$rw", a.RearWing);
                    cmd.Parameters.AddWithValue("$drs", a.DRSEnabled);
                    cmd.Parameters.AddWithValue("$df", a.DownforceLevel);
                    cmd.Parameters.AddWithValue("$wa", a.WingAngle);
                    cmd.ExecuteNonQuery();
                }
                else if (data is EnginePowertrain.EngineData e)
                {
                    cmd.CommandText = "INSERT INTO Engine (SessionId, EngineType, EnginePower, Transmission, ERSMode, ERSBoost) VALUES ($sid,$et,$ep,$tr,$em,$eb);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$et", e.EngineType);
                    cmd.Parameters.AddWithValue("$ep", e.EnginePower);
                    cmd.Parameters.AddWithValue("$tr", e.Transmission);
                    cmd.Parameters.AddWithValue("$em", e.ERSMode);
                    cmd.Parameters.AddWithValue("$eb", e.ERSBoost);
                    cmd.ExecuteNonQuery();
                }
                else if (data is WheelAndTire.WheelData w)
                {
                    cmd.CommandText = "INSERT INTO Wheels (SessionId, TireCompound, TirePressure) VALUES ($sid,$tc,$tp);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$tc", w.TireCompound);
                    cmd.Parameters.AddWithValue("$tp", w.TirePressure);
                    cmd.ExecuteNonQuery();
                }
                else if (data is SuspensionHandling.SuspensionData s)
                {
                    cmd.CommandText = "INSERT INTO Suspension (SessionId, SuspensionType, SuspensionLevel, SteeringLevel, RideHeight, CamberToeAngles) VALUES ($sid,$st,$sl,$se,$rh,$ca);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$st", s.SuspensionType);
                    cmd.Parameters.AddWithValue("$sl", s.SuspensionLevel);
                    cmd.Parameters.AddWithValue("$se", s.SteeringLevel);
                    cmd.Parameters.AddWithValue("$rh", s.RideHeight);
                    cmd.Parameters.AddWithValue("$ca", s.CamberToeAngles);
                    cmd.ExecuteNonQuery();
                }
                else if (data is BrakingSystem.BrakeData b)
                {
                    cmd.CommandText = "INSERT INTO Brakes (SessionId, BrakeType, BrakeLevel, ABSEnabled) VALUES ($sid,$bt,$bl,$ab);";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    cmd.Parameters.AddWithValue("$bt", b.BrakeType);
                    cmd.Parameters.AddWithValue("$bl", b.BrakeLevel);
                    cmd.Parameters.AddWithValue("$ab", b.ABSEnabled);
                    cmd.ExecuteNonQuery();
                }
            }

            tx.Commit();
        }

        // Load the latest saved module configuration
        public static CarModule[] LoadModules()
        {
            EnsureDatabase();

            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            
            
            // List available sessions
            cmd.CommandText = "SELECT Id, CreatedAt, Name FROM Sessions ORDER BY Id DESC;";
            var sessions = new System.Collections.Generic.List<(long Id, string CreatedAt, string Name)>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    sessions.Add((reader.GetInt64(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1), reader.IsDBNull(2) ? string.Empty : reader.GetString(2)));
            }

            if (sessions.Count == 0)
                return Array.Empty<CarModule>();

            // Ask user which name to load
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select a saved session to load (enter number), or:");
                Console.WriteLine("[A] Show all characters (Drivers)");
                Console.WriteLine("[D] Delete a character (Driver) by Id");
                Console.WriteLine();

                for (int i = 0; i < sessions.Count; i++)
                {
                    var s = sessions[i];
                    // Prefer saved Name; fall back to CreatedAt; finally fall back to a generic session label.
                    var label = !string.IsNullOrWhiteSpace(s.Name)
                        ? s.Name
                        : (!string.IsNullOrWhiteSpace(s.CreatedAt) ? s.CreatedAt : $"Session {s.Id}");

                    // Display the label to the user; include internal session id for clarity if needed.
                    Console.WriteLine($"[{i + 1}] {label} (Id: {s.Id})");
                }

                Console.WriteLine();
                Console.Write("Enter choice number or letter: ");
                var input = Console.ReadLine() ?? string.Empty;
                input = input.Trim();

                if (string.Equals(input, "A", StringComparison.OrdinalIgnoreCase))
                {
                    // Show all characters (Drivers)
                    cmd.CommandText = "SELECT Id, SessionId, Name, Age, Experience FROM Drivers ORDER BY Id;";
                    cmd.Parameters.Clear();
                    using var rdr = cmd.ExecuteReader();

                    var rows = new System.Collections.Generic.List<(long Id, long SessionId, string Name, int Age, string Experience)>();
                    while (rdr.Read())
                    {
                        var id = rdr.IsDBNull(0) ? 0L : rdr.GetInt64(0);
                        var sid = rdr.IsDBNull(1) ? 0L : rdr.GetInt64(1);
                        var name = rdr.IsDBNull(2) ? string.Empty : rdr.GetString(2);
                        var age = rdr.IsDBNull(3) ? 0 : rdr.GetInt32(3);
                        var exp = rdr.IsDBNull(4) ? string.Empty : rdr.GetString(4);

                        // keep raw values
                        rows.Add((id, sid, name, age, exp));
                    }

                    Console.WriteLine();
                    Console.WriteLine("All characters (Drivers):");

                    if (rows.Count == 0)
                    {
                        Console.WriteLine("  (no drivers found)");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to return to sessions menu...");
                        Console.ReadKey();
                        continue; // redisplay menu
                    }

                    // Determine column widths (cap name/experience to avoid extremely wide columns)
                    const int maxNameWidth = 30;
                    const int maxExpWidth = 40;

                    string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max - 3) + "...";


                    // Header
                    Console.WriteLine(
                        $"{ "Id".PadLeft(3) }  " +
                        $"{ "SessionId".PadLeft(9) }  " +
                        $"{ "Name".PadRight(maxNameWidth) }  " +
                        $"{ "Age".PadLeft(3) }  " +
                        $"{ "Experience".PadRight(maxExpWidth) }");

                    // Separator
                    Console.WriteLine(
                        $"{ new string('-', 3) }  " +
                        $"{ new string('-', 9) }  " +
                        $"{ new string('-', maxNameWidth) }  " +
                        $"{ new string('-', 3) }  " +
                        $"{ new string('-', maxExpWidth) }");

                    // Rows
                    foreach (var r in rows)
                    {
                        var nameDisplay = Truncate(r.Name ?? string.Empty, maxNameWidth);
                        var expDisplay = Truncate(r.Experience ?? string.Empty, maxExpWidth);

                        Console.WriteLine(
                            $"{ r.Id.ToString().PadLeft(3) }  " +
                            $"{ r.SessionId.ToString().PadLeft(9) }  " +
                            $"{ nameDisplay.PadRight(maxNameWidth) }  " +
                            $"{ r.Age.ToString().PadLeft(3) }  " +
                            $"{ expDisplay.PadRight(maxExpWidth) }");
                    }

                    Console.WriteLine();
                    Console.WriteLine("Press any key to return to sessions menu...");
                    Console.ReadKey();
                    continue; // redisplay menu
                }

                if (string.Equals(input, "D", StringComparison.OrdinalIgnoreCase))
                {
                    // Delete a driver by Id with confirmation and session info
                    Console.Write("Enter Driver Id to delete: ");
                    var idInput = Console.ReadLine() ?? string.Empty;
                    if (!long.TryParse(idInput.Trim(), out var deleteId))       
                    {
                        Console.WriteLine("Invalid id. Press any key to return.");
                        Console.ReadKey();
                        continue;
                    }

                    // Fetch driver info
                    cmd.CommandText = "SELECT Id, SessionId, Name FROM Drivers WHERE Id = $id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$id", deleteId);
                    using var drvReader = cmd.ExecuteReader();
                    if (!drvReader.Read())
                    {
                        Console.WriteLine($"No driver found with Id {deleteId}. Press any key to return.");
                        Console.ReadKey();
                        continue;
                    }

                    var drvId = drvReader.IsDBNull(0) ? 0L : drvReader.GetInt64(0);
                    var drvSessionId = drvReader.IsDBNull(1) ? 0L : drvReader.GetInt64(1);
                    var drvName = drvReader.IsDBNull(2) ? string.Empty : drvReader.GetString(2);
                    drvReader.Close();

                    // Fetch session info for clarity
                    string sessionLabel = $"Session {drvSessionId}";
                    cmd.CommandText = "SELECT Name, CreatedAt FROM Sessions WHERE Id = $sid;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", drvSessionId);
                    using var sessReader = cmd.ExecuteReader();
                    if (sessReader.Read())
                    {
                        var sName = sessReader.IsDBNull(0) ? string.Empty : sessReader.GetString(0);
                        var sCreated = sessReader.IsDBNull(1) ? string.Empty : sessReader.GetString(1);
                        sessionLabel = !string.IsNullOrWhiteSpace(sName) ? sName : (!string.IsNullOrWhiteSpace(sCreated) ? sCreated : sessionLabel);
                    }
                    sessReader.Close();

                    // Confirm delete
                    Console.WriteLine();
                    Console.WriteLine($"You are about to delete driver '{drvName}' (Id: {drvId}) from session: {sessionLabel}");
                    Console.Write("Confirm delete? (Y/N): ");
                    var confirm = Console.ReadLine() ?? string.Empty;
                    if (!string.Equals(confirm.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Delete cancelled. Press any key to return.");
                        Console.ReadKey();
                        continue;
                    }

                    // Perform deletion
                    cmd.CommandText = "DELETE FROM Drivers WHERE Id = $id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$id", deleteId);
                    var affected = cmd.ExecuteNonQuery();
                    Console.WriteLine(affected > 0 ? $"Deleted driver '{drvName}' (Id {deleteId})." : $"No driver found with Id {deleteId}.");
                    Console.WriteLine("Press any key to return to sessions menu...");
                    Console.ReadKey();
                    continue; // redisplay menu
                }

                // numeric choice => attempt to parse and return selected session modules
                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= sessions.Count)
                {
                    var sessionId = sessions[choice - 1].Id;

                    // Load latest driver for session
                    cmd.CommandText = "SELECT Name, Age, Experience FROM Drivers WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);
                    string name = string.Empty;
                    byte age = 0;
                    string experience = string.Empty;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            age = reader.IsDBNull(1) ? (byte)0 : (byte)reader.GetInt32(1);
                            experience = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        }
                    }

                    // Aerodynamics
                    cmd.CommandText = "SELECT FrontWing, RearWing, DRSEnabled, DownforceLevel, WingAngle FROM Aerodynamics WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    string fw = "", rw = "", drs = "", df = "", wa = "";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fw = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            rw = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            drs = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            df = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            wa = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                        }
                    }

                    // Engine
                    cmd.CommandText = "SELECT EngineType, EnginePower, Transmission, ERSMode, ERSBoost FROM Engine WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    string et = "", ep = "", tr = "", em = "", eb = "";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            et = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            ep = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            tr = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            em = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            eb = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                        }
                    }

                    // Wheels
                    cmd.CommandText = "SELECT TireCompound, TirePressure FROM Wheels WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    string tc = ""; byte tp = 0;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tc = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            tp = reader.IsDBNull(1) ? (byte)0 : (byte)reader.GetInt32(1);
                        }
                    }

                    // Suspension
                    cmd.CommandText = "SELECT SuspensionType, SuspensionLevel, SteeringLevel, RideHeight, CamberToeAngles FROM Suspension WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    string st = "", sl = "", se = "", rh = "", ca = "";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            st = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            sl = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            se = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            rh = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            ca = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                        }
                    }

                    // Brakes
                    cmd.CommandText = "SELECT BrakeType, BrakeLevel, ABSEnabled FROM Brakes WHERE SessionId = $sid ORDER BY Id DESC LIMIT 1;";
                    string bt = "", bl = "", ab = "";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bt = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            bl = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            ab = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        }
                    }

                    // Build modules in the same order
                    CarModule[] modules = new CarModule[]
                    {
                        new TeamLivery(name, age, experience),
                        new Aerodynamics(fw, rw, drs, df, wa),
                        new EnginePowertrain(et, ep, tr, em, eb),
                        new WheelAndTire(tc, tp),
                        new SuspensionHandling(st, sl, se, rh, ca), // Updated to reflect changes
                        new BrakingSystem(bt, bl, ab)
                    };

                    return modules;
                }

                Console.WriteLine("Invalid choice. Press any key to try again.");
                Console.ReadKey();
            }
        }
    }
}