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
                var roundResult = new BattleRound
                {
                    PlayerACard = cardA.Name,
                    PlayerADamage = cardA.Damage,
                    PlayerBCard = cardB.Name,
                    PlayerBDamage = cardB.Damage
                };

                double damageA = CalculateEffectiveDamage(cardA, cardB);
                double damageB = CalculateEffectiveDamage(cardB, cardA);

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

                if (attacker.Name == "Goblin" && defender.Name == "Dragon") return 0;
                if (attacker.Name == "Wizzard" && defender.Name == "Ork") return 0;
                if (attacker.Name == "Knight" && defender.Name == "Spell" && defender.Name == "Water") return 0;
                if (attacker.Name == "Kraken" && defender.Name == "Spell") return attacker.Damage;
                if (attacker.Name == "FireElves" && defender.Name == "Dragon") return attacker.Damage;

                // Schadensberechnung basierend auf Elementtypen
                if (attacker.Name == "Spell" || defender.Name == "Spell")
                {
                    switch (attacker.Name)
                    {
                        case "Water":
                            damage *= defender.Name == "Fire" ? 2 : defender.Name == "Normal" ? 0.5 : 1;
                            break;
                        case "Fire":
                            damage *= defender.Name == "Normal" ? 2 : defender.Name == "Water" ? 0.5 : 1;
                            break;
                        case "Normal":
                            damage *= defender.Name == "Water" ? 2 : defender.Name == "Fire" ? 0.5 : 1;
                            break;
                    }
                }

                return damage;
            }
        }
    }
}

