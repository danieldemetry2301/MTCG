using FHTW.Swen1.Swamp;
namespace FHTW.Swen1.Swamp
{
    public class BattleController
    {
        private UserController userController;
            public BattleLog StartBattle(User playerA, User playerB)
            {
               
            playerA.Deck = userController.GetUserDeck(playerA.Username);
            playerB.Deck = userController.GetUserDeck(playerB.Username);

            if (playerA.Deck.Count == 0 || playerB.Deck.Count == 0)
                {
                    throw new InvalidOperationException("One or both players have an empty deck.");
                }

                var battleLog = new BattleLog();
                int roundCounter = 0;

                while (playerA.Deck.Count > 0 && playerB.Deck.Count > 0 && roundCounter < 100)
                {
                    var cardA = SelectRandomCard(playerA.Deck);
                    var cardB = SelectRandomCard(playerB.Deck);

                    var roundResult = DetermineRoundResult(cardA, cardB, playerA.Username, playerB.Username);
                    battleLog.Rounds.Add(roundResult);

                    if (roundResult.Winner == playerA.Username)
                    {
                        TransferCard(playerB, playerA, cardB);
                    }
                    else if (roundResult.Winner == playerB.Username)
                    {
                        TransferCard(playerA, playerB, cardA);
                    }

                    roundCounter++;
                }

                battleLog.Result = new BattleResult
                {
                    Winner = playerA.Deck.Count > playerB.Deck.Count ? playerA.Username : playerB.Username,
                    RoundsPlayed = roundCounter
                };

                return battleLog;
            }
        



        private Card SelectRandomCard(List<Card> deck)
        {
            var random = new Random();
            int randomIndex = random.Next(deck.Count);
            return deck[randomIndex];
        }

        private void TransferCard(User fromUser, User toUser, Card card)
        {
            fromUser.Deck.Remove(card);
            toUser.Deck.Add(card);
        }

        private BattleRound DetermineRoundResult(Card cardA, Card cardB, string playerAName, string playerBName)
        {
            string winner = null;
            string reason = "";

            double damageA = CalculateEffectiveDamage(cardA, cardB);
            double damageB = CalculateEffectiveDamage(cardB, cardA);

            if (damageA > damageB)
            {
                winner = playerAName;
                reason = $"{cardA.Name} defeats {cardB.Name}";
            }
            else if (damageB > damageA)
            {
                winner = playerBName;
                reason = $"{cardB.Name} defeats {cardA.Name}";
            }
            else
            {
                reason = "Draw";
            }

            return new BattleRound
            {
                PlayerACard = cardA.Name,
                PlayerBCard = cardB.Name,
                Winner = winner,
                Reason = reason
            };
        }

        private BattleResult DetermineBattleResult(User playerA, User playerB, int roundsPlayed)
        {
            var result = new BattleResult
            {
                Winner = playerA.Deck.Count > playerB.Deck.Count ? playerA.Username : playerB.Username,
                RoundsPlayed = roundsPlayed
            };

            return result;
        }

        private double CalculateEffectiveDamage(Card attacker, Card defender)
        {
            double damage = attacker.Damage;

            if (attacker.Type == "Spell" || defender.Type == "Spell")
            {
                switch (attacker.Element)
                {
                    case "Water":
                        damage *= defender.Element == "Fire" ? 2 : defender.Element == "Normal" ? 0.5 : 1;
                        break;
                    case "Fire":
                        damage *= defender.Element == "Normal" ? 2 : defender.Element == "Water" ? 0.5 : 1;
                        break;
                    case "Normal":
                        damage *= defender.Element == "Water" ? 2 : defender.Element == "Fire" ? 0.5 : 1;
                        break;
                }
            }

            if (attacker.Name == "Goblin" && defender.Name == "Dragon") damage = 0;
            if (attacker.Name == "Wizzard" && defender.Name == "Ork") damage = 0;
            if (attacker.Name == "Knight" && defender.Type == "Spell" && defender.Element == "Water") damage = 0;
            if (attacker.Name == "Kraken" && defender.Type == "Spell") return attacker.Damage;
            if (attacker.Name == "FireElves" && defender.Name == "Dragon") return attacker.Damage;

            return damage;
        }
    }
}

