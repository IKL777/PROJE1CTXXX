using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Class1;
using Microsoft.EntityFrameworkCore;
using PR;

namespace PROJECT
{
    public partial class RegistrationWindow : Window
    {
        private bool _isPasswordVisible = false;

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordBox.Password = PasswordText.Text;
                PasswordText.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                TogglePasswordButton.Content = "👁️";
                _isPasswordVisible = false;
            }
            else
            {
                PasswordText.Text = PasswordBox.Password;
                PasswordText.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "🔒";
                _isPasswordVisible = true;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new UserProfileWindow();
            profileWindow.ShowDialog();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = NameTextBox.Text.Trim();
            var password = _isPasswordVisible ? PasswordText.Text.Trim() : PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                // ✅ ИЩЕМ ТОЛЬКО ПО ЛОГИНУ (без пароля!)
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                // ✅ ПРОВЕРЯЕМ ХЕШ ТОЛЬКО ЕСЛИ ПОЛЬЗОВАТЕЛЬ НАЙДЕН
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    await Task.Delay(500); // Защита от брутфорса
                    MessageBox.Show("Неверное имя или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    // ✅ Очищаем ТОЛЬКО поле пароля (логин оставляем)
                    PasswordBox.Clear();
                    PasswordText.Clear();
                    return;
                }

                App.CurrentUser = user;
                var mainWindow = new MainNovigationWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Забыли пароль? Создайте новый аккаунт");
        }
    }
}