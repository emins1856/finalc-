using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordResetApp
{
    public class BruteForceAttacker
    {
        private static readonly char[] Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        public async Task<(string foundPassword, TimeSpan duration)> BruteForceAttackAsync(string targetHash, int maxLength, int maxThreads, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            string foundPassword = null;

            var tasks = new List<Task>();

            for (int i = 0; i < maxThreads; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var localFoundPassword = BruteForce(targetHash, maxLength, cancellationToken);
                    if (localFoundPassword != null)
                    {
                        Interlocked.CompareExchange(ref foundPassword, localFoundPassword, null);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            stopwatch.Stop();
            return (foundPassword, stopwatch.Elapsed);
        }

        private string BruteForce(string targetHash, int maxLength, CancellationToken cancellationToken)
        {
            var currentPassword = new char[maxLength];
            for (int length = 1; length <= maxLength; length++)
            {
                if (GeneratePasswords(targetHash, currentPassword, 0, length, cancellationToken))
                {
                    return new string(currentPassword, 0, length);
                }
            }

            return null;
        }

        private bool GeneratePasswords(string targetHash, char[] currentPassword, int position, int length, CancellationToken cancellationToken)
        {
            if (position == length)
            {
                var password = new string(currentPassword, 0, length);
                return VerifyPassword(password, targetHash);
            }

            for (int i = 0; i < Characters.Length; i++)
            {
                currentPassword[position] = Characters[i];
                if (GeneratePasswords(targetHash, currentPassword, position + 1, length, cancellationToken))
                {
                    return true;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            return false;
        }

        private bool VerifyPassword(string password, string targetHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + PasswordManager.Salt));
                var hashString = Convert.ToBase64String(hash);
                return hashString == targetHash;
            }
        }
    }
}
