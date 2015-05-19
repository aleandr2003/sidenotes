using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace SideNotes.Models
{
    public class Password
    {
        public bool Matches(string clearText)
        {
            var probedPasswordHash = new Rfc2898DeriveBytes(clearText, Salt).GetBytes(HashSize);
            return Hash.SequenceEqual(probedPasswordHash);
        }

        public Password(string clearText)
        {
            var deriveBytes = new Rfc2898DeriveBytes(clearText, SaltSize);
            Hash = deriveBytes.GetBytes(HashSize);
            Salt = deriveBytes.Salt;
        }

        public Password(byte[] hash, byte[] salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public Password()
            : this("")
        {
        }

        public byte[] Hash { get; private set; }

        public byte[] Salt { get; private set; }

        const int SaltSize = 16;
        const int HashSize = 128;
    }
}