using DeaneryManagementSystem.Abstractions;
using System.Security.Cryptography;

namespace DeaneryManagementSystem.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public string Hash(string str)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(str, salt, Iterations, _hashAlgorithName, KeySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string hashToCheck, string input)
        {
            var elements = hashToCheck.Split(Delimiter);
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(input, salt, Iterations, _hashAlgorithName, KeySize);
            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}
