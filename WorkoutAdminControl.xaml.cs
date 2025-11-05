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
using Class1;

namespace PROJECT
{
    /// <summary>
    /// Логика взаимодействия для WorkoutAdminControl.xaml
    /// </summary>
    public partial class WorkoutAdminControl : Window
    {
        public WorkoutAdminControl()
        {
            InitializeComponent();
            LoadExercises();
        }

        private void LoadExercises()
        {
            ExercisesPanel.Children.Clear();
            foreach (var exercise in ExerciseRepository.AllExercises)
            {
                var card = CreateExerciseCardUI(exercise);
                ExercisesPanel.Children.Add(card);
            }
        }

        private UIElement CreateExerciseCardUI(ExerciseCard exercise)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Colors.LightBlue),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                Tag = exercise
            };

            var stack = new StackPanel();

            // === ЗАХВАТНАЯ ЗОНА ДЛЯ DRAG & DROP ===
            var dragHandle = new TextBlock
            {
                Text = "🏋️ " + exercise.Name,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent, // важно для hit-testing
                Cursor = Cursors.SizeAll
            };

            // Только по этому элементу можно тащить
            dragHandle.PreviewMouseLeftButtonDown += (s, e) =>
            {
                var data = new DataObject("ExerciseCard", exercise);
                DragDrop.DoDragDrop(dragHandle, data, DragDropEffects.Copy);
                e.Handled = true; // необязательно, но чисто
            };

            stack.Children.Add(dragHandle);
            stack.Children.Add(new TextBlock { Text = exercise.Description });
            stack.Children.Add(new TextBlock { Text = $"Повторений: {exercise.Reps}" });
            stack.Children.Add(new TextBlock { Text = $"Подходов: {exercise.Sets}" });
            stack.Children.Add(new TextBlock { Text = $"Вес: {exercise.Weight}" });

            // === КНОПКИ ===
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

            var editButton = new Button
            {
                Content = "✏️ Редактировать",
                Margin = new Thickness(0, 0, 10, 0),
                Tag = exercise,
                Background = Brushes.Transparent
            };
            editButton.Click += EditExercise_Click;
                        // ыквф
            var deleteButton = new Button
            {
                Content = "🗑️ Удалить",
                Tag = exercise,
                Background = Brushes.Transparent
            };
            deleteButton.Click += DeleteExercise_Click;

            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);
            stack.Children.Add(buttonPanel);

            border.Child = stack;
            return border;
        }

        private void CreateExercise_Click(object sender, RoutedEventArgs e)
        {
            // Простой пример: жёстко зададим упражнение
            var dialog = new CreateExerciseDialog(); 
            dialog.Owner = this;

            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreatedExercise != null)
            {
                ExerciseRepository.AllExercises.Add(dialog.CreatedExercise);
                LoadExercises(); // ← Вот он! Обновляет UI после добавления
            }
        }
        private void EditExercise_Click(object sender, RoutedEventArgs e)
        {
            
            var button = sender as Button;
            var exercise = button?.Tag as ExerciseCard;

            if (exercise == null) return;

            // Создаём диалог и заполняем его текущими данными
            var dialog = new CreateExerciseDialog
            {
                Owner = this
            };

            // Заполняем поля
            dialog.NameBox.Text = exercise.Name;
            dialog.DescriptionBox.Text = exercise.Description;
            dialog.RepsBox.Text = exercise.Reps.ToString();
            dialog.SetsBox.Text = exercise.Sets.ToString();
            dialog.WeightBox.Text = exercise.Weight.ToString();

            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreatedExercise != null)
            {
                // Обновляем существующий объект (чтобы не нарушать ссылки, если они где-то используются)
                exercise.Name = dialog.CreatedExercise.Name;
                exercise.Description = dialog.CreatedExercise.Description;
                exercise.Reps = dialog.CreatedExercise.Reps;
                exercise.Sets = dialog.CreatedExercise.Sets;
                exercise.Weight = dialog.CreatedExercise.Weight;

                LoadExercises(); // Перезагружаем UI
            }
        }

        private void DeleteExercise_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var exercise = button?.Tag as ExerciseCard;

            if (exercise == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить упражнение '{exercise.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                ExerciseRepository.AllExercises.Remove(exercise);
                LoadExercises(); // Обновляем список
            }
        }
    }
}
