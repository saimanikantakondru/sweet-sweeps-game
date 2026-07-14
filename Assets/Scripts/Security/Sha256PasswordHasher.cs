using System.Security.Cryptography;
using System.Text;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Security
{
    public class Sha256PasswordHasher : IPasswordHasher
    {
        public string Hash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();

                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
            
                return sb.ToString();
            }
        }
    }
}