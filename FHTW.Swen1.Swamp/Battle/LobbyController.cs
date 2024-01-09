using FHTW.Swen1.Swamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    waitingUsers.RemoveAt(0);
                    waitingUsers.RemoveAt(0);

                    BattleLog log = battleController.StartBattle(playerA, playerB);
                    return FormatBattleLog(log);
                }
            }

            return "Waiting for another player to join the lobby..\n";
        }

        private string FormatBattleLog(BattleLog log)
        {
            string output = "";
            foreach (var round in log.Rounds)
            {
                output += $"{round.PlayerACard} vs {round.PlayerBCard} - Winner: {round.Winner} ({round.Reason})\n";
            }
            output += $"Final Winner: {log.Result.Winner} after {log.Result.RoundsPlayed} rounds.";
            return output;
        }
    }
}

