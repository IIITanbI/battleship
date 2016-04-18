using ONXCmn.Logic;
using System;

namespace ONX.Cmn
{
    public delegate void OnEventHandler(string message);

    public interface IMyService
	{
        Turn YouTurn(Turn d);

        GameConfig GetGameConfig();
        void StartGame();
        bool ReadyForBattle();

        event OnEventHandler Ev;
    }
}
