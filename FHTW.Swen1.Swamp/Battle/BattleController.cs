using FHTW.Swen1.Swamp;
namespace FHTW.Swen1.Swamp
{
    using System;
    using System.Collections.Generic;

    namespace FHTW.Swen1.Swamp
    {
        public class BattleController
        {
            private UserController userController = new UserController();
            
            public BattleLog StartBattle(User playerA, User playerB)
            {
                playerA.Deck = userController.GetUserDeck(playerA.Username);
                playerB.Deck = userController.GetUserDeck(playerB.Username);

                var battleLog = new BattleLog();
                int roundCounter = 0;

                while (playerA.Deck.Count > 0 && playerB.Deck.Count > 0 && roundCounter < 100)
                {
                    roundCounter++;
                    var cardA = SelectRandomCard(playerA.Deck);
                    var cardB = SelectRandomCard(playerB.Deck);

                    var roundResult = DetermineRoundResult(cardA, cardB, playerA.Username, playerB.Username);
                    roundResult.RoundNumber = roundCounter;
                    battleLog.Rounds.Add(roundResult);

                    if (roundResult.Winner == playerA.Username)
                    {
                        playerB.Deck.Remove(cardB);
                        playerA.Deck.Add(cardB);
                    }
                    else if (roundResult.Winner == playerB.Username)
                    {
                        playerA.Deck.Remove(cardA);
                        playerB.Deck.Add(cardA);
                    }

                    roundResult.PlayerACardCount = playerA.Deck.Count;
                    roundResult.PlayerBCardCount = playerB.Deck.Count;
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
                double damageA = CalculateEffectiveDamage(cardA, cardB);
                double damageB = CalculateEffectiveDamage(cardB, cardA);

                var roundResult = new BattleRound
                {
                    PlayerACard = cardA.Name,
                    PlayerADamage = damageA, 
                    PlayerBCard = cardB.Name,
                    PlayerBDamage = damageB,
                    RoundNumber = 0 
                };

                if (damageA > damageB)
                {
                    roundResult.Winner = playerAName;
                    roundResult.Loser = playerBName;
                    roundResult.Reason = $"{cardA.Name} defeats {cardB.Name}";
                }
                else if (damageB > damageA)
                {
                    roundResult.Winner = playerBName;
                    roundResult.Loser = playerAName;
                    roundResult.Reason = $"{cardB.Name} defeats {cardA.Name}";
                }
                else
                {
                    roundResult.Reason = "Draw";
                }

                return roundResult;
            }


            private double CalculateEffectiveDamage(Card attacker, Card defender)
            {
                double damage = attacker.Damage;

                if (attacker.Name.Contains("Goblin") && defender.Name.Contains("Dragon")) return damage*0;
                if (attacker.Name.Contains("Wizzard") && defender.Name.Contains("Ork")) return damage*0;
                if (attacker.Name.Contains("Knight") && defender.Name == "WaterSpell") return damage*0;
                if (attacker.Name.Contains("Spell")  && defender.Name.Contains("Kraken")) return damage*0;
                if (attacker.Name.Contains("Dragon") && defender.Name.Contains("FireElves")) return damage*0;
                if (attacker.Name == ("WaterSpell") && defender.Name == ("FireSpell")) return damage*2;
                if (attacker.Name == ("FireSpell") && defender.Name == ("WaterSpell")) return damage*0.5;
                if (attacker.Name == ("FireSpell") && defender.Name == ("RegularSpell")) return damage*0;
                if (attacker.Name == ("RegularSpell") && !defender.Name.Contains("Water") || !defender.Name.Contains("Fire") || !defender.Name.Contains("Regular")) return damage;

                return damage;
            }
        }
    }
}

