using System;
using MySql.Data.MySqlClient;

namespace CanFlyPipeline
{
    public class db
    {
        public static void TestDatabaseConnection()
        {
            // Connection details
            string server = "monorail.proxy.rlwy.net";
            int port = 18609;
            string database = "railway";
            string user = "root";
            string password = "yAweUMNeUTfMIQtAtqZmzUpjNAqzvLQV";

            // Connection string
            string connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};SslMode=Preferred;";

            try
            {
                Console.WriteLine("Attempting to connect to the MySQL database...");

                // Create a new MySQL connection
                using (var connection = new MySqlConnection(connectionString))
                {
                    // Open the connection
                    connection.Open();
                    Console.WriteLine("Connection to the MySQL database successful.");

                    // Example query to check the connection
                    string query = "SELECT NOW();";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Execute the query and read the result
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"Current Date and Time: {reader.GetDateTime(0)}");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("A MySQL error occurred:");
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
