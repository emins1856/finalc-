using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordResetApp
{
    public partial class MainWindow : Window
    {
        private readonly PasswordManager _passwordManager;
        private readonly BruteForceAttacker _bruteForceAttacker;
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            _passwordManager = new PasswordManager();
            _bruteForceAttacker = new BruteForceAttacker();
        }

        private void EncryptPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var password = PasswordTextBox.Text;
            var encryptedPassword = _passwordManager.EncryptPassword(password);
            _passwordManager.StoreEncryptedPassword(encryptedPassword);
            MessageBox.Show("Password encrypted and stored successfully!");
        }

        private async void StartBruteForceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(MaxThreadsTextBox.Text, out int maxThreads))
                {
                    MessageBox.Show("Invalid number of threads");
                    return;
                }

                var targetHash = File.ReadAllText("encryptedPassword.txt");
                _cancellationTokenSource = new CancellationTokenSource();

                var (foundPassword, duration) = await _bruteForceAttacker.BruteForceAttackAsync(targetHash, 5, maxThreads, _cancellationTokenSource.Token);

                if (foundPassword != null)
                {
                    ResultTextBlock.Text = $"Password found: {foundPassword}";
                }
                else
                {
                    ResultTextBlock.Text = "Password not found within the given constraints.";
                }

                TimeTakenTextBlock.Text = $"Time taken: {duration.TotalSeconds:F2} seconds";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
