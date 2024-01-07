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
                        Coins INTEGER
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Packages (
                        Id UUID PRIMARY KEY
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Cards (
                        Id UUID PRIMARY KEY,
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

                var insertUserCommand = $"INSERT INTO Users (Username, Password) VALUES ('{user.Username}', '{user.Password}')";
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

        public static void AcquireCards(long userId, List<Card> cards, string packageId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var acquireCardCommand = $"INSERT INTO Cards (Id, Name, Damage, UserId, PackageId) VALUES ('{card.Id}', '{card.Name}', {card.Damage}, {userId}, '{packageId}')";
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
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var updateUserCoinsCommand = $"UPDATE Users SET Coins = {newCoins} WHERE Id = {userId}";
                ExecuteCommand(connection, updateUserCoinsCommand);

                connection.Close();
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
