using Microsoft.Data.Sqlite;
using System;

namespace FHTW.Swen1.Swamp.Database
{
    public class DatabaseHelper
    {
        private const string DatabaseFileName = "mctg.db";
        private const string DataConnectionString = "Data Source=mctg.db";

        public static void CreateDatabase()
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();
                connection.Close();
            }
        }

        public static void CreateTables()
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();

                ExecuteCommand(connection, @"
            CREATE TABLE IF NOT EXISTS Users (
                Username TEXT NOT NULL,
                Password TEXT NOT NULL
            )");

                ExecuteCommand(connection, @"
            CREATE TABLE IF NOT EXISTS Packages (
                Id STRING PRIMARY KEY
            )");

                ExecuteCommand(connection, @"
            CREATE TABLE IF NOT EXISTS Cards (
                Id STRING PRIMARY KEY ,
                Name STRING NOT NULL,
                Damage INTEGER NOT NULL,
                PackageId INTEGER,
                FOREIGN KEY (PackageId) REFERENCES Packages(Id)
            )");


                connection.Close();
            }
        }

        public static void InsertUser(User user)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();


                var insertUserCommand = $"INSERT INTO Users (Username, Password) VALUES ('{user.Username}', '{user.Password}')";

                ExecuteCommand(connection, insertUserCommand);

                /*foreach (var card in user.Cards)
                {
                    var insertCardCommand = $"INSERT INTO Card (UserId, CardId, Name, Damage) VALUES ('{user.Username}', '{card.Id}', '{card.Name}', {card.Damage})";
                    ExecuteCommand(connection, insertCardCommand);
                }*/

                connection.Close();
            }
        }

        public static void InsertPackage(Package package)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();

                var packageCommand = $"INSERT INTO Packages (Id) VALUES ('{package.Id}')";
                ExecuteCommand(connection, packageCommand);

                foreach (var card in package.Cards)
                {
                    var insertCommand = $"INSERT INTO Cards (Id, Name, Damage, PackageId) VALUES ('{card.Id}', '{card.Name}', {card.Damage}, '{package.Id}')";
                    ExecuteCommand(connection, insertCommand);
                }

                connection.Close();
            }
        }


        private static void ExecuteCommand(SqliteConnection connection, string commandText)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void ResetDatabase()
        {
            if (File.Exists(DatabaseFileName))
            {
                File.Delete(DatabaseFileName);
                CreateDatabase();
            }
        }

    }
}
