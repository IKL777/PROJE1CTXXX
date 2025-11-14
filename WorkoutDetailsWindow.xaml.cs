using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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

            DateText.Text = $"Дата: {workout.Date:dd.MM.yyyy}";
            TypeText.Text = $"Тип: {workout.Type}";
            ExercisesList.ItemsSource = workout.Exercises;

            LoadProgressComparison();
            LoadProgressChart();
        }

        private async void LoadProgressComparison()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                // Загружаем ТРЕНИРОВКИ с их УПРАЖНЕНИЯМИ
                var previousWorkouts = await context.Workouts
                    
                    .Where(w => w.Date < CurrentWorkout.Date && w.User.Id == CurrentWorkout.UserId)
                    .OrderByDescending(w => w.Date)
                    .Include(w => w.Exercises) // ← ВАЖНО: загружаем упражнения
                    .ToListAsync();

                var progressList = new List<ProgressComparisonItem>();

                foreach (var exercise in CurrentWorkout.Exercises)
                {
                    // Ищем ПОСЛЕДНЮЮ тренировку с таким же упражнением по ИМЕНИ
                    var lastWorkoutWithExercise = previousWorkouts
                        .FirstOrDefault(w => w.Exercises.Any(e => e.Name == exercise.Name));

                    if (lastWorkoutWithExercise != null)
                    {
                        var previousExercise = lastWorkoutWithExercise.Exercises
                            .FirstOrDefault(e => e.Name == exercise.Name);

                        if (previousExercise != null)
                        {
                            var comparisonText = GenerateComparisonText(previousExercise, exercise);
                            progressList.Add(new ProgressComparisonItem
                            {
                                Name = exercise.Name,
                                ComparisonText = comparisonText
                            });
                            continue;
                        }
                    }

                    // Если не нашли — новое упражнение
                    progressList.Add(new ProgressComparisonItem
                    {
                        Name = exercise.Name,
                        ComparisonText = "Новое упражнение"
                    });
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

            if (Math.Abs(current.Weight - old.Weight) > 0.01)
                changes.Add($"Вес: {old.Weight} → {current.Weight}");

            return changes.Count > 0 ? string.Join(", ", changes) : "Без изменений";
        }

        private async void LoadProgressChart()
        {
            try
            {
                using var context = new AppDbContext();
                context.Database.EnsureCreated();

                // Загружаем ВСЕ тренировки с упражнениями
                var allWorkouts = await context.Workouts
                    .Include(w => w.Exercises)
                    .Where(w => w.Date <= CurrentWorkout.Date && w.User.Id == CurrentWorkout.UserId)
                    .OrderBy(w => w.Date)
                    .Include(w => w.Exercises) // ← ВАЖНО
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

                // Загружаем ВСЕ тренировки с упражнениями
                var allWorkouts = await context.Workouts
                    .Where(w => w.Date <= CurrentWorkout.Date && w.User.Id == CurrentWorkout.UserId)
                    .OrderBy(w => w.Date)
                    .Include(w => w.Exercises) // ← ВАЖНО
                    .ToListAsync();

                // Собираем ИСТОРИЮ по упражнению
                var exerciseHistory = new List<ExerciseHistoryItem>();

                foreach (var workout in allWorkouts)
                {
                    var exercises = workout.Exercises
                        .Where(e => e.Name == selectedExerciseName)
                        .ToList();

                    foreach (var exercise in exercises)
                    {
                        exerciseHistory.Add(new ExerciseHistoryItem
                        {
                            Date = workout.Date,
                            Weight = exercise.Weight,
                            Reps = exercise.Reps,
                            Sets = exercise.Sets
                        });
                    }
                }

                if (exerciseHistory.Any())
                {
                    var dates = exerciseHistory.Select(x => x.Date).ToList();
                    var weights = exerciseHistory.Select(x => x.Weight).ToList();
                    var reps = exerciseHistory.Select(x => (double)x.Reps).ToList();
                    var sets = exerciseHistory.Select(x => (double)x.Sets).ToList();

                    var series = new List<ISeries>
                    {
                        new LineSeries<double>
                        {
                            Name = "Вес (кг)",
                            Values = weights,
                            Fill = null,
                            GeometrySize = 10,
                            LineSmoothness = 0
                        },
                        new LineSeries<double>
                        {
                            Name = "Повторения",
                            Values = reps,
                            Fill = null,
                            GeometrySize = 10,
                            LineSmoothness = 0
                        },
                        new LineSeries<double>
                        {
                            Name = "Подходы",
                            Values = sets,
                            Fill = null,
                            GeometrySize = 10,
                            LineSmoothness = 0
                        }
                    };

                    ProgressChart.Series = series;
                    ProgressChart.XAxes = new[]
                    {
                        new Axis
                        {
                            Name = "Дата",
                            Labels = dates.Select(d => d.ToString("dd.MM")).ToArray(),
                            LabelsRotation = -10
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

    // Вспомогательный класс для истории упражнений
    public class ExerciseHistoryItem
    {
        public DateTime Date { get; set; }
        public double Weight { get; set; }
        public int Reps { get; set; }
        public int Sets { get; set; }
    }
}