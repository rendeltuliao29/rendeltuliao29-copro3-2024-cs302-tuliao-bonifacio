using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace CJR_Racing
{
    public static class DatabaseHelper
    {
        private static readonly string DbFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "db.db");

        // Process-wide writer lock to serialize all DB schema/insert/delete writes
        private static readonly object DbWriteLock = new();

        public static void path()
        {
            Console.WriteLine("DB Path: " + DbFile);
        }

        // Create connections with a BusyTimeout and shared cache to reduce transient locks
        private static SqliteConnection CreateConnection()
        {
            var cs = new SqliteConnectionStringBuilder
            {
                DataSource = DbFile,
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.ReadWriteCreate,
                DefaultTimeout = 60
            }.ToString();

            return new SqliteConnection(cs);
        }

        // EnsureDatabase modifies schema — serialize with the writer lock to avoid races
        public static void EnsureDatabase()
        {
            lock (DbWriteLock)
            {
                using var conn = CreateConnection();
                Console.Clear();
                Console.WriteLine("Please wait...");
                Thread.Sleep(1500);
                conn.Open();

                // Make SQLite wait for locks instead of failing immediately
                using (var busyCmd = conn.CreateCommand())
                {
                    busyCmd.CommandText = "PRAGMA busy_timeout = 10000;"; // 10s
                    busyCmd.ExecuteNonQuery();
                }

                using var cmd = conn.CreateCommand();

                // Enable WAL to reduce writer/readers lock contention
                cmd.CommandText = "PRAGMA journal_mode = WAL;";
                cmd.ExecuteNonQuery();

                // Create minimal Sessions table if missing
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Sessions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CreatedAt TEXT,
                        Name TEXT
                    );
                ";
                cmd.ExecuteNonQuery();

                // Ensure every expected column exists (safe migration for older DBs)
                EnsureColumnExists(conn, "Sessions", "DriverName", "TEXT");
                EnsureColumnExists(conn, "Sessions", "DriverAge", "INTEGER");
                EnsureColumnExists(conn, "Sessions", "Experience", "TEXT");

                EnsureColumnExists(conn, "Sessions", "FrontWing", "TEXT");
                EnsureColumnExists(conn, "Sessions", "RearWing", "TEXT");
                EnsureColumnExists(conn, "Sessions", "DRSEnabled", "TEXT");
                EnsureColumnExists(conn, "Sessions", "DownforceLevel", "TEXT");
                EnsureColumnExists(conn, "Sessions", "WingAngle", "TEXT");

                EnsureColumnExists(conn, "Sessions", "EngineType", "TEXT");
                EnsureColumnExists(conn, "Sessions", "EnginePower", "TEXT");
                EnsureColumnExists(conn, "Sessions", "Transmission", "TEXT");
                EnsureColumnExists(conn, "Sessions", "ERSMode", "TEXT");
                EnsureColumnExists(conn, "Sessions", "ERSBoost", "TEXT");

                EnsureColumnExists(conn, "Sessions", "TireCompound", "TEXT");
                EnsureColumnExists(conn, "Sessions", "TirePressure", "INTEGER");

                EnsureColumnExists(conn, "Sessions", "SuspensionType", "TEXT");
                EnsureColumnExists(conn, "Sessions", "SuspensionLevel", "TEXT");
                EnsureColumnExists(conn, "Sessions", "SteeringLevel", "TEXT");
                EnsureColumnExists(conn, "Sessions", "RideHeight", "TEXT");
                EnsureColumnExists(conn, "Sessions", "CamberToeAngles", "TEXT");

                EnsureColumnExists(conn, "Sessions", "BrakeType", "TEXT");
                EnsureColumnExists(conn, "Sessions", "BrakeLevel", "TEXT");
                EnsureColumnExists(conn, "Sessions", "ABSEnabled", "TEXT");

                Console.Clear();
                Console.WriteLine("Getting ready...");
                
            }
        }

        // Adds a column if it does not exist in the specified table
        private static void EnsureColumnExists(SqliteConnection conn, string table, string column, string definition)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = $"PRAGMA table_info(\"{table}\");";

            bool found = false;
            // Dispose reader immediately to avoid holding a read-lock during ALTER TABLE
            using (var reader = checkCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var colName = reader.GetString(1);
                    if (string.Equals(colName, column, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                const int maxAttempts = 6;
                int attempt = 0;
                while (true)
                {
                    try
                    {
                        using var alterCmd = conn.CreateCommand();
                        alterCmd.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition};";
                        alterCmd.ExecuteNonQuery();
                        break; // success
                    }
                    catch (SqliteException ex)
                    {
                        attempt++;
                        if (attempt >= maxAttempts || (ex.SqliteErrorCode != 5 && !ex.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase)))
                            throw;

                        Thread.Sleep(200 * attempt);
                    }
                }
            }
        }

        public static void SaveModules(CarModule[] modules)
        {
            // Collect all values and user input before touching the DB to avoid holding a write lock during Console.ReadLine.
            Console.Write("Enter a name for this save (optional): ");
            var sessionName = Console.ReadLine() ?? string.Empty;

            // Collect module data
            string driverName = string.Empty;
            int driverAge = 0;
            string driverExperience = string.Empty;

            string fw = string.Empty, rw = string.Empty, drs = string.Empty, df = string.Empty, wa = string.Empty;

            string et = string.Empty, ep = string.Empty, tr = string.Empty, em = string.Empty, eb = string.Empty;

            string tc = string.Empty; int tp = 0;

            string st = string.Empty, sl = string.Empty, se = string.Empty, rh = string.Empty, ca = string.Empty;

            string bt = string.Empty, bl = string.Empty, ab = string.Empty;

            foreach (var module in modules)
            {
                var data = module.GetData();
                if (data is TeamLivery.DriverData d)
                {
                    driverName = d.Name;
                    driverAge = d.Age;
                    driverExperience = d.Experience;
                }
                else if (data is Aerodynamics.AeroData a)
                {
                    fw = a.FrontWing;
                    rw = a.RearWing;
                    drs = a.DRSEnabled;
                    df = a.DownforceLevel;
                    wa = a.WingAngle;
                }
                else if (data is EnginePowertrain.EngineData e)
                {
                    et = e.EngineType;
                    ep = e.EnginePower;
                    tr = e.Transmission;
                    em = e.ERSMode;
                    eb = e.ERSBoost;
                }
                else if (data is WheelAndTire.WheelData w)
                {
                    tc = w.TireCompound;
                    tp = w.TirePressure;
                }
                else if (data is SuspensionHandling.SuspensionData s)
                {
                    st = s.SuspensionType;
                    sl = s.SuspensionLevel;
                    se = s.SteeringLevel;
                    rh = s.RideHeight;
                    ca = s.CamberToeAngles;
                }
                else if (data is BrakingSystem.BrakeData b)
                {
                    bt = b.BrakeType;
                    bl = b.BrakeLevel;
                    ab = b.ABSEnabled;
                }
            }

            // Perform DB write inside process-wide lock to avoid concurrent writers
            lock (DbWriteLock)
            {
                try
                {
                    EnsureDatabase(); // will run under the same lock
                    using var conn = CreateConnection();
                    Console.Clear();
                    Console.WriteLine("Saving...");
                    conn.Open();

                    using (var busyCmd = conn.CreateCommand())
                    {
                        busyCmd.CommandText = "PRAGMA busy_timeout = 10000;"; // 10s
                        busyCmd.ExecuteNonQuery();
                    }

                    using var tx = conn.BeginTransaction();
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = tx;

                    cmd.CommandText = @"
                        INSERT INTO Sessions (
                            CreatedAt, Name,
                            DriverName, DriverAge, Experience,
                            FrontWing, RearWing, DRSEnabled, DownforceLevel, WingAngle,
                            EngineType, EnginePower, Transmission, ERSMode, ERSBoost,
                            TireCompound, TirePressure,
                            SuspensionType, SuspensionLevel, SteeringLevel, RideHeight, CamberToeAngles,
                            BrakeType, BrakeLevel, ABSEnabled
                        ) VALUES (
                            $now, $name,
                            $dName, $dAge, $dExp,
                            $fw, $rw, $drs, $df, $wa,
                            $et, $ep, $tr, $em, $eb,
                            $tc, $tp,
                            $st, $sl, $se, $rh, $ca,
                            $bt, $bl, $ab
                        );
                    ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
                    cmd.Parameters.AddWithValue("$name", sessionName);

                    cmd.Parameters.AddWithValue("$dName", driverName);
                    cmd.Parameters.AddWithValue("$dAge", driverAge);
                    cmd.Parameters.AddWithValue("$dExp", driverExperience);

                    cmd.Parameters.AddWithValue("$fw", fw);
                    cmd.Parameters.AddWithValue("$rw", rw);
                    cmd.Parameters.AddWithValue("$drs", drs);
                    cmd.Parameters.AddWithValue("$df", df);
                    cmd.Parameters.AddWithValue("$wa", wa);

                    cmd.Parameters.AddWithValue("$et", et);
                    cmd.Parameters.AddWithValue("$ep", ep);
                    cmd.Parameters.AddWithValue("$tr", tr);
                    cmd.Parameters.AddWithValue("$em", em);
                    cmd.Parameters.AddWithValue("$eb", eb);

                    cmd.Parameters.AddWithValue("$tc", tc);
                    cmd.Parameters.AddWithValue("$tp", tp);

                    cmd.Parameters.AddWithValue("$st", st);
                    cmd.Parameters.AddWithValue("$sl", sl);
                    cmd.Parameters.AddWithValue("$se", se);
                    cmd.Parameters.AddWithValue("$rh", rh);
                    cmd.Parameters.AddWithValue("$ca", ca);

                    cmd.Parameters.AddWithValue("$bt", bt);
                    cmd.Parameters.AddWithValue("$bl", bl);
                    cmd.Parameters.AddWithValue("$ab", ab);

                    const int maxExecAttempts = 8;
                    int execAttempt = 0;
                    while (true)
                    {
                        try
                        {
                            
                            cmd.ExecuteNonQuery();
                           
                            break;
                        }
                        catch (SqliteException ex)
                        {
                            execAttempt++;
                            
                            if (execAttempt >= maxExecAttempts || (ex.SqliteErrorCode != 5 && !ex.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase)))
                                throw;

                            Thread.Sleep(250 * execAttempt);
                        }
                    }

                    tx.Commit();
                    Console.Clear();
                    Console.WriteLine("Saved successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DB] SaveModules: Exception: " + ex);
                    throw;
                }
            }
        }

        // Load the latest saved module configuration
        public static CarModule[]? LoadModules()
        {
            EnsureDatabase();

            using var conn = CreateConnection();
            Console.WriteLine("Connecting...");
            conn.Open();

            using (var busyCmd = conn.CreateCommand())
            {
                busyCmd.CommandText = "PRAGMA busy_timeout = 10000;"; // 10s
                busyCmd.ExecuteNonQuery();
            }

            using var cmd = conn.CreateCommand();

            // List available sessions (single table)
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("╔═══════════════╗");
                Console.WriteLine("║   LOAD GAME   ║");
                Console.WriteLine("╚═══════════════╝");
                Console.WriteLine("Select a saved session to load (enter number), or:");
                Console.WriteLine("\n[A] Show all characters (Drivers)");
                Console.WriteLine("[B] Enter session ID to delete:");
                Console.WriteLine("[C] Back to main menu");
                Console.WriteLine("═══════════════════════════════════");
                Console.WriteLine();
                
                // re-query sessions each loop to reflect deletes
                cmd.CommandText = "SELECT Id, CreatedAt, Name FROM Sessions ORDER BY Id DESC;";
                sessions.Clear();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        sessions.Add((r.GetInt64(0), r.IsDBNull(1) ? string.Empty : r.GetString(1), r.IsDBNull(2) ? string.Empty : r.GetString(2)));
                }

                if (sessions.Count == 0)
                    return Array.Empty<CarModule>();

                for (int i = 0; i < sessions.Count; i++)
                {
                    var s = sessions[i];
                    var label = !string.IsNullOrWhiteSpace(s.Name)
                        ? s.Name
                        : (!string.IsNullOrWhiteSpace(s.CreatedAt) ? s.CreatedAt : $"Session {s.Id}");

                    Console.WriteLine($"[{i + 1}] {label} (Id: {s.Id})");
                }

                Console.WriteLine();
                Console.Write("Enter choice number or letter: ");
                
                var input = Console.ReadLine() ?? string.Empty;
                input = input.Trim();

                if (string.Equals(input, "C", StringComparison.OrdinalIgnoreCase))
                {
                    // User chose to go back to the main menu
                    return null;
                }
                
                if (string.Equals(input, "A", StringComparison.OrdinalIgnoreCase))
                {
                    cmd.CommandText = "SELECT Id, DriverName, DriverAge, Experience FROM Sessions ORDER BY Id;";
                    cmd.Parameters.Clear();
                    using var rdr = cmd.ExecuteReader();

                    var rows = new System.Collections.Generic.List<(long Id, string Name, int Age, string Experience)>();
                    while (rdr.Read())
                    {
                        var id = rdr.IsDBNull(0) ? 0L : rdr.GetInt64(0);
                        var name = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                        var age = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                        var exp = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3);

                        rows.Add((id, name, age, exp));
                    }

                    Console.WriteLine();
                    Console.Clear();
                    Console.WriteLine("All characters (Drivers):");

                    if (rows.Count == 0)
                    {
                        Console.WriteLine("  (no drivers found)");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to return to sessions menu...");
                        Console.ReadKey();
                        continue; // redisplay menu
                    }

                    const int maxNameWidth = 30;
                    const int maxExpWidth = 40;

                    string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max - 3) + "...";

                    Console.WriteLine(
                        $"{ "Id".PadLeft(3) }  " +
                        $"{ "Name".PadRight(maxNameWidth) }  " +
                        $"{ "Age".PadLeft(3) }  " +
                        $"{ "Experience".PadRight(maxExpWidth) }");

                    Console.WriteLine(
                        $"{ new string('-', 3) }  " +
                        $"{ new string('-', maxNameWidth) }  " +
                        $"{ new string('-', 3) }  " +
                        $"{ new string('-', maxExpWidth) }");

                    foreach (var r in rows)
                    {
                        var nameDisplay = Truncate(r.Name ?? string.Empty, maxNameWidth);
                        var expDisplay = Truncate(r.Experience ?? string.Empty, maxExpWidth);

                        Console.WriteLine(
                            $"{ r.Id.ToString().PadLeft(3) }  " +
                            $"{ nameDisplay.PadRight(maxNameWidth) }  " +
                            $"{ r.Age.ToString().PadLeft(3) }  " +
                            $"{ expDisplay.PadRight(maxExpWidth) }");
                    }

                    Console.WriteLine();
                    Console.WriteLine("Press any key to return to sessions menu...");
                    Console.ReadKey();
                    continue; // redisplay menu
                }
                    
                if (string.Equals(input, "B", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("Enter Session Id to delete: ");
                    var idInput = Console.ReadLine() ?? string.Empty;
                    
                    if (!long.TryParse(idInput.Trim(), out var deleteId))
                    {
                        Console.WriteLine("Invalid id. Press any key to return.");
                        Console.ReadKey();
                        continue;
                    }

                    cmd.CommandText = "SELECT Id, DriverName, DriverAge, Experience FROM Sessions WHERE Id = $id;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$id", deleteId);

                    string drvName = string.Empty;
                    int drvAge = 0;
                    string drvExperience = string.Empty;

                    using (var drvReader = cmd.ExecuteReader())
                    {
                        if (!drvReader.Read())
                        {
                            Console.WriteLine($"No session found with Id {deleteId}. Press any key to return.");
                            Console.ReadKey();
                            continue;
                        }

                        drvName = drvReader.IsDBNull(1) ? string.Empty : drvReader.GetString(1);
                        drvAge = drvReader.IsDBNull(2) ? 0 : drvReader.GetInt32(2);
                        drvExperience = drvReader.IsDBNull(3) ? string.Empty : drvReader.GetString(3);
                    }

                    const int maxNameWidth = 30;
                    const int maxExpWidth = 40;
                    string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max - 3) + "...";

                    void PrintRow()
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            $"{ "Id".PadLeft(3) }  " +
                            $"{ "Name".PadRight(maxNameWidth) }  " +
                            $"{ "Age".PadLeft(3) }  " +
                            $"{ "Experience".PadRight(maxExpWidth) }");
                        Console.WriteLine(
                            $"{ new string('-', 3) }  " +
                            $"{ new string('-', maxNameWidth) }  " +
                            $"{ new string('-', 3) }  " +
                            $"{ new string('-', maxExpWidth) }");

                        var nameDisplay = Truncate(drvName ?? string.Empty, maxNameWidth);
                        var expDisplay = Truncate(drvExperience ?? string.Empty, maxExpWidth);

                        Console.WriteLine(
                            $"{ deleteId.ToString().PadLeft(3) }  " +
                            $"{ nameDisplay.PadRight(maxNameWidth) }  " +
                            $"{ drvAge.ToString().PadLeft(3) }  " +
                            $"{ expDisplay.PadRight(maxExpWidth) }");
                        Console.WriteLine();
                    }
                    Console.Clear();
                    PrintRow();

                    Console.WriteLine($"You are about to delete the session shown above.");
                    while (true)
                    {
                        Console.Clear();
                        PrintRow();
                        Console.WriteLine("You are about to delete the session shown above.");
                        Console.Write("Confirm delete? (Y/N): ");

                        string confirm = Console.ReadLine()?.Trim().ToUpper() ?? "";

                        // Accept only Y or N
                        if (confirm == "Y")
                        {
                            Console.Clear();
                            Console.WriteLine("Deleting...");
                            Thread.Sleep(500);
                            // your delete logic here

                            break; // exit the loop after delete
                        }
                        else if (confirm == "N")
                        {
                            Console.WriteLine("Delete cancelled. Press any key to return.");
                            Console.ReadKey();
                            break; // exit the loop, go back to parent loop
                        }

                        // Invalid input
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Please enter only Y or N.");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to try again...");
                        Console.ReadKey();
                    }


                    // Serialize the delete with the writer lock
                    lock (DbWriteLock)
                    {
                        using var tx = conn.BeginTransaction();
                        using var deleteCmd = conn.CreateCommand();
                        deleteCmd.Transaction = tx;

                        deleteCmd.CommandText = "DELETE FROM Sessions WHERE Id = $id;";
                        deleteCmd.Parameters.Clear();
                        deleteCmd.Parameters.AddWithValue("$id", deleteId);
                        var delRow = deleteCmd.ExecuteNonQuery();

                        tx.Commit();

                        Console.WriteLine();
                        Console.Clear();
                        Console.WriteLine(delRow > 0 ? "Deletion succesful!." : $"No session found with Id {deleteId}.");
                        Console.WriteLine("Press any key to return to sessions menu...");
                        Console.ReadKey();
                    }

                    continue; // redisplay menu
                }

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= sessions.Count)
                {
                    var sessionId = sessions[choice - 1].Id;

                    // Load all module columns from single Sessions row
                    cmd.CommandText = @"
                        SELECT
                            DriverName, DriverAge, Experience,
                            FrontWing, RearWing, DRSEnabled, DownforceLevel, WingAngle,
                            EngineType, EnginePower, Transmission, ERSMode, ERSBoost,
                            TireCompound, TirePressure,
                            SuspensionType, SuspensionLevel, SteeringLevel, RideHeight, CamberToeAngles,
                            BrakeType, BrakeLevel, ABSEnabled
                        FROM Sessions WHERE Id = $sid LIMIT 1;
                    ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("$sid", sessionId);

                    string name = string.Empty;
                    byte age = 0;
                    string experience = string.Empty;

                    string a_fw = "", a_rw = "", a_drs = "", a_df = "", a_wa = "";
                    string e_et = "", e_ep = "", e_tr = "", e_em = "", e_eb = "";
                    string w_tc = ""; byte w_tp = 0;
                    string s_st = "", s_sl = "", s_se = "", s_rh = "", s_ca = "";
                    string b_bt = "", b_bl = "", b_ab = "";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            age = reader.IsDBNull(1) ? (byte)0 : (byte)reader.GetInt32(1);
                            experience = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                            a_fw = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            a_rw = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            a_drs = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            a_df = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                            a_wa = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);

                            e_et = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                            e_ep = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                            e_tr = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                            e_em = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                            e_eb = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);

                            w_tc = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
                            w_tp = reader.IsDBNull(14) ? (byte)0 : (byte)reader.GetInt32(14);

                            s_st = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);
                            s_sl = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                            s_se = reader.IsDBNull(17) ? string.Empty : reader.GetString(17);
                            s_rh = reader.IsDBNull(18) ? string.Empty : reader.GetString(18);
                            s_ca = reader.IsDBNull(19) ? string.Empty : reader.GetString(19);

                            b_bt = reader.IsDBNull(20) ? string.Empty : reader.GetString(20);
                            b_bl = reader.IsDBNull(21) ? string.Empty : reader.GetString(21);
                            b_ab = reader.IsDBNull(22) ? string.Empty : reader.GetString(22);
                        }
                    }

                    CarModule[] modules = new CarModule[]
                    {
                        new TeamLivery(name, age, experience),
                        new Aerodynamics(a_fw, a_rw, a_drs, a_df, a_wa),
                        new EnginePowertrain(e_et, e_ep, e_tr, e_em, e_eb),
                        new WheelAndTire(w_tc, w_tp),
                        new SuspensionHandling(s_st, s_sl, s_se, s_rh, s_ca),
                        new BrakingSystem(b_bt, b_bl, b_ab)
                    };

                    return modules;
                }

                Console.WriteLine("Invalid choice. Press any key to try again.");
                Console.ReadKey();
            }
        }
    }
}