//https://docs.microsoft.com/en-us/azure/mysql/connect-csharp
using MySql.Data.MySqlClient;
using System;
using System.Net.NetworkInformation;
using System.Text;

namespace LINQSQL
{
    class AzureConnect
    {
        private MySqlConnection connection;

        static void Main(string[] args)
        {
            AzureConnect databaseObject = new AzureConnect();
            //Console.WriteLine(databaseObject.ModifyConnection("open"));
            //Console.WriteLine(databaseObject.connection.Ping());
            databaseObject.CreateTable();
            databaseObject.Insert();
            Console.ReadKey();
        }

        public AzureConnect()
        {
            Initialize();
        }


        /// <summary>
        /// server  : chkoomysql.mysql.database.azure.com, port 3306
        /// admin   : serveradmin
        /// pw      : a123456A
        /// </summary>
        /// Server=tcp:chkoosqltestserver.database.windows.net,1433;Initial Catalog=mySampleDatabase;
        /// Persist Security Info=False;User ID={your_username};Password={your_password};
        /// MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
        private void Initialize()
        {
            var connectionString = new MySqlConnectionStringBuilder {
                Server = "chkoomysql.mysql.database.azure.com",
                Port = 3306,
                UserID = "serveradmin@chkoomysql",
                Password = "a123456A",
                Database = "exercisesdb",
                SslMode = MySqlSslMode.Required,
            };

            connection = new MySqlConnection(connectionString.ToString());
            Console.WriteLine(connectionString.ConnectionString);
            Console.WriteLine("Initialized connection.");
        }
        public bool isConnectedToServer()
        {
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send("chkoosqltestserver.database.windows.net", 1433);
                if (reply.Status == IPStatus.Success)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ModifyConnection(string option)
        {
            try
            {
                if (option.ToLower().Trim().Equals("open"))
                {
                    connection.Open();
                    return true;
                }
                else if (option.ToLower().Trim().Equals("close"))
                {
                    connection.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error has occured!+\n" + ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// Create table exercises: Documented exercise names and id. category
        /// Another table exercise information: e 
        /// </summary>
        public void CreateTable()
        {
            string[] tablesToCreate = new string[3];
            tablesToCreate[0] = "CREATE TABLE IF NOT EXISTS exercises "
                + "(id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT, name VARCHAR(30) NOT NULL, "
                + "difficulty ENUM ('beginner', 'intermediate', 'advanced'), rating FLOAT(3,1) "
                + "CHECK (rating >= 0 AND rating <= 10), PRIMARY KEY(id));";
            tablesToCreate[1] = "CREATE TABLE IF NOT EXISTS exercise_info " +
                "(id TINYINT UNSIGNED, description VARCHAR(255), PRIMARY KEY (id), FOREIGN KEY (id) REFERENCES exercises(id));";
            tablesToCreate[2] = "CREATE TABLE IF NOT EXISTS bodypart (id TINYINT UNSIGNED, body_part VARCHAR(30), FOREIGN KEY (id) REFERENCES exercises(id));";
            try
            {
                if (this.ModifyConnection("open"))
                {
                    MySqlCommand cmd;
                    for (int i = 0; i < tablesToCreate.Length; ++i)
                    {
                        cmd = new MySqlCommand(tablesToCreate[i], connection);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table " + tablesToCreate[i].Split(' ')[2] + " created!");
                    }
                    this.ModifyConnection("close");
                }
            }
            catch (MySqlException ex)
            {
                this.ModifyConnection("close");
                Console.WriteLine("Error! " + ex.ToString());
            }
        }

        public bool Insert()
        {
            try
            {
                string input;
                do
                {
                    Console.WriteLine("Enter: exercise_name, difficulty (beginner, intermediate, advanced), rating(0-10), description, body part1, body part 2, ...bodypart n");
                    input = Console.ReadLine();
                } while (input.Split(' ').Length < 4);

                string[] inputSplit = input.Split(new string[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries);
                //          StringBuilder insertToExercises = new StringBuilder("INSERT INTO exercises (name, difficulty, difficulty, rating) VALUES ('"
                //             + inputSplit[0] + "', '" + inputSplit[1] + "', '" + inputSplit[2] + "', '" + inputSplit[3] + "');");

                string insertToExercises = string.Format("INSERT INTO exercises (name, difficulty, rating) "
                    + "VALUES ('{0}', '{1}', '{2}');", inputSplit[0], inputSplit[1], inputSplit[2]);
                string insertToExerciseInfo = string.Format("INSERT INTO exercise_info (id, description) "
                    + "VALUES (LAST_INSERT_ID(), '{0}');", inputSplit[3]);

                string[] insertToBodyPart = new string[inputSplit.Length - 4];

                for (int i = 4, count = 0; i < inputSplit.Length; ++i, ++count)
                {
                    insertToBodyPart[count] = string.Format("INSERT INTO bodypart (id, body_part) "
                        + "VALUES (LAST_INSERT_ID(), '{0}');", inputSplit[i]);
                }
                StringBuilder sqlCommand = new StringBuilder("BEGIN; " + insertToExercises + insertToExerciseInfo);
                for (int i = 0; i < insertToBodyPart.Length; ++i)
                {
                    sqlCommand.Append(insertToBodyPart[i]);
                }
                sqlCommand.Append("COMMIT;");
                Console.WriteLine(sqlCommand.ToString());


                if (this.ModifyConnection("open"))
                {
                    MySqlCommand cmd = new MySqlCommand(insertToExercises.ToString(), connection);
                    cmd.ExecuteNonQuery();
                    
                    this.ModifyConnection("close");
                    return true;
                }
                return false;
            }
            catch (MySqlException ex)
            {
                this.ModifyConnection("close");
                Console.WriteLine("Error!" + ex.ToString());
                return false;
            }
        }
        public bool DropTables()
        {
            try
            {
                if (this.ModifyConnection("open"))
                {
                    string query = "drop table exercises; drop table exercise_info; drop table bodypart;";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    this.ModifyConnection("close");
                    return true;
                }
                return false;
            }
            catch (MySqlException ex)
            {
                this.ModifyConnection("close");
                Console.WriteLine("Error! " + ex.ToString());
                return false;
            }
        }
    }
}
