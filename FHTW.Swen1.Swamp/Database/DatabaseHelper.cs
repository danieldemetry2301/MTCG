using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;

namespace FHTW.Swen1.Swamp.Database
{
    public class DatabaseHelper
    {
        private const string DatabaseFileName = "mtcg";
        private const string DataConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=mtcg";

        public static void CreateDatabase()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                connection.Close();
            }
        }

        public static void CreateTables()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id SERIAL PRIMARY KEY,
                        Username VARCHAR(255),
                        Password VARCHAR(255),
                        Coins INTEGER NOT NULL
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Packages (
                        Id UUID PRIMARY KEY
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Cards (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Damage DOUBLE PRECISION NOT NULL,
                        PackageId UUID,
                        UserId INTEGER,
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (PackageId) REFERENCES Packages(Id)
                    )");

                connection.Close();
            }
        }

        public static void InsertUser(User user)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var insertUserCommand = $"INSERT INTO Users (Username, Password, Coins) VALUES ('{user.Username}', '{user.Password}', {user.Coins})";
                ExecuteCommand(connection, insertUserCommand);

                connection.Close();
            }
        }

        public static void InsertPackage(Package package)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var packageCommand = $"INSERT INTO Packages (Id) VALUES ('{package.Id}')";
                ExecuteCommand(connection, packageCommand);

                connection.Close();
            }
        }

        public static void InsertCards(List<Card> cards)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var cardCommand = $"INSERT INTO Cards (Id, Name, Damage, PackageId) VALUES ('{card.Id}', '{card.Name}', {card.Damage}, '{card.PackageId}')";
                    ExecuteCommand(connection, cardCommand);
                }

                connection.Close();
            }
        }


        public static void AcquireCards(long userId, List<Card> cards)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var acquireCardCommand = $"UPDATE Cards SET UserId = {userId} WHERE Id = '{card.Id}'";
                    ExecuteCommand(connection, acquireCardCommand);
                }

                connection.Close();
            }
        }


        public static void ResetDatabase()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                ExecuteCommand(connection, "DROP TABLE IF EXISTS Cards");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS Packages");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS Users");
                CreateTables();

                connection.Close();
            }
        }


        public static void UpdateUserCoins(long userId, long newCoins)
        {
            try
            {
                using (var connection = new NpgsqlConnection(DataConnectionString))
                {
                    connection.Open();
                    var updateUserCoinsCommand = $"UPDATE Users SET Coins = {newCoins} WHERE Id = {userId}";
                    using (var command = new NpgsqlCommand(updateUserCoinsCommand, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user coins: {ex.Message}");
            }
        }


        private static void ExecuteCommand(NpgsqlConnection connection, string commandText)
        {
            using (var command = new NpgsqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
