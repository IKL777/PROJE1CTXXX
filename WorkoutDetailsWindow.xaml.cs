using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
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

            // Загружаем и отображаем прогресс в вкладке "Сравнение"
            LoadProgressComparison();

            // Загружаем упражнения для графика
            LoadProgressChart();
        }

        private async void LoadProgressComparison()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var previousWorkouts = await context.Workouts
                    .Where(w => w.Date < CurrentWorkout.Date)
                    .OrderByDescending(w => w.Date)
                    .ToListAsync();

                var progressList = new List<ProgressComparisonItem>();

                foreach (var exercise in CurrentWorkout.Exercises)
                {
                    // Ищем по НАЗВАНИЮ упражнения, а не по ID
                    var previousExercise = previousWorkouts
                        .SelectMany(w => w.Exercises)
                        .Where(e => e.Name == exercise.Name)
                        .OrderByDescending(e => e.Workout.Date)
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

        private async void LoadProgressChart()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var allWorkouts = await context.Workouts
                    .Where(w => w.Date <= CurrentWorkout.Date)
                    .OrderBy(w => w.Date)
                    .ToListAsync();

                var allExerciseNames = allWorkouts
                    .SelectMany(w => w.Exercises)
                    .Select(e => e.Name)
                    .Distinct()
                    .ToList();

                ExerciseComboBox.ItemsSource = allExerciseNames;
                if (allExerciseNames.Any())
                    ExerciseComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для графика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExerciseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedExerciseName = ExerciseComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedExerciseName))
                return;

            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                var allWorkouts = await context.Workouts
                    .Where(w => w.Date <= CurrentWorkout.Date)
                    .OrderBy(w => w.Date)
                    .ToListAsync();

                var exerciseHistory = allWorkouts
                    .Where(w => w.Exercises.Any(e => e.Name == selectedExerciseName))
                    .SelectMany(w => w.Exercises.Where(e => e.Name == selectedExerciseName).Select(e => new { Date = w.Date, Exercise = e }))
                    .ToList();

                if (exerciseHistory.Any())
                {
                    var dates = exerciseHistory.Select(x => x.Date).ToList();
                    var weights = exerciseHistory.Select(x => x.Exercise.Weight).ToList();
                    var reps = exerciseHistory.Select(x => (double)x.Exercise.Reps).ToList();
                    var sets = exerciseHistory.Select(x => (double)x.Exercise.Sets).ToList();

                    var series = new List<ISeries>
                    {
                        new LineSeries<double>
                        {
                            Name = "Вес (кг)",
                            Values = weights,
                            Fill = null,
                            GeometrySize = 10
                        },
                        new LineSeries<double>
                        {
                            Name = "Повторения",
                            Values = reps,
                            Fill = null,
                            GeometrySize = 10
                        },
                        new LineSeries<double>
                        {
                            Name = "Подходы",
                            Values = sets,
                            Fill = null,
                            GeometrySize = 10
                        }
                    };

                    ProgressChart.Series = series;
                    ProgressChart.XAxes = new[]
                    {
                        new Axis
                        {
                            Name = "Дата",
                            Labels = dates.Select(d => d.ToString("dd.MM")).ToArray()
                        }
                    };
                }
                else
                {
                    ProgressChart.Series = new List<ISeries>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отображения графика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ProgressComparisonItem
    {
        public string Name { get; set; } = string.Empty;
        public string ComparisonText { get; set; } = string.Empty;
    }
}