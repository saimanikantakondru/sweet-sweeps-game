namespace SweetSweeps.Core.Contracts
{
    public interface IPasswordValidator
    {
        bool Validate(string input);
    }
}