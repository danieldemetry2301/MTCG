using FHTW.Swen1.Swamp.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FHTW.Swen1.Swamp
{
    public class Router
    {
        private UserController userController = new UserController();
        private PackageController packageController;
        private List<Package> packages = new List<Package>();
        private DatabaseHelper databaseHelper;

        public Router()
        {
            packageController = new PackageController(userController);
        }

        public void RouteRequest(HttpSvrEventArgs e)
        {
            var path = e.Path;

            if (path.StartsWith("/users") && e.Method == "POST")
            {
                HandleUserRegistration(e);
            }
            else if (path.StartsWith("/sessions") && e.Method == "POST")
            {
                HandleUserLogin(e);
            }
            else if (path.StartsWith("/packages") && e.Method == "POST")
            {
                HandleCreatePackage(e);
            }
            else if (path.StartsWith("/transactions/packages") && e.Method == "POST")
            {
                HandleAcquirePackage(e);
            }
            else if (path.StartsWith("/cards") && e.Method == "GET")
            {
                HandleShowCards(e);
            }
            else if (path.StartsWith("/deck") && e.Method == "GET")
            {
                HandleShowDeck(e);
            }
            else if (path.StartsWith("/deck") && e.Method == "PUT")
            {
                HandleUpdateDeck(e);
            }
            else
            {
                e.Reply(404, "Not Found");
            }
        }

        private void HandleUpdateDeck(HttpSvrEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value;
            var username = TokenHelper.ExtractUsernameFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                e.Reply(401, "Access token is missing or invalid");
                return;
            }

            var cardIds = JsonConvert.DeserializeObject<List<string>>(e.Payload);
            var result = userController.ConfigureUserDeck(username, cardIds);

            if (result.StartsWith("200"))
            {
                e.Reply(200, result);
            }
            else if (result.StartsWith("400"))
            {
                e.Reply(400, result);
            }
            else if (result.StartsWith("403"))
            {
                e.Reply(403, result);
            }
        }

        private void HandleAcquirePackage(HttpSvrEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value;
            var username = TokenHelper.ExtractUsernameFromToken(token);

            if (string.IsNullOrEmpty(username))
            {
                e.Reply(401, "Access token is missing or invalid");
                return;
            }

            var result = packageController.AcquirePackage(username);

            
            if (result.StartsWith("200"))
            {
                e.Reply(200, result);
            }
            else if (result.StartsWith("403"))
            {
                e.Reply(403, result);
            }
            else if (result.StartsWith("404"))
            {
                e.Reply(404, result);
            }
           
        }

        private void HandleShowDeck(HttpSvrEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value;
            var username = TokenHelper.ExtractUsernameFromToken(token);
            var cardsIndeck = userController.GetUserDeck(username);

            if (string.IsNullOrEmpty(username))
            {
                e.Reply(401, "Access token is missing or invalid");
                return;
            }

            var user = userController.GetUserByUsername(username);

            if (cardsIndeck.Any())
            {
                var cardsJson = JsonConvert.SerializeObject(cardsIndeck, Formatting.Indented);
                e.Reply(200, cardsJson);
            }
            else
            {
                e.Reply(204, "The request was fine, but the deck doesn't have any cards");
            }
        }

        private void HandleUserRegistration(HttpSvrEventArgs e)
        {
            var payload = e.Payload;
            var user = JsonConvert.DeserializeObject<User>(payload);

            var result = userController.RegisterUser(user);
            e.Reply(200, result);
        }

        private void HandleUserLogin(HttpSvrEventArgs e)
        {
            var payload = e.Payload;
            var loginData = JsonConvert.DeserializeObject<User>(payload);

            var result = userController.LoginUser(loginData.Username, loginData.Password);

            if (result.StartsWith("200"))
            {
                var token = $"{loginData.Username}-mtcgToken";
                e.Reply(200, token);
            }
            else
            {
                e.Reply(401, result);
            }
        }

        private void HandleCreatePackage(HttpSvrEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value;
            var username = TokenHelper.ExtractUsernameFromToken(token);

            if (string.IsNullOrEmpty(username))
            {
                e.Reply(401, "Access token is missing or invalid");
                return;
            }

            var payload = e.Payload;
            var receivedCards = DeserializeCardsFromRequest(payload);

            var result = packageController.CreatePackage(username, receivedCards);

            if (result.StartsWith("201"))
            {
                e.Reply(201, result);
            }
            else if (result.StartsWith("403"))
            {
                e.Reply(403, result);
            }
            else if (result.StartsWith("409"))
            {
                e.Reply(409, result);
            }
        }

        private void HandleShowCards(HttpSvrEventArgs e)
        {
            var token = e.Headers.FirstOrDefault(h => h.Name == "Authorization")?.Value;
            var username = TokenHelper.ExtractUsernameFromToken(token);
            var acquiredCards = userController.GetAllAcquiredCards(username);

            if (string.IsNullOrEmpty(username))
            {
                e.Reply(401, "Access token is missing or invalid");
                return;
            }

            var user = userController.GetUserByUsername(username);

            if (acquiredCards.Any())
            {
                var cardsJson = JsonConvert.SerializeObject(acquiredCards, Formatting.Indented);
                e.Reply(200, cardsJson);
            }
            else
            {
                e.Reply(404, "User not found");
            }
        }

        private List<Card> DeserializeCardsFromRequest(string payload)
        {
            try
            {
                var jArray = JArray.Parse(payload);

                var cards = jArray.Select(jToken =>
                {
                    var cardObj = jToken.ToObject<JObject>();
                    var id = cardObj.GetValue("Id").ToString();
                    var name = cardObj.GetValue("Name").ToString();
                    var damage = cardObj.GetValue("Damage").ToObject<double>();

                    return new Card { Id = id, Name = name, Damage = damage };
                }).ToList();

                return cards;
            }
            catch (Exception)
            {
                return new List<Card>();
            }
        }
    }
}
