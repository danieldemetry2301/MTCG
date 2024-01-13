using FHTW.Swen1.Swamp.Database;
using MTCG_DEMETRY;

namespace FHTW.Swen1.Swamp
{
    public class TradingController
    {
        UserController userController = new();

        public string CreateTradingDeal(string username, TradingDeal deal)
        {
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string ExecuteTradingDeal(string username, string dealId, string offeredCardId)
        {
            var user = userController.GetUserByUsername(username);
            if (user == null)
            {
                return "401 Access token is missing or invalid";
            }

            var deal = DatabaseHelper.GetTradingDealById(dealId);
            if (deal == null)
            {
                return "404 The provided deal ID was not found.";
            }

            if (!DatabaseHelper.UserOwnsCard(user.Id, offeredCardId))
            {
                return "403 The offered card is not owned by the user, or the requirements are not met (Type, MinimumDamage), or the offered card is locked in the deck.";
            }

            if (DatabaseHelper.GetUserDeck(user.Id, offeredCardId))
            {
                return "403 The offered card is locked in the deck.";
            }

            var dealOwner = DatabaseHelper.GetUserByCardId(deal.CardToTrade);
            if (dealOwner == null || dealOwner.Id == user.Id)
            {
                return "403 Trading with self is not allowed or deal owner not found.";
            }

            if (!MeetsTradingCriteria(offeredCardId, deal))
            {
                return "403 The offered card does not meet the trading criteria.";
            }

            DatabaseHelper.ExchangeCards(user.Id, dealOwner.Id, offeredCardId, deal.CardToTrade);
            DatabaseHelper.DeleteTradingDeal(dealId);

            return "200 Trading deal successfully executed.";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool MeetsTradingCriteria(string offeredCardId, TradingDeal deal)
        {

            if (deal == null)
            {
                return false;
            }

            var offeredCard = DatabaseHelper.GetCardById(offeredCardId);

            if (offeredCard == null)
            {
                return false;
            }

            bool typeMatches = offeredCard.Type.ToLower().Equals(deal.Type.ToLower(), StringComparison.OrdinalIgnoreCase);

            bool damageMeetsRequirement = offeredCard.Damage >= deal.MinimumDamage;

            return typeMatches && damageMeetsRequirement;
        }
    }
}
        



