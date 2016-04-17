using ONXCmn.Logic;
using System;

namespace ONX.Cmn
{
	public interface IMyService
	{
        Turn YouTurn(Turn d);

        GameConfig GetGameConfig();
        void StartGame();
        bool ReadyForBattle();
    }
}
