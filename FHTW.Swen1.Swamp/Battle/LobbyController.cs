using FHTW.Swen1.Swamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_DEMETRY.Battle
{

    using FHTW.Swen1.Swamp;
    using FHTW.Swen1.Swamp.Database;
    using FHTW.Swen1.Swamp.FHTW.Swen1.Swamp;
    using System.Collections.Generic;
    using System.Text;

    namespace MTCG_DEMETRY.Battle
    {
        public class LobbyController
        {
            private static List<User> waitingUsers = new List<User>();
            private BattleController battleController = new BattleController();

            public string JoinBattleLobby(User user)
            {
                lock (waitingUsers)
                {
                    if (!waitingUsers.Contains(user))
                    {
                        waitingUsers.Add(user);
                    }

                    if (waitingUsers.Count >= 2)
                    {
                        User playerA = waitingUsers[0];
                        User playerB = waitingUsers[1];
                        waitingUsers.RemoveRange(0, 2);

                        BattleLog log = battleController.StartBattle(playerA, playerB);
                        UpdateEloAndStats(playerA, playerB, log);
                        return FormatBattleLog(log, playerA, playerB);
                    }
                }

                return "Warten auf einen weiteren Spieler, um der Lobby beizutreten..\n";
            }

            private string FormatBattleLog(BattleLog log, User playerA, User playerB)
            {
                StringBuilder output = new StringBuilder();
                int winsPlayerA = 0;
                int winsPlayerB = 0;
                int lossesPlayerA = 0;
                int lossesPlayerB = 0;

                foreach (var round in log.Rounds)
                {
                    output.AppendLine($"/////////////////////////////////");
                    output.AppendLine($"ROUND {round.RoundNumber}");
                    output.AppendLine($"{round.PlayerACard} (Damage: {round.PlayerADamage}) vs {round.PlayerBCard} (Damage: {round.PlayerBDamage})");
                    output.AppendLine("FIGHT!");

                    if (round.Winner != null)
                    {
                        output.AppendLine($"The winner of round {round.RoundNumber} is {round.Winner}");
                    }
                    else
                    {
                        output.AppendLine("It's a draw!");
                    }

                    output.AppendLine($"{playerA.Username} has now {round.PlayerACardCount} cards, {playerB.Username} has now {round.PlayerBCardCount} cards");
                    output.AppendLine($"/////////////////////////////////");
                }

                if (log.Result.RoundsPlayed == 100 && playerA.Deck.Count > 0 && playerB.Deck.Count > 0)
                {
                    output.AppendLine("Nobody won after 100 Rounds => Its a draw!");
                    winsPlayerA = log.Rounds.Count(r => r.Winner == playerA.Username);
                    lossesPlayerA = log.Rounds.Count(r => r.Loser == playerA.Username);
                    int draws = 100 - (winsPlayerA + lossesPlayerA);
                    output.AppendLine($"Player {playerA.Username} won {winsPlayerA} times and lost {lossesPlayerA} times and had {draws} draws in this game.");

                    winsPlayerB = log.Rounds.Count(r => r.Winner == playerB.Username);
                    lossesPlayerB = log.Rounds.Count(r => r.Loser == playerB.Username);
                    output.AppendLine($"Player {playerB.Username} won {winsPlayerB} times and lost {lossesPlayerB} times and had {draws} draws in this game.");

                }
                else
                {
                    output.AppendLine($"Final Winner: {log.Result.Winner} after {log.Result.RoundsPlayed} rounds.");
                }

                return output.ToString();
            }


            private void UpdateEloAndStats(User playerA, User playerB, BattleLog log)
            {

                if (playerA.Deck.Count == 0 || playerB.Deck.Count == 0)
                {
                    int eloChangeWinner = 3;
                    int eloChangeLoser = -5;

                    User winner = playerA.Deck.Count == 0 ? playerB : playerA;
                    User loser = playerA.Deck.Count == 0 ? playerA : playerB;

                    winner.Elo += eloChangeWinner;
                    loser.Elo += eloChangeLoser;

                    winner.Wins += 1;
                    loser.Losses += 1;

                    DatabaseHelper.UpdateUserEloAndStats(winner);
                    DatabaseHelper.UpdateUserEloAndStats(loser);
                }
            }
        }
    }
}

