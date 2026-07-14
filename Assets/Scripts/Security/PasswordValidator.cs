using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Security
{
    public class PasswordValidator : IPasswordValidator
    {
        private readonly IPasswordHasher _hasher;
        private readonly string _expectedHash;

        public PasswordValidator(IPasswordHasher hasher, string expectedHash)
        {
            _hasher = hasher;
            _expectedHash = expectedHash;
        }

        public bool Validate(string input)
        {
            return _hasher.Hash(input) == _expectedHash;
        }
    }
}