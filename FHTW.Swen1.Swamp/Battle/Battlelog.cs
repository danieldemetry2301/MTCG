namespace FHTW.Swen1.Swamp
{
    public class BattleLog
    {
        public List<BattleRound> Rounds { get; set; } = new List<BattleRound>();
        public BattleResult Result { get; set; }
    }
}
