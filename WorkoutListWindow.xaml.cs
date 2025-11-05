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

        private void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new WorkoutInputWindow1();
            if (inputWindow.ShowDialog() == true)
            {
                var newWorkout = new Workout
                {
                    Date = inputWindow.SelectedDate,
                    Type = inputWindow.SelectedWorkoutType,
                    Exercises = inputWindow.Exercises
                };
                Workouts.Add(newWorkout);
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