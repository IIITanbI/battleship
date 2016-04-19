using ONXCmn.Logic;
using System;

namespace ONX.Cmn
{
    public delegate void OnEventHandler(Turn turn);

    public enum TurnResult
    {
        None,
        Miss,
        Damage,
        Kill,
        Win
    }

    [Serializable]
    public class Turn
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Turn()
        {
            this.Row = -1;
            this.Column = -1;
        }
        public Turn(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }
    }

    public interface IMyService
    {
        TurnResult PerformTurn(Turn turn);

        IMyService server { get; set; }
        GameConfig GetGameConfig(IMyService client);
        void StartGame();
        bool ReadyForBattle();
    }
}
