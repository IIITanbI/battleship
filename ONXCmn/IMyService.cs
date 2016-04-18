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
        Win
    }

    [Serializable]
    public class Turn
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public TurnResult PreviousTurnResult { get; set; } = TurnResult.Miss;

        public Turn()
        {

        }
        public Turn(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }
        public Turn(int row, int column, TurnResult previousTurnResult)
            : this(row, column)
        {
            this.PreviousTurnResult = previousTurnResult;
        }
    }

    public interface IMyService
    {
        void YouTurn(Turn turn);

        IMyService server { get; set; }
        GameConfig GetGameConfig();
        void StartGame();
        bool ReadyForBattle();

        event OnEventHandler Ev;
    }
}
