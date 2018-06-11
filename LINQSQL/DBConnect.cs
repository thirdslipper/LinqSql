/*
 * Author: Colin Koo
 * Date: 6/11/18
 * Description: This program is to practice setting up and utilizing LINQ in C# by initially
 * following a guide, then either modifying the hardcoded methods or making a separate class
 * based on this.
 * Source: https://www.codeproject.com/Articles/43438/Connect-C-to-MySQL
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// add reference to mysql.data, then change mysql.data property to copy local = true.
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
//using System.Data.SqlClient;

namespace LINQSQL
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string userid;
        private string password;
        static void Main(string[] args)
        {
            DBConnect dbconnect = new DBConnect();
        }

        public DBConnect() 
        {
            initialize();
        }

        /// <summary>28 32 36
        /// server  : chkoosqltestserver, port 1433
        /// admin   : serveradmin
        /// pw      : a123456A
        /// </summary>
        private void initialize()   
        {
            server = "localhost";
            database = "connectcsharptomysql";
            userid = "username";
            password = "password";
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + userid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.");
                        break;
                    case 1045:
                        MessageBox.Show("Invalid username/password");
                        break;
                }
                return false;
            }
        }
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void Insert()
        {
            string query = "INSERT INTO tableinfo (name, age) VALUES ('John Smith', '33')";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        private void Update()
        {
            string query = "UPDATE tableinfo SET name ='Joe', age='22' WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = query;
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        private void Delete()
        {
            String query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }
        public List <string>[] Select()
        {
            string query = "SELECT * FROM tableinfo";
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {   //string indices of result
                    list[0].Add(dataReader["id"] + "");
                    list[0].Add(dataReader["name"] + "");
                    list[0].Add(dataReader["age"] + "");
                }
                dataReader.Close();
                this.CloseConnection();
                return list;
            }
            else
            {
                return list;
            }
        }
        public int Count()
        {
            string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                    //returns the first column of first row of result set, or null
                bool result = int.TryParse(cmd.ExecuteScalar()+"", out Count);
                //default method used is Count = int.Parse(cmd.ExecuteScalar()+ "");
            }
            return Count;
        }
        //filename -u username -p password -h localhost DBName > "C:\Backup.sql"
        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                string path = "C:\\MySqlBackUp" + year + "-" + month + day +
                    "-" + hour + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} - p{1} -h{2} {3}", userid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error, unable to backup"); 
            }
        }
        //filename -u username -p password -h localhost DBName < "C:\Backup.sql"
        public void Restore()
        {
            try
            {
                string path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", userid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error, unable to restore");
            }
        }
    }
}
