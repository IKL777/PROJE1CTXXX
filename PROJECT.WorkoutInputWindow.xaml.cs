using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Class1;
using System.Diagnostics;

namespace PROJECT
{
    public partial class WorkoutInputWindow1 : Window
    {
        public DateTime SelectedDate { get; private set; }
        public WorkoutType SelectedWorkoutType { get; private set; }
        public ObservableCollection<Exercise> Exercises { get; private set; } = new();
        public ObservableCollection<Exercise> AvailableExercises { get; } = new();
        public ObservableCollection<Exercise> SelectedExercises { get; } = new();

        public WorkoutInputWindow1()
        {
            InitializeComponent();
            DataContext = this;
            LoadAvailableExercises();
            DatePicker.SelectedDate = DateTime.Now;
        }

        private async void LoadAvailableExercises()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();
                var exercises = await context.Exercises.ToListAsync();
                AvailableExercises.Clear();
                foreach (var exercise in exercises)
                {
                    AvailableExercises.Add(exercise);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки упражнений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AvailableExercise_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;
            var listBoxItem = FindAncestor<ListBoxItem>(originalSource);

            if (listBoxItem?.DataContext is Exercise exercise)
            {
                DragDrop.DoDragDrop(listBoxItem, exercise, DragDropEffects.Copy);
                e.Handled = true;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                    return ancestor;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void SelectedList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Exercise)) is Exercise exercise)
            {
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name && ex.Sets == exercise.Sets && ex.Reps == exercise.Reps && ex.Weight == exercise.Weight))
                {
                    SelectedExercises.Add(exercise);
                }
            }
        }

        private void RemoveSelectedExercise_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var listBoxItem = FindAncestor<ListBoxItem>(button);
            if (listBoxItem?.DataContext is Exercise exercise)
            {
                SelectedExercises.Remove(exercise);
            }
        }


        private async void AddButton_Click1(object sender, RoutedEventArgs e)
        {
            if (SelectedExercises.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одно упражнение.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedDate = DatePicker.SelectedDate ?? DateTime.Now;
            var selectedItem = TypeComboBox.SelectedItem as ComboBoxItem;
            var typeStr = selectedItem?.Content?.ToString() ?? "WithWeights";

            SelectedWorkoutType = typeStr switch
            {
                "Bodyweight" => WorkoutType.Bodyweight,
                _ => WorkoutType.WithWeights
            };

            try
            {
                using var context = new AppDbContext();

                var newWorkout = new Workout
                {
                    Date = SelectedDate,
                    Type = SelectedWorkoutType,
                    Exercises = new List<Exercise>() // пустой список
                };

                context.Workouts.Add(newWorkout);
                await context.SaveChangesAsync(); // Получаем Id тренировки

                foreach (var ex in SelectedExercises)
                {
                    var clonedExercise = new Exercise
                    {
                        // ВАЖНО: НЕ КОПИРУЕМ Id!
                        Name = ex.Name,
                        Description = ex.Description,
                        Sets = ex.Sets,
                        Reps = ex.Reps,
                        Weight = ex.Weight,
                        WorkoutId = newWorkout.Id // ← Только внешний ключ
                                                  // Workout = newWorkout ← УДАЛИТЬ ЭТУ СТРОКУ!
                    };

                    context.Exercises.Add(clonedExercise);
                    newWorkout.Exercises.Add(clonedExercise);
                }

                await context.SaveChangesAsync();

                Exercises = new ObservableCollection<Exercise>(newWorkout.Exercises);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка сохранения тренировки:\n{ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }

                // Логируем в Debug для детального анализа
                System.Diagnostics.Debug.WriteLine($"DEBUG ERROR:\n{errorMessage}");

                MessageBox.Show(errorMessage, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectedList_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Exercise)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
}