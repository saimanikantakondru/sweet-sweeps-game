namespace SweetSweeps.Core.Contracts
{
    public interface IPasswordHasher
    {
        string Hash(string input);
    }
}