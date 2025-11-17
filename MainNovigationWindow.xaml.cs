using Class1;
using PR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PROJECT
{
    /// <summary>
    /// Логика взаимодействия для MainNovigationWindow.xaml
    /// </summary>
    public partial class MainNovigationWindow : Window
    {
        public MainNovigationWindow()
        {
            InitializeComponent();
            WelcomeText.Text = $"Добро пожаловать,\n{App.CurrentUser.Username}!";
        }
        private void BtnWorkouts_Click(object sender, RoutedEventArgs e)
        {
            var workoutWindow = new WorkoutListWindow(); // ← окно со списком тренировок
            workoutWindow.Show();
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            var ProfileWindow= new ProfileWindow();
            ProfileWindow.Show();
        }

        private void BtnRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                // ПЕРЕДАЁМ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ
                var tipsWindow = new RecoveryTipsWindow(App.CurrentUser);
                tipsWindow.Show();
            }
            else
            {
                MessageBox.Show("Сначала войдите в профиль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnPrograms_Click(object sender, RoutedEventArgs e)
        {
            var programsWindow = new WorkoutProgramsWindow();
            programsWindow.Show();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся к окну входа
            var loginWindow = new RegistrationWindow();
            loginWindow.Show();
            this.Close();
        }

        private void CloseALL_Click(object sender, RoutedEventArgs e)
        {
            var windowsToClose = Application.Current.Windows
        .Cast<Window>()
        .Where(w => w != this)
        .ToList();

            foreach (var window in windowsToClose)
            {
                window.Close();
            }

            this.Close();
        }
        private void ButtonAdmin_Click(object sender, RoutedEventArgs e)
        {
            new WorkoutAdminControl().Show();
        }

    }
}
