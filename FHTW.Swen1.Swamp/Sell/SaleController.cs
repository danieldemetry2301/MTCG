using FHTW.Swen1.Swamp;
using FHTW.Swen1.Swamp.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_DEMETRY.Sell
{
    public class SaleController
    {
        UserController userController = new();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string CreateOffer(string username, SellOffer sellOffer)
        {
            var user = userController.GetUserByUsername(username);
            if (user == null)
            {
                return "401 Access token is missing or invalid";
            }
            var card = DatabaseHelper.GetCardById(sellOffer.CardId);
           
            if (!DatabaseHelper.UserOwnsCard(user.Id, card.Id) || DatabaseHelper.GetUserDeck(user.Id, card.Id))
            {
                return "403 The offer contains a card that is not owned by the user or locked in the deck.";
            }

            if (DatabaseHelper.SellOfferExists(sellOffer.Id))
            {
                return "409 A offer with this offer ID already exists";
            }

            DatabaseHelper.InsertSaleOffer(sellOffer, card.Id);
            return "201 Sale offer successfully created";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string ExecuteSellDeal(string username, string offerId)
        {
            var user = userController.GetUserByUsername(username);
            if (user == null)
            {
                return "401 Access token is missing or invalid";
            }

            DatabaseHelper.AddCoinsToUser(user.Id, 20);

            user = userController.GetUserByUsername(username);

            var offer = DatabaseHelper.GetSellOfferById(offerId);
            if (offer == null)
            {
                return "404 The provided offer ID was not found.";
            }

            if (user.Coins < offer.Price)
            {
                return "403 User does not have enough coins to complete the transaction.";
            }

            var cardOwner = DatabaseHelper.GetUserByCardId(offer.CardId);
            if (cardOwner == null)
            {
                return "404 Owner of the card not found.";
            }

            string dummyCardId = "";

            DatabaseHelper.ExchangeCards(cardOwner.Id, user.Id, offer.CardId, dummyCardId);
            DatabaseHelper.RemoveCoinsFromUser(user.Id, offer.Price);
            DatabaseHelper.AddCoinsToUser(cardOwner.Id, offer.Price);
            DatabaseHelper.DeleteSellOffer(offerId);

            return "200 Transaction successfully completed.";
        }


    }
}
