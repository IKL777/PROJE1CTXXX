using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Class1;// предполагается, что у вас есть класс Exercise и WorkoutType
using PROJECT; 

namespace PROJECT
{
    public partial class WorkoutListWindow : Window
    {
        public ObservableCollection<Workout> Workouts { get; set; } = new ObservableCollection<Workout>();

        public WorkoutListWindow()
        {
            InitializeComponent();
            WorkoutList.ItemsSource = Workouts;
        }

        private async void AddWorkout_Click(object sender, RoutedEventArgs e)
        {

            var inputWindow = new WorkoutInputWindow1();
            if (inputWindow.ShowDialog() == true)
            {
                var newWorkout = new Workout
                {
                    Date = inputWindow.SelectedDate,
                    Type = inputWindow.SelectedWorkoutType,
                    Exercises = inputWindow.Exercises.ToList() // ← ObservableCollection → List
                };

                try
                {
                    using var context = new AppDbContext();
                    context.Database.EnsureCreated();
                    context.Workouts.Add(newWorkout);
                    await context.SaveChangesAsync();

                    Workouts.Add(newWorkout); // Для отображения в списке
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения тренировки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WorkoutList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WorkoutList.SelectedItem is Workout selectedWorkout)
            {
                var detailsWindow = new WorkoutDetailsWindow(selectedWorkout); // ← передаём!
                detailsWindow.Show();
            }
        }

        private void WorkoutList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}