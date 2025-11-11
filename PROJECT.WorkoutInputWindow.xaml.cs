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

                // Проверяем, есть ли упражнения в БД
                if (!context.Exercises.Any())
                {
                    // Добавляем тестовые упражнения если база пустая
                    var defaultExercises = new List<Exercise>
                    {
                        new Exercise { Name = "Приседания", Description = "Базовое упражнение на ноги", Sets = 3, Reps = 10, Weight = 60 },
                        new Exercise { Name = "Жим лёжа", Description = "Базовое упражнение на грудь", Sets = 3, Reps = 8, Weight = 50 },
                        new Exercise { Name = "Тяга штанги", Description = "Упражнение на спину", Sets = 4, Reps = 10, Weight = 70 },
                        new Exercise { Name = "Становая тяга", Description = "Комплексное упражнение", Sets = 3, Reps = 6, Weight = 100 },
                        new Exercise { Name = "Подтягивания", Description = "Упражнение на спину и бицепс", Sets = 3, Reps = 8, Weight = 0 }
                    };

                    context.Exercises.AddRange(defaultExercises);
                    await context.SaveChangesAsync();
                }

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
                // КЛОНИРУЕМ упражнение при перетаскивании
                var clonedExercise = new Exercise
                {
                    Id = 0, // Новый Id
                    Name = exercise.Name,
                    Description = exercise.Description,
                    Sets = exercise.Sets,
                    Reps = exercise.Reps,
                    Weight = exercise.Weight
                };

                DragDrop.DoDragDrop(listBoxItem, clonedExercise, DragDropEffects.Copy);
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
                // Проверяем по имени, а не по всем параметрам
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name))
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
                    Exercises = new List<Exercise>()
                };

                context.Workouts.Add(newWorkout);
                await context.SaveChangesAsync();

                foreach (var ex in SelectedExercises)
                {
                    var clonedExercise = new Exercise
                    {
                        Name = ex.Name,
                        Description = ex.Description,
                        Sets = ex.Sets,
                        Reps = ex.Reps,
                        Weight = ex.Weight,
                        WorkoutId = newWorkout.Id
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

                Debug.WriteLine($"DEBUG ERROR:\n{errorMessage}");
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