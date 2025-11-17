using Class1;
using Microsoft.EntityFrameworkCore;
using PROJECT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace PROJECT
{
    public partial class UserProfileWindow : Window
    {
        private bool _isPasswordVisible = false;

        public UserProfileWindow()
        {
            InitializeComponent();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            
            var username = UsernameBox.Text.Trim();
            var password = _isPasswordVisible ? PasswordText.Text.Trim() : PasswordBox.Password.Trim();//


            if (password.Length < 6)//Защита длины пароля
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль обязательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(AgeBox.Text, out int age) || age < 12 || age > 100)
            {
                MessageBox.Show("Укажите корректный возраст (от 12 до 100 лет).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(HeightBox.Text, out decimal height) || height < 100 || height > 250)
            {
                MessageBox.Show("Укажите корректный рост (от 100 до 250 см).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(WeightBox.Text, out decimal weight) || weight< 45 || weight > 150)
            {
                MessageBox.Show("Укажите корректный вес (от 45 до 150 кг).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var equipment = EquipmentBox.SelectedItems.Cast<ListBoxItem>()
                .Select(item => item.Content.ToString())
                .ToList();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);//хэширование пароля 

            var user = new User
            {
                Username = username,
                Password = hashedPassword,
                Age = age,
                Gender = (GenderBox.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                Height = height,
                FitnessLevel = (FitnessLevelBox.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                Goal = (GoalBox.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                WorkoutsPerWeek = int.Parse((WorkoutsPerWeekBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "3"),
                PreferredDuration = int.Parse((DurationBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "60"),
                Equipment = equipment,
                Weight = weight
            };

            try
            {
                // ✅ Только ОДНО объявление context — внутри try
                using var context = new AppDbContext();
                context.Database.EnsureCreated(); // Создаёт таблицы, если их нет

                if (await context.Users.AnyAsync(u => u.Username == username))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                context.Users.Add(user);
                await context.SaveChangesAsync();

                user.SaveToJson();

                MessageBox.Show("Профиль успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить профиль: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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

        private void GoalBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}