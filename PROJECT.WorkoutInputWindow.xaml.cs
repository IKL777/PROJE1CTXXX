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
                        new Exercise { Name = "Подтягивания", Description = "Упражнение на спину и бицепс", Sets = 3, Reps = 8, Weight = 0 },
                        new Exercise { Name = "Отжимания", Description = "Упражнение на грудь и плечи", Sets = 3, Reps = 20, Weight = 0 }
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
                var clonedExercise = new Exercise
                {
                    Id = 0,
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
                // Проверяем ТОЛЬКО по имени
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name))
                {
                    SelectedExercises.Add(exercise);
                }
                else
                {
                    MessageBox.Show($"Упражнение '{exercise.Name}' уже добавлено. Измените параметры в существующей карточке.",
                        "Дубликат",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
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

            foreach (var ex in SelectedExercises)
            {
                if (ex.Sets <= 0 || ex.Sets > 40)
                {
                    MessageBox.Show($"Упражнение '{ex.Name}': количество подходов должно быть больше 0 и меньше 40!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ex.Reps < 0 || ex.Reps>150)
                {
                    MessageBox.Show($"Упражнение '{ex.Name}': повторения не могут быть отрицательными и больше 150 ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ex.Weight < 0 || ex.Weight > 500)
                {
                    MessageBox.Show($"Упражнение '{ex.Name}': вес должен быть от 0 до 500 кг!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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
                    UserId=App.CurrentUser.Id,
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
            if (e.Data.GetData(typeof(Exercise)) is Exercise exercise)
            {
                // НЕ ДОБАВЛЯЕМ! Только проверяем возможность перетаскивания
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void Weight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, точку и запятую
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9\.\,]$");
        }


        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]$");
        }
    }
}