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

        private async Task LoadWorkouts()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var workouts = await context.Workouts
                    .Where(w => w.UserId == App.CurrentUser.Id) // ← КЛЮЧЕВОЙ ФИЛЬТР
                    .OrderBy(w => w.Date)
                    .Include(w => w.Exercises) 
                    .ToListAsync();

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
                    
                    LoadWorkouts();
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
        private async void DeleteWorkout_Click(object sender, RoutedEventArgs e)
        {
            if (WorkoutList.SelectedItem is Workout workout)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить тренировку\n{workout.Date:dd.MM.yyyy} - {workout.Type}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                    return;

                try
                {
                    using var context = new AppDbContext();
                    context.Workouts.Remove(workout);
                    await context.SaveChangesAsync();
                    await LoadWorkouts(); // Перезагружаем список
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления тренировки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}