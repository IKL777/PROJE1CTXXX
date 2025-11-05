using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public ObservableCollection<ExerciseCard> AvailableExercises { get; } = new();
        public ObservableCollection<Exercise> SelectedExercises { get; } = new();

        public WorkoutInputWindow1()
        {
            InitializeComponent();
            DataContext = this;

            foreach (var card in ExerciseRepository.AllExercises)
            {
                AvailableExercises.Add(card);
            }

            DatePicker.SelectedDate = DateTime.Now;
        }

        // Перетаскивание ИЗ списка "все упражнения"
        private void AvailableExercise_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;
            var listBoxItem = FindAncestor<ListBoxItem>(originalSource);

            if (listBoxItem?.DataContext is ExerciseCard card)
            {
                var exercise = new Exercise
                {
                    Name = card.Name,
                    Sets = card.Sets,
                    Reps = card.Reps,
                    Weight = card.Weight,
                    DifficultyLevel = Difficulty.Medium
                };

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
                if (!SelectedExercises.Any(ex => ex.Name == exercise.Name))
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
        private void AddButton_Click1(object sender, RoutedEventArgs e)
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

            Exercises = new ObservableCollection<Exercise>(SelectedExercises);
            DialogResult = true;
            Close();
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