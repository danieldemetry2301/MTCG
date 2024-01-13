using FHTW.Swen1.Swamp;
using FHTW.Swen1.Swamp.Database;
using FHTW.Swen1.Swamp.FHTW.Swen1.Swamp;
using MTCG_DEMETRY;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MTCG_Tests
{
    public class UnitTests
    {
        private UserController _userController = new UserController();
        private PackageController _packageController = new PackageController(new UserController());
        private BattleController _battleController = new BattleController();
        private TradingController _tradingController = new TradingController();
        [SetUp]
        public void Setup()
        {
            
        }
        
        //////////////////////////////////////////////// UserController Tests /////////////////////////////////////////////////////////////////

        [Test]
        [Order(1)]
        public void TestRegisterUser_ValidUser_ReturnsSuccessMessage()
        {
            DatabaseHelper.ResetDatabase();
            var newUser = new User { Username = "DanielTestUser", Password = "DanielTestPassword" };
            var result = _userController.RegisterUser(newUser);
            var userCheck = _userController.GetUserByUsername("DanielTestUser");
            Assert.AreEqual("201 User successfully registered", result);
            Assert.IsTrue(userCheck.Username == "DanielTestUser");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(2)]
        public void TestRegisterUser_ExistingUsername_ReturnsErrorMessage()
        {
            var user1 = new User { Username = "DanielExistingUser", Password = "DanielExistingUser" };
            _userController.RegisterUser(user1);

            var user2 = new User { Username = "DanielExistingUser", Password = "DanielExistingUser" };
            var result = _userController.RegisterUser(user2);
            var userCheck = _userController.GetUserByUsername("DanielExistingUser");
            Assert.AreEqual("409 User with the same username already registered", result);
            Assert.IsTrue(userCheck.Username == "DanielExistingUser");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(3)]
        public void TestLoginUser_ValidCredentials_ReturnsSuccess()
        {

            var newUser = new User { Username = "DanielTestUser", Password = "DanielTestPassword" };
            var result = _userController.RegisterUser(newUser);
            var loginResult = _userController.LoginUser("DanielTestUser", "DanielTestPassword");
            Assert.AreEqual("200 User login successful", loginResult);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(4)]
        public void TestLoginUser_InvalidCredentials_ReturnsErrorMessage()
        {
            var newUser = new User { Username = "DanielTestUser", Password = "DanielTestPassword" };
            _userController.RegisterUser(newUser);
            var loginResult = _userController.LoginUser("Invalid", "DanielTestPassword");
            Assert.AreEqual("401 Invalid username/password provided", loginResult);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(5)]
        public void TestUpdateUserProfile_ValidData_UpdatesSuccessfully()
        { 
            var user = new User { Username = "DanielTestUser", Password = "DanielTestPassword", Name = "Dan Dan" };
            _userController.RegisterUser(user);

            var updatedUser = new User { Name = "Daniel Dan", Bio = "Hallo!", Image = ";-)" };
            _userController.UpdateUserProfile("DanielTestUser", updatedUser);

            var userProfile = _userController.GetUserProfile("DanielTestUser");
            Assert.AreEqual("Daniel Dan", userProfile.Name);
            Assert.AreEqual("Hallo!", userProfile.Bio);
            Assert.AreEqual(";-)", userProfile.Image); 
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(6)]
        public void TestUpdateUserProfile_InvalidData_UpdateError()
        {
            var user = new User { Username = "DanielTestUser1", Password = "DanielTestPassword", Name = "Dan Dan" };
            _userController.RegisterUser(user);

            var updatedUser = new User { Name = "Daniel Dan", Bio = "Hallo!", Image = ";-)" };
            _userController.UpdateUserProfile("DanielTestUser1", updatedUser);

            var userProfile = _userController.GetUserProfile("DanielTestUser1");
            Assert.AreNotEqual("Daniel Error", userProfile.Name);
            Assert.AreNotEqual("Hallo!!", userProfile.Bio);
            Assert.AreNotEqual(";-))", userProfile.Image);
        }

        //////////////////////////////////////////////// PackageController Tests /////////////////////////////////////////////////////////////////

        [Test]
        [Order(7)]
        public void TestAcquirePackage_NoCardAvailable_Failure()
        {
            var user = new User { Username = "DanielUserRich", Password = "123", Coins = 10 };
            _userController.RegisterUser(user);

            var result = _packageController.AcquirePackage("DanielUserRich");
            var acquiredCards = _userController.GetUserAcquiredCards("DanielUserRich");
            Assert.AreEqual("404 No card package available for buying", result);
            Assert.IsTrue(acquiredCards.Count == 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(8)]
        public void TestCreatePackage_AsAdmin_Successfully()
        {
            var cards = new List<Card>
            {
                new Card { Id = "5678", Name = "DanielSpell", Damage = 25.0 },
                new Card { Id = "9101", Name = "ThomasSpell", Damage = 95.0 },
            };
            var result = _packageController.CreatePackage("admin", cards);
            var cardInDatabase = DatabaseHelper.GetCardById("5678");
            Assert.AreEqual("201 Package and cards successfully created", result);
            Assert.IsTrue(cardInDatabase.Name == "DanielSpell");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(9)]
        public void TestCreatePackage_AsNonAdmin_AccessDenied()
        {
            var cards = new List<Card>
            {
                new Card { Id = "5678", Name = "DanielSpell", Damage = 25.0 },
                new Card { Id = "9101", Name = "ThomasSpell", Damage = 95.0 },
            };
            var result = _packageController.CreatePackage("noAdmin", cards);
            var cardInDatabase = DatabaseHelper.GetCardById("5678");
            Assert.AreEqual("403 Provided user is not 'admin'", result);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(10)]
        public void TestAcquirePackage_InsufficientCoins_Failure()
        {
            var user = new User { Username = "DanielUserNoMoney", Password = "123", Coins = 0 };
            _userController.RegisterUser(user);

            var result = _packageController.AcquirePackage("DanielUserNoMoney");
            Assert.AreEqual("403 Not enough money for buying a card package", result);
            var acquiredCards = _userController.GetUserAcquiredCards("DanielUserNoMoney");
            Assert.IsTrue(acquiredCards.Count == 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(11)]
        public void TestAcquirePackage_Sucessfully()
        {
            var cards = new List<Card>
            {
                new Card { Id = "5678", Name = "DanielSpell", Damage = 25.0 },
                new Card { Id = "9101", Name = "ThomasSpell", Damage = 95.0 },
            };
            _packageController.CreatePackage("admin", cards);
            var user = new User { Username = "DanielUserRich", Password = "123", Coins = 10 };
            _userController.RegisterUser(user);

            var result = _packageController.AcquirePackage("DanielUserRich");
            var acquiredCards = _userController.GetUserAcquiredCards("DanielUserRich");
            Assert.AreEqual("200 A package has been successfully bought", result);
            Assert.IsTrue(acquiredCards.Count == 2);

        }

        //////////////////////////////////////////////// BattleController Tests /////////////////////////////////////////////////////////////////

        [Test]
        [Order(12)]
        public void TestStartBattle_Player1Wins()
        {
            DatabaseHelper.ResetDatabase();
            var package1 = new List<Card>
            {
                new Card { Id = "5678", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "9101", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "5679", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "9102", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "9132", Name = "Thomas", Damage = 100.0 }
            };
            _packageController.CreatePackage("admin", package1);
            var player1 = new User { Username = "Player1", Password = "123"};
            var player2 = new User { Username = "Player2", Password = "123" };
            _userController.RegisterUser(player1);
            _userController.RegisterUser(player2);
            _packageController.AcquirePackage("Player1");
            var package2 = new List<Card>
            {
                new Card { Id = "0000", Name = "Daniel", Damage = 10.0 },
                new Card { Id = "1111", Name = "Thomas", Damage = 10.0 },
                new Card { Id = "2222", Name = "Daniel", Damage = 10.0 },
                new Card { Id = "3333", Name = "Thomas", Damage = 10.0 },
                new Card { Id = "4444", Name = "Thomas", Damage = 5.0 }
            };
            _packageController.CreatePackage("admin", package2);
            _packageController.AcquirePackage("Player2");
            _userController.ConfigureUserDeck("Player1", new List<string> {"5678","9101","5679","9102"});
            _userController.ConfigureUserDeck("Player2", new List<string> { "0000", "1111", "2222", "3333"});

            var battleLog = _battleController.StartBattle(player1, player2);
            Assert.AreEqual("Player1", battleLog.Result.Winner);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(13)]
        public void TestStartBattle_Draw()
        {
            DatabaseHelper.ResetDatabase();
            var package1 = new List<Card>
            {
                new Card { Id = "5678", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "9101", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "5679", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "9102", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "9132", Name = "Thomas", Damage = 100.0 }
            };
            _packageController.CreatePackage("admin", package1);
            var player1 = new User { Username = "Player1", Password = "123" };
            var player2 = new User { Username = "Player2", Password = "123" };
            _userController.RegisterUser(player1);
            _userController.RegisterUser(player2);
            _packageController.AcquirePackage("Player1");
            var package2 = new List<Card>
            {
                new Card { Id = "0000", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "1111", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "2222", Name = "Daniel", Damage = 100.0 },
                new Card { Id = "3333", Name = "Thomas", Damage = 100.0 },
                new Card { Id = "4444", Name = "Thomas", Damage = 100.0 }
            };
            _packageController.CreatePackage("admin", package2);
            _packageController.AcquirePackage("Player2");
            _userController.ConfigureUserDeck("Player1", new List<string> { "5678", "9101", "5679", "9102" });
            _userController.ConfigureUserDeck("Player2", new List<string> { "0000", "1111", "2222", "3333" });

            var battleLog = _battleController.StartBattle(player1, player2);
            Assert.IsTrue(battleLog.Result.RoundsPlayed >= 100);
            Assert.IsTrue(player1.Deck.Count > 0 && player2.Deck.Count > 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(14)]
        public void TestStartBattle_CalculateEffectiveDamage_Player2Wins()
        {
            DatabaseHelper.ResetDatabase();
            var package1 = new List<Card>
            {
                new Card { Id = "5678", Name = "Goblin", Damage = 200.0 },
                new Card { Id = "9101", Name = "Goblin", Damage = 200.0 },
                new Card { Id = "5679", Name = "Goblin", Damage = 200.0 },
                new Card { Id = "9102", Name = "Goblin", Damage = 200.0 },
                new Card { Id = "9132", Name = "Goblin", Damage = 200.0 }
            };
            _packageController.CreatePackage("admin", package1);
            var player1 = new User { Username = "Player1", Password = "123" };
            var player2 = new User { Username = "Player2", Password = "123" };
            _userController.RegisterUser(player1);
            _userController.RegisterUser(player2);
            _packageController.AcquirePackage("Player1");
            var package2 = new List<Card>
            {
                new Card { Id = "0000", Name = "Dragon", Damage = 20.0 },
                new Card { Id = "1111", Name = "Dragon", Damage = 20.0 },
                new Card { Id = "2222", Name = "Dragon", Damage = 20.0 },
                new Card { Id = "3333", Name = "Dragon", Damage = 20.0 },
                new Card { Id = "4444", Name = "Dragon", Damage = 20.0 }
            };
            _packageController.CreatePackage("admin", package2);
            _packageController.AcquirePackage("Player2");
            _userController.ConfigureUserDeck("Player1", new List<string> { "5678","9101","5679","9102"});
            _userController.ConfigureUserDeck("Player2", new List<string> { "0000","1111","2222","3333"});

            var battleLog = _battleController.StartBattle(player1, player2);
            Assert.AreEqual("Player2", battleLog.Result.Winner);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(15)]
        public void TestStartBattle_CalculateEffectiveDamage_Player1Wins()
        {
            DatabaseHelper.ResetDatabase();
            var package1 = new List<Card>
            {
                new Card { Id = "5678", Name = "WaterSpell", Damage = 20.0 },
                new Card { Id = "9101", Name = "WaterSpell", Damage = 20.0 },
                new Card { Id = "5679", Name = "WaterSpell", Damage = 20.0 },
                new Card { Id = "9102", Name = "WaterSpell", Damage = 20.0 },
                new Card { Id = "9132", Name = "WaterSpell", Damage = 20.0 }
            };
            _packageController.CreatePackage("admin", package1);
            var player1 = new User { Username = "Player1", Password = "123" };
            var player2 = new User { Username = "Player2", Password = "123" };
            _userController.RegisterUser(player1);
            _userController.RegisterUser(player2);
            _packageController.AcquirePackage("Player1");
            var package2 = new List<Card>
            {
                new Card { Id = "0000", Name = "FireSpell", Damage = 30.0 },
                new Card { Id = "1111", Name = "FireSpell", Damage = 30.0 },
                new Card { Id = "2222", Name = "FireSpell", Damage = 30.0 },
                new Card { Id = "3333", Name = "FireSpell", Damage = 30.0 },
                new Card { Id = "4444", Name = "FireSpell", Damage = 30.0 }
            };
            _packageController.CreatePackage("admin", package2);
            _packageController.AcquirePackage("Player2");
            _userController.ConfigureUserDeck("Player1", new List<string> { "5678", "9101", "5679", "9102" });
            _userController.ConfigureUserDeck("Player2", new List<string> { "0000", "1111", "2222", "3333" });

            var battleLog = _battleController.StartBattle(player1, player2);
            Assert.AreEqual("Player1", battleLog.Result.Winner);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(16)]
        public void TestStartBattle_NoCardsInDeck()
        {
            DatabaseHelper.ResetDatabase();
            
            var player1 = new User { Username = "Player1", Password = "123" };
            var player2 = new User { Username = "Player2", Password = "123" };
            _userController.RegisterUser(player1);
            _userController.RegisterUser(player2);

            _userController.ConfigureUserDeck("Player1", new List<string> { });
            _userController.ConfigureUserDeck("Player2", new List<string> { });

            var battleLog = _battleController.StartBattle(player1, player2);
            Assert.IsTrue(player1.Deck.Count < 4);
            Assert.IsTrue(player2.Deck.Count < 4);
        }

        //////////////////////////////////////////////// TradingController Tests /////////////////////////////////////////////////////////////////

        [Test]
        [Order(17)]
        public void TestCreateTradeOffer_Sucess()
        {
            DatabaseHelper.ResetDatabase();
            var user = new User { Username = "Trader", Password = "12345" };
            _userController.RegisterUser(user);
            var package = new List<Card>
            {
                new Card { Id = "0000", Name = "FireSpell", Damage = 35.0 },
                new Card { Id = "1111", Name = "WaterSpell", Damage = 40.0 },
                new Card { Id = "2222", Name = "Monster", Damage = 45.0 },
                new Card { Id = "3333", Name = "Goblin", Damage = 50.0 },
                new Card { Id = "4444", Name = "Dragon", Damage = 60.0 }
            };
            _packageController.CreatePackage("admin", package);
            _packageController.AcquirePackage("Trader");
            var tradeOffer = new TradingDeal { Id = "1111", CardToTrade = "0000", MinimumDamage = 40.0, Type = "Monster" };
            var result = _tradingController.CreateTradingDeal("Trader",tradeOffer);
            var tradingDeals = DatabaseHelper.GetTradingDeals();
            Assert.AreEqual("201 Trading deal successfully created", result);
            Assert.IsTrue(tradingDeals.Count == 1);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(18)]
        public void TestCreateTradeOffer_Error_CardDoesNotExist()
        {
            DatabaseHelper.ResetDatabase();
            var user = new User { Username = "Trader", Password = "12345" };
            _userController.RegisterUser(user);
            var package = new List<Card>
            {
                new Card { Id = "0000", Name = "FireSpell", Damage = 35.0 },
                new Card { Id = "1111", Name = "WaterSpell", Damage = 40.0 },
                new Card { Id = "2222", Name = "Monster", Damage = 45.0 },
                new Card { Id = "3333", Name = "Goblin", Damage = 50.0 },
                new Card { Id = "4444", Name = "Dragon", Damage = 60.0 }
            };
            _packageController.CreatePackage("admin", package);
            _packageController.AcquirePackage("Trader");
            var tradeOffer = new TradingDeal { Id = "1111", CardToTrade = "5555", MinimumDamage = 40.0, Type = "Monster" };
            var result = _tradingController.CreateTradingDeal("Trader", tradeOffer);
            var tradingDeals = DatabaseHelper.GetTradingDeals();
            Assert.AreEqual("403 The deal contains a card that is not owned by the user or locked in the deck.", result);
            Assert.IsTrue(tradingDeals.Count == 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(19)]
        public void TestAcceptTrading_Sucess()
        {
            DatabaseHelper.ResetDatabase();
            var user = new User { Username = "Trader", Password = "12345" };
            var user2 = new User { Username = "Trader2", Password = "12345" };
            _userController.RegisterUser(user);
            _userController.RegisterUser(user2);
            var package = new List<Card>
            {
                new Card { Id = "0000", Name = "CardFromUser1", Damage = 35.0 },
            };
            _packageController.CreatePackage("admin", package);
            _packageController.AcquirePackage("Trader");
            var package2 = new List<Card>
            {
                new Card { Id = "1111", Name = "CardFromUser2", Damage = 45.0 },
            };
            _packageController.CreatePackage("admin", package2);
            _packageController.AcquirePackage("Trader2");
            var tradeOffer = new TradingDeal { Id = "1111", CardToTrade = "0000", MinimumDamage = 40.0, Type = "Monster" };
            _tradingController.CreateTradingDeal("Trader", tradeOffer);
            var result = _tradingController.ExecuteTradingDeal("Trader2", "1111", "1111");
            var userOwnsCard = DatabaseHelper.UserOwnsCard(1, "1111");
            var tradingDeals = DatabaseHelper.GetTradingDeals();
            Assert.AreEqual("200 Trading deal successfully executed.", result);
            Assert.IsTrue(userOwnsCard);
            Assert.IsTrue(tradingDeals.Count == 0);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(20)]
        public void TestAcceptTrading_TradingWithYourself_Error()
        {
            DatabaseHelper.ResetDatabase();
            var user = new User { Username = "Trader", Password = "12345" };
            _userController.RegisterUser(user);
            var package = new List<Card>
            {
                new Card { Id = "0000", Name = "CardFromUser1", Damage = 35.0 },
                new Card { Id = "1111", Name = "CardFromUser1", Damage = 45.0 },
            };
            _packageController.CreatePackage("admin", package);
            _packageController.AcquirePackage("Trader");

            var tradeOffer = new TradingDeal { Id = "1111", CardToTrade = "0000", MinimumDamage = 40.0, Type = "Monster" };
            _tradingController.CreateTradingDeal("Trader", tradeOffer);
            var result = _tradingController.ExecuteTradingDeal("Trader", "1111", "1111");
            var tradingDeals = DatabaseHelper.GetTradingDeals();
            Assert.AreEqual("403 Trading with self is not allowed or deal owner not found.", result);
            Assert.IsTrue(tradingDeals.Count == 1);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Test]
        [Order(20)]
        public void TestAcceptTrading_InvalidUser_Error()
        {
            DatabaseHelper.ResetDatabase();
            var user = new User { Username = "Trader", Password = "12345" };
            _userController.RegisterUser(user);
            var package = new List<Card>
            {
                new Card { Id = "0000", Name = "CardFromUser1", Damage = 35.0 },
                new Card { Id = "1111", Name = "CardFromUser1", Damage = 45.0 },
            };
            _packageController.CreatePackage("admin", package);
            _packageController.AcquirePackage("Trader");

            var tradeOffer = new TradingDeal { Id = "1111", CardToTrade = "0000", MinimumDamage = 40.0, Type = "Monster" };
            _tradingController.CreateTradingDeal("Trader", tradeOffer);
            var result = _tradingController.ExecuteTradingDeal("UnkownUser", "1111", "1111");
            var tradingDeals = DatabaseHelper.GetTradingDeals();
            Assert.AreEqual("401 Access token is missing or invalid", result);
            Assert.IsTrue(tradingDeals.Count == 1);
            DatabaseHelper.ResetDatabase();
        }
    }
}