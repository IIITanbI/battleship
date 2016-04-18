using ONXCmn.Logic;
using System;

namespace ONX.Cmn
{
	public interface IMyService
	{
        void YouTurn(Turn d);

        GameConfig GetGameConfig(IMyService client);
        void StartGame();
        bool ReadyForBattle();
    }
}
