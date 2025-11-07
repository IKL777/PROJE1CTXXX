using Class1;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PROJECT
{
    public partial class WorkoutAdminControl : Window
    {
        public WorkoutAdminControl()
        {
            InitializeComponent();
            LoadExercises();
        }

        private async void LoadExercises()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var exercises = await context.Exercises.ToListAsync();

                ExercisesPanel.Children.Clear();
                foreach (var exercise in exercises)
                {
                    var card = CreateExerciseCardUI(exercise);
                    ExercisesPanel.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки упражнений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private UIElement CreateExerciseCardUI(Exercise exercise)
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

            var dragHandle = new TextBlock
            {
                Text = "🏋️ " + exercise.Name,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                Cursor = Cursors.SizeAll
            };

            dragHandle.PreviewMouseLeftButtonDown += (s, e) =>
            {
                var data = new DataObject("Exercise", exercise);
                DragDrop.DoDragDrop(dragHandle, data, DragDropEffects.Copy);
                e.Handled = true;
            };

            stack.Children.Add(dragHandle);
            stack.Children.Add(new TextBlock { Text = exercise.Description });
            stack.Children.Add(new TextBlock { Text = $"Повторений: {exercise.Reps}" });
            stack.Children.Add(new TextBlock { Text = $"Подходов: {exercise.Sets}" });
            stack.Children.Add(new TextBlock { Text = $"Вес: {exercise.Weight}" });

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

            var editButton = new Button
            {
                Content = "✏️ Редактировать",
                Margin = new Thickness(0, 0, 10, 0),
                Tag = exercise,
                Background = Brushes.Transparent
            };
            editButton.Click += EditExercise_Click;

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

        private async void CreateExercise_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateExerciseDialog();
            dialog.Owner = this;

            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreatedExercise != null)
            {
                var newExercise = dialog.CreatedExercise;

                try
                {
                    using var context = new AppDbContext();
                    context.Exercises.Add(newExercise);
                    await context.SaveChangesAsync();

                    LoadExercises(); // Обновляем UI
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения упражнения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditExercise_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var exercise = button?.Tag as Exercise;

            if (exercise == null) return;

            var dialog = new CreateExerciseDialog
            {
                Owner = this
            };

            // Заполняем поля текущими значениями
            dialog.NameBox.Text = exercise.Name;
            dialog.DescriptionBox.Text = exercise.Description;
            dialog.SetsBox.Text = exercise.Sets.ToString();
            dialog.RepsBox.Text = exercise.Reps.ToString();
            dialog.WeightBox.Text = exercise.Weight.ToString();

            bool? result = dialog.ShowDialog();

            if (result == true && dialog.CreatedExercise != null)
            {
                // Обновляем данные в БД
                exercise.Name = dialog.CreatedExercise.Name;
                exercise.Description = dialog.CreatedExercise.Description;
                exercise.Sets = dialog.CreatedExercise.Sets;
                exercise.Reps = dialog.CreatedExercise.Reps;
                exercise.Weight = dialog.CreatedExercise.Weight;

                try
                {
                    using var context = new AppDbContext();
                    context.Exercises.Update(exercise);
                    await context.SaveChangesAsync();

                    LoadExercises(); // Обновляем UI
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления упражнения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteExercise_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var exercise = button?.Tag as Exercise;

            if (exercise == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить упражнение '{exercise.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new AppDbContext();
                    context.Exercises.Remove(exercise);
                    await context.SaveChangesAsync();

                    LoadExercises(); // Обновляем UI
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления упражнения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}