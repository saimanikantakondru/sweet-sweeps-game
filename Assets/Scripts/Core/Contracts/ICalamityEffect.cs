namespace SweetSweeps.Core.Contracts
{
    public interface ICalamityEffect
    {
        void Apply(CalamityLevel level, float intensity);
        void Remove();
    }
    
    public enum CalamityLevel
    {
        None,
        LevelOne,
        LevelTwo
    }
}