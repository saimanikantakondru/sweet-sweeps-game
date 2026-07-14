namespace SweetSweeps.Core.Contracts
{
    public interface ILevelHistoryService
    {
        int GetLastPlayedIndex(string worldId);
        void SetLastPlayedIndex(string worldId, int index);
        void Clear();
    }
}
