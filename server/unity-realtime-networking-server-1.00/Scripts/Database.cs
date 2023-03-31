using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace DevelopersHub.RealtimeNetworking.Server
{
    class Database
    {

        #region MySQL
        
        private static MySqlConnection _mysqlConnection;
        private const string _mysqlServer = "127.0.0.1";
        private const string _mysqlUsername = "root";
        private const string _mysqlPassword = "";
        private const string _mysqlDatabase = "city_business";

        public static MySqlConnection mysqlConnection
        {
            get
            {
                if (_mysqlConnection == null || _mysqlConnection.State == ConnectionState.Closed)
                {
                    try
                    {
                        _mysqlConnection = new MySqlConnection("SERVER=" + _mysqlServer + "; DATABASE=" + _mysqlDatabase + "; UID=" + _mysqlUsername + "; PASSWORD=" + _mysqlPassword + ";");
                        _mysqlConnection.Open();
                        Console.WriteLine("Connection established with MySQL database.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to connect the MySQL database.");
                    }
                }
                else if (_mysqlConnection.State == ConnectionState.Broken)
                {
                    try
                    {
                        _mysqlConnection.Close();
                        _mysqlConnection = new MySqlConnection("SERVER=" + _mysqlServer + "; DATABASE=" + _mysqlDatabase + "; UID=" + _mysqlUsername + "; PASSWORD=" + _mysqlPassword + ";");
                        _mysqlConnection.Open();
                        Console.WriteLine("Connection re-established with MySQL database.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to connect the MySQL database.");
                    }
                }
                return _mysqlConnection;
            }
        }

        public static void Demo_MySQL_1()
        {
            string query = String.Format("UPDATE table SET int_column = {0}, string_column = '{1}', datetime_column = NOW();", 123, "Hello World");
            using (MySqlCommand command = new MySqlCommand(query, mysqlConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void Demo_MySQL_2()
        {
            string query = String.Format("SELECT column1, column2 FROM table WHERE column3 = {0} ORDER BY column1 DESC;", 123);
            using (MySqlCommand command = new MySqlCommand(query, mysqlConnection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int column1 = int.Parse(reader["column1"].ToString());
                            string column2 = reader["column2"].ToString();
                        }
                    }
                }
            }
        }

        public async static void AuthenticatePlayer(int clientID, string device)
        {
            long account_id = await AuthenticatePlayerAsync(clientID, device);
            Sender.TCP_Send(clientID, 1, account_id);
        }
        private async static Task<long> AuthenticatePlayerAsync(int clientID, string device)
        {
            Task<long> task = Task.Run(() =>
            {
                long account_id = 0;
                string query = String.Format("SELECT id FROM accounts WHERE device_id = '{0}';", device);
                bool found = false;
                using (MySqlCommand command = new MySqlCommand(query, mysqlConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                 account_id = long.Parse(reader["ID"].ToString());
                                
                                found = true;
                            }
                        }

                    }
                }
                if (found == false)
                {
                    query = String.Format("INSERT INTO accounts (Device_ID) VALUES('{0}');", device);
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnection))
                    {
                        command.ExecuteNonQuery();
                        account_id = command.LastInsertedId;
                        
                    }
                }
                return account_id;


            });
            return await task;
            
        }


        public async static void SyncPlayerData(int clientID, string device)
        {
            Data.Player player = await SyncPlayerDataAsync(clientID, device);
            string data = Data.Serialize<Data.Player>(player);
            Sender.TCP_Send(clientID, 2, data);
           // Sender.TCP_Send(clientID, 1, account_id);
        }
        private async static Task<Data.Player> SyncPlayerDataAsync(int clientID, string device)
        {
            Task<Data.Player> task = Task.Run(() =>
            {
                Data.Player data = new Data.Player();
                string query = String.Format("SELECT id, gold, Gems FROM accounts WHERE device_id = '{0}';", device);
                
                using (MySqlCommand command = new MySqlCommand(query, mysqlConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                //data.id = long.Parse(reader["ID"].ToString());

                                data.gold = int.Parse(reader["gold"].ToString());
                                data.gems = int.Parse(reader["Gems"].ToString());
                            }
                        }

                    }
                }
                
                return data;


            });
            return await task;

        }

        #endregion



    }
}