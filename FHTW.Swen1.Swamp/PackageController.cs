using System;
using System.Collections.Generic;
using System.Linq;
using FHTW.Swen1.Swamp.Database;
using Npgsql;
using NpgsqlTypes;

namespace FHTW.Swen1.Swamp
{
    public class PackageController
    {
        private static List<Package> globalPackages = new List<Package>();
        private UserController userController;
        private const string DataConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=mtcg";

        public PackageController(UserController userController)
        {
            this.userController = userController;
        }

        private bool IsAdminUser(string username)
        {
            return username.ToLower() == "admin";
        }

        private bool CardExistsInGlobalPackages(List<Card> cards)
        {
            return cards.Any(card =>
                globalPackages.Any(existingPackage =>
                    existingPackage.Cards.Any(existingCard =>
                        existingCard.Id == card.Id && existingCard.PackageId == card.PackageId)));
        }

        public string CreatePackage(string adminUsername, List<Card> cards)
        {
            if (!IsAdminUser(adminUsername))
            {
                return "403 Provided user is not 'admin'";
            }

            if (CardExistsInGlobalPackages(cards))
            {
                return "409 At least one card in the packages already exists";
            }

            var packageId = Guid.NewGuid().ToString();

            if (globalPackages.Any(existingPackage => existingPackage.Id == packageId))
            {
                return "409 Package with the same ID already exists";
            }

            var newPackage = new Package { Id = packageId, Cards = cards };
            globalPackages.Add(newPackage);

            foreach (var card in cards)
            {
                card.PackageId = packageId;
            }

            InsertPackageToPostgres(newPackage);

            return "201 Package and cards successfully created";
        }

        public string AcquirePackage(string username)
        {
            var user = userController.GetUserByUsername(username);

            if (user == null)
            {
                return "404 No card package available for buying";
            }

            if (user.Coins < 5)
            {
                return "403 Not enough money for buying a card package";
            }

            if (globalPackages.Count == 0)
            {
                return "404 No card package available for buying";
            }

            user.Coins -= 5;


            DatabaseHelper.UpdateUserCoins(user.Id, user.Coins);
            if (globalPackages.Count > 0)
            {
                var acquiredPackage = globalPackages[0];
                globalPackages.RemoveAt(0);

                DatabaseHelper.UpdateUserCoins(user.Id, (long)user.Coins);
                user.Cards.AddRange(acquiredPackage.Cards);
                DatabaseHelper.AcquireCards(user.Id, acquiredPackage.Cards, acquiredPackage.Id);

                return "200 A package has been successfully bought";
            }
            return "404 No card package available for buying";
        }






        private static void InsertPackageToPostgres(Package package)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var packageCommand = $"INSERT INTO Packages (Id) VALUES ('{package.Id}')";
                ExecuteCommand(connection, packageCommand);
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