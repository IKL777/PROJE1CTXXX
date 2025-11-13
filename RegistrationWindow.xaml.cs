using Class1;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PROJECT;

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
                context.Database.EnsureCreated(); // На случай, если БД ещё не создана

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user == null)
                {
                    MessageBox.Show("Неверное имя или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // СОХРАНЯЕМ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ
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

        
    }
}