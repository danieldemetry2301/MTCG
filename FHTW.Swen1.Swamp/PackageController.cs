using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHTW.Swen1.Swamp
{
    using FHTW.Swen1.Swamp.Database;
    using Microsoft.Data.Sqlite;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PackageController
    {
        private static List<Package> globalPackages = new List<Package>();

        private UserController userController;
        private const string DataConnectionString = "Data Source=mctg.db";


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
            var newPackage = new Package { Id = packageId, Cards = cards };
            globalPackages.Add(newPackage);

            foreach (var card in cards)
            {
                card.PackageId = packageId;
            }

            DatabaseHelper.InsertPackage(newPackage);

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

            var acquiredPackage = globalPackages[0];

            user.Cards.AddRange(acquiredPackage.Cards);

            globalPackages.RemoveAt(0);
            return "200 A package has been successfully bought";
        }

        private static void ExecuteCommand(SqliteConnection connection, string commandText)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                }
            }
        }
    }

}


