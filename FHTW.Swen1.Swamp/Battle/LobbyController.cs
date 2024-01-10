using FHTW.Swen1.Swamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_DEMETRY.Battle
{

    using FHTW.Swen1.Swamp;
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
                        return FormatBattleLog(log, playerA, playerB);
                    }
                }

                return "Warten auf einen weiteren Spieler, um der Lobby beizutreten..\n";
            }

            private string FormatBattleLog(BattleLog log, User playerA, User playerB)
            {
                StringBuilder output = new StringBuilder();

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

                output.AppendLine($"Final Winner: {log.Result.Winner} after {log.Result.RoundsPlayed} rounds.");

                return output.ToString();
            }
        }
    }
}

