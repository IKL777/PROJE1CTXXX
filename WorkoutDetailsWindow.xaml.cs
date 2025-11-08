using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Class1;

namespace PROJECT
{
    public partial class WorkoutDetailsWindow : Window
    {
        private Workout CurrentWorkout;

        public WorkoutDetailsWindow(Workout workout)
        {
            InitializeComponent();
            if (workout == null)
                return;

            CurrentWorkout = workout;

            // Отображаем данные в "Детали"
            DateText.Text = $"Дата: {workout.Date:dd.MM.yyyy}";
            TypeText.Text = $"Тип: {workout.Type}";
            ExercisesList.ItemsSource = workout.Exercises;

            // Загружаем и отображаем прогресс
            LoadProgressComparison();
        }

        private async void LoadProgressComparison()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                // Находим тренировки до текущей, отсортированные по дате
                var previousWorkouts = await context.Workouts
                    .Where(w => w.Date < CurrentWorkout.Date)
                    .OrderByDescending(w => w.Date)
                    .ToListAsync();

                // Список для отображения прогресса
                var progressList = new List<ProgressComparisonItem>();

                foreach (var exercise in CurrentWorkout.Exercises)
                {
                    // Ищем это упражнение в предыдущих тренировках
                    var previousExercise = previousWorkouts
                    .SelectMany(w => w.Exercises.Select(e => new { Exercise = e, WorkoutDate = w.Date }))
                    .Where(x => x.Exercise.Id == exercise.Id)
                    .OrderByDescending(x => x.WorkoutDate)
                    .Select(x => x.Exercise)
                    .FirstOrDefault();

                    if (previousExercise != null)
                    {
                        var comparisonText = GenerateComparisonText(previousExercise, exercise);
                        progressList.Add(new ProgressComparisonItem
                        {
                            Name = exercise.Name,
                            ComparisonText = comparisonText
                        });
                    }
                    else
                    {
                        progressList.Add(new ProgressComparisonItem
                        {
                            Name = exercise.Name,
                            ComparisonText = "Новое упражнение"
                        });
                    }
                }

                ProgressComparisonList.ItemsSource = progressList;
                ProgressHeader.Text = "Сравнение с предыдущими тренировками";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки прогресса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateComparisonText(Exercise old, Exercise current)
        {
            var changes = new List<string>();

            if (current.Sets != old.Sets)
                changes.Add($"Подходы: {old.Sets} → {current.Sets}");

            if (current.Reps != old.Reps)
                changes.Add($"Повторения: {old.Reps} → {current.Reps}");

            if (current.Weight != old.Weight)
                changes.Add($"Вес: {old.Weight} → {current.Weight}");

            return changes.Count > 0 ? string.Join(", ", changes) : "Без изменений";
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Вспомогательный класс для отображения прогресса
    public class ProgressComparisonItem
    {
        public string Name { get; set; } = string.Empty;
        public string ComparisonText { get; set; } = string.Empty;
    }
}