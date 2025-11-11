using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Class1;
using Microsoft.EntityFrameworkCore;

namespace PROJECT
{
    public partial class WorkoutListWindow : Window
    {
        public ObservableCollection<Workout> Workouts { get; set; } = new();

        public WorkoutListWindow()
        {
            InitializeComponent();
            WorkoutList.ItemsSource = Workouts;
            LoadWorkouts();
        }

        private async void LoadWorkouts()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var workouts = await context.Workouts.ToListAsync();
                Workouts.Clear();
                foreach (var workout in workouts)
                {
                    Workouts.Add(workout);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тренировок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new WorkoutInputWindow1();
            if (inputWindow.ShowDialog() == true)
            {
                try
                {
                    // Перезагружаем список тренировок из БД
                    using var context = new AppDbContext();
                    var workouts = await context.Workouts.Include(w => w.Exercises).ToListAsync();

                    Workouts.Clear();
                    foreach (var workout in workouts)
                    {
                        Workouts.Add(workout);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления списка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WorkoutList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WorkoutList.SelectedItem is Workout selectedWorkout)
            {
                var detailsWindow = new WorkoutDetailsWindow(selectedWorkout);
                detailsWindow.Show();
            }
        }

        private void WorkoutList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Пока пустой — можно добавить логику, если нужно
        }
    }
}