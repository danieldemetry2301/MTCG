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
    }
}
