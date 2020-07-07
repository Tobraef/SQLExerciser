using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace SQLExerciser.Models
{
    public interface IHasher
    {
        string HashText(string toHash);

        bool CompareAgainst(string hashedText, string input);
    }


    public class Hasher : IHasher
    {
        const int saltSize = 16;
        const int hashSize = 20;

        byte[] Salt
        {
            get
            {
                using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
                {
                    byte[] salt = new byte[saltSize];
                    provider.GetBytes(salt);
                    return salt;
                }
            }
        }

        byte[] Hash(string toHash, byte[] salt, int iterations = 1000)
        {
            using (Rfc2898DeriveBytes hashGenerator = new Rfc2898DeriveBytes(toHash, salt))
            {
                hashGenerator.IterationCount = iterations;
                return hashGenerator.GetBytes(hashSize);
            }
        }

        public string HashText(string toHash)
        {
            var salt = Salt;
            var hashed = Hash(toHash, salt);
            var combined = new byte[saltSize + hashSize];
            Array.Copy(salt, combined, saltSize);
            Array.Copy(hashed, 0, combined, saltSize, hashSize);
            return Convert.ToBase64String(combined).Replace('-', '+').Replace('_', '/');
        }

        public bool CompareAgainst(string hashedText, string rawInput)
        {
            if (hashedText.Length % 4 != 0)
                return false;
            try
            {
                var byteRepresentation = Convert.FromBase64String(hashedText);
                var salt = byteRepresentation.Take(saltSize);
                var hashed = byteRepresentation.Skip(saltSize);
                var hashedInput = Hash(rawInput, salt.ToArray());
                return hashed.SequenceEqual(hashedInput);
            } catch(Exception) { return false; }
        }
    }
}