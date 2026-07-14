using System;
using SweetSweeps.Data;

namespace SweetSweeps.Menu.Contracts
{
    public interface IWorldSelectService
    {
        WorldDataSO CurrentWorld { get; }
        int CurrentIndex { get; }
        int TotalWorlds  { get; }

        event Action<WorldDataSO> OnWorldChanged;

        void SelectRandom();
    }
}