using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remit.Service
{
    public interface ISecurityService
    {
        string GenerateHashWithSalt(string pwd, string salt);
    }
    class SecurityService : ISecurityService
    {
        /// <summary>
        /// Computes a salted hash of the password and salt provided and returns as a base64 encoded string.
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use in the hash.</param>
        public string GenerateHashWithSalt(string password, string salt)
        {
            // merge password and salt together
            string sHashWithSalt = password + salt.ToUpper();
            // convert this merged value to a byte array
            byte[] saltedHashBytes = Encoding.UTF8.GetBytes(sHashWithSalt);
            // use hash algorithm to compute the hash
            System.Security.Cryptography.HashAlgorithm algorithm = new System.Security.Cryptography.SHA256Managed();
            // convert merged bytes to a hash as byte array
            byte[] hash = algorithm.ComputeHash(saltedHashBytes);
            // return the has as a base 64 encoded string
            return Convert.ToBase64String(hash);
        }
    }
}