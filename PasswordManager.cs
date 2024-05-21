using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordResetApp
{
    public class PasswordManager
    {
        public const string Salt = "staticSalt"; 

        public string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + Salt; 
                var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword); 
                var hashBytes = sha256.ComputeHash(saltedPasswordBytes); 
                return Convert.ToBase64String(hashBytes); 
            }
        }

        public void StoreEncryptedPassword(string encryptedPassword)
        {
            File.WriteAllText("encryptedPassword.txt", encryptedPassword); 
        }
    }
}
