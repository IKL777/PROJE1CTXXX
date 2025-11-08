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

namespace PROJECT
{
    public partial class WorkoutInputWindow1 : Window
    {
        public DateTime SelectedDate { get; private set; }
        public WorkoutType SelectedWorkoutType { get; private set; }
        public ObservableCollection<Exercise> Exercises { get; private set; } = new();

        // Только для отображения, не сохраняется в БД
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

        // Перетаскивание ИЗ списка "все упражнения"
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

        // Перетаскивание В список "выбранные"
        private void SelectedList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Exercise)) is Exercise exercise)
            {
                // Проверяем, нет ли уже такого упражнения с **такими же параметрами**
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name && ex.Sets == exercise.Sets && ex.Reps == exercise.Reps && ex.Weight == exercise.Weight))
                {
                    SelectedExercises.Add(exercise);
                }
            }
        }

        // Удаление упражнения из выбранных
        private void RemoveSelectedExercise_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var listBoxItem = FindAncestor<ListBoxItem>(button);
            if (listBoxItem?.DataContext is Exercise exercise)
            {
                SelectedExercises.Remove(exercise);
            }
        }

        // Кнопка "Создать тренировку"
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

            var newWorkout = new Workout
            {
                Date = SelectedDate,
                Type = SelectedWorkoutType,
                Exercises = SelectedExercises.ToList() // ← Теперь это работает!
            };

            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                context.Workouts.Add(newWorkout);
                await context.SaveChangesAsync(); // ← Сохраняем

                Exercises = new ObservableCollection<Exercise>(SelectedExercises); // Для UI
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения тренировки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обязательно для DragOver
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