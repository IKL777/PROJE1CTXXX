using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class1
{
    public enum WorkoutType { WithWeights, Bodyweight }
    public enum Difficulty { Easy, Medium, Hard }

    public class Workout
    {
        public int id {  get; set; }
        public DateTime Date { get; set; }
        public Class1.WorkoutType Type { get; set; }
        public ObservableCollection<Exercise> Exercises { get; set; }

        public string DisplayText
        {
            get
            {
                var exercisesNames = Exercises.Select(e => e.Name).ToArray();
                string exercisesStr = string.Join(", ", exercisesNames);
                return $"{Date.ToShortDateString()} - {Type} - {exercisesStr}";
            }
        }
    }

    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
        public Difficulty DifficultyLevel { get; set; }

        public string DisplayText => $"{Name} - {Sets}x{Reps} @ {Weight}kg";
    }

    public class ExerciseCard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Reps { get; set; }
        public int Sets { get; set; }
        public double Weight { get; set; }
        public override string ToString() => Name;
    }

    public static class ExerciseRepository
    {
        public static ObservableCollection<ExerciseCard> AllExercises { get; } = new();

    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Gender { get; set; }
        public decimal Height { get; set; }
        public string? FitnessLevel { get; set; }
        public string? Goal { get; set; }
        public int WorkoutsPerWeek { get; set; }
        public int PreferredDuration { get; set; }
        public string EquipmentCsv { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Equipment
        {
            get => string.IsNullOrEmpty(EquipmentCsv)
                ? new List<string>()
                : EquipmentCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            set => EquipmentCsv = string.Join(",", value);
        }
    }

    public static class UserRepository
    {
        public static List<User> Users { get; } = new List<User>
        {

        };
    }

    public class WorkoutProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ExerciseCard> Exercises { get; set; } = new();
    }

    public static class ProgramRepository
    {
        public static List<WorkoutProgram> Programs { get; } = new();

        static ProgramRepository()
        {
            // Пример программы "Новичок: Набор массы"
            var beginnerProgram = new WorkoutProgram
            {
                Name = "🔥 Новичок: Набор массы",
                Description = "3 тренировки в неделю. Акцент на базовые упражнения.",
                Exercises = new List<ExerciseCard>
                {
                    new ExerciseCard { Name = "Приседания со штангой", Description = "Базовое упражнение на ноги", Sets = 4, Reps = 8, Weight = 60 },
                    new ExerciseCard { Name = "Жим лёжа", Description = "Базовое упражнение на грудь", Sets = 4, Reps = 8, Weight = 50 },
                    new ExerciseCard { Name = "Тяга штанги в наклоне", Description = "Упражнение на спину", Sets = 4, Reps = 10, Weight = 55 }
                }
            };

            // Пример программы "Сушка"
            var cutProgram = new WorkoutProgram
            {
                Name = "⚡ Сушка за 4 недели",
                Description = "Кардио + силовые. Дефицит калорий.",
                Exercises = new List<ExerciseCard>
                {
                    new ExerciseCard { Name = "Бёрпи", Description = "Кардио + силовая", Sets = 4, Reps = 15, Weight = 0 },
                    new ExerciseCard { Name = "Планка", Description = "Удержание кора", Sets = 3, Reps = 0, Weight = 0 },
                    new ExerciseCard { Name = "Выпады с гантелями", Description = "Ноги + кардио", Sets = 3, Reps = 12, Weight = 10 }
                }
            };

            Programs.Add(beginnerProgram);
            Programs.Add(cutProgram);
        }
    }
    public class AppDbContext : DbContext
    {
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // База данных будет сохранена в файл workout.db в папке приложения
            optionsBuilder.UseSqlite("Data Source=workout_app.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Можно оставить пустым, если нет сложных конфигураций
            base.OnModelCreating(modelBuilder);
        }

    }
}

    



