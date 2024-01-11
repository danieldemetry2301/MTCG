using FHTW.Swen1.Swamp.Database;
using MTCG_DEMETRY;

namespace FHTW.Swen1.Swamp
{
    public class TradingController
    {
        UserController userController = new();

        public string CreateTradingDeal(string token, TradingDeal deal)
        {
            var username = TokenHelper.ExtractUsernameFromToken(token);

            var user = userController.GetUserByUsername(username);
            if (user == null)
            {
                return "401 Access token is missing or invalid";
            }

            if (!DatabaseHelper.UserOwnsCard(user.Id, deal.CardToTrade) || DatabaseHelper.GetUserDeck(user.Id, deal.CardToTrade))
            {
                return "403 The deal contains a card that is not owned by the user or locked in the deck.";
            }

            if (DatabaseHelper.TradingDealExists(deal.Id))
            {
                return "409 A deal with this deal ID already exists";
            }

            DatabaseHelper.InsertTradingDeal(deal);
            return "201 Trading deal successfully created";
        }
    }
}
        



