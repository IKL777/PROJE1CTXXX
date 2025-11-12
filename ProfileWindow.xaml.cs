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
    /// Логика взаимодействия для ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        public ProfileWindow()
        {
            InitializeComponent();
        }
        private void LoadUserData()
        {
            var user = App.CurrentUser;
            if (user == null) return;

            UsernameBox.Text = user.Username;
            AgeBox.Text = user.Age.ToString();
            HeightBox.Text = user.Height.ToString();

            // Пол
            GenderBox.SelectedIndex = user.Gender == "Женский" ? 1 : 0;

            // Уровень подготовки
            var fitnessLevels = new[] { "Новичок", "Средний", "Продвинутый" };
            GenderBox.SelectedIndex = System.Array.IndexOf(fitnessLevels, user.FitnessLevel);

            // Цель
            var goals = new[] { "Набор массы", "Сушка", "Выносливость" };
            GoalBox.SelectedIndex = System.Array.IndexOf(goals, user.Goal);

            // Оборудование
            foreach (var item in EquipmentBox.Items.OfType<ListBoxItem>())
            {
                item.IsSelected = user.Equipment.Contains(item.Content.ToString());
            }

            // Тренировок в неделю
            WorkoutsPerWeekBox.SelectedIndex = user.WorkoutsPerWeek - 2; // 2→0, 3→1...

            // Длительность
            var durations = new[] { 30, 45, 60, 75, 90 };
            DurationBox.SelectedIndex = System.Array.IndexOf(durations, user.PreferredDuration);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var user = App.CurrentUser;
            if (user == null) return;

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

            user.Age = age;
            user.Height = height;
            user.Gender = (GenderBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            user.FitnessLevel = (FitnessLevelBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            user.Goal = (GoalBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            user.Equipment = EquipmentBox.SelectedItems.Cast<ListBoxItem>()
                .Select(item => item.Content.ToString()).ToList();

            user.WorkoutsPerWeek = WorkoutsPerWeekBox.SelectedIndex + 2;
            user.PreferredDuration = int.Parse((DurationBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "60");

            MessageBox.Show("Профиль сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}
