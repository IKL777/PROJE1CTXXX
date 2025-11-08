using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Class1
{
    public enum WorkoutType { WithWeights, Bodyweight }
    public enum Difficulty { Easy, Medium, Hard }

    public class Workout
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Class1.WorkoutType Type { get; set; }

        public List<Exercise> Exercises { get; set; } = new();

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
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }

        public int? WorkoutId { get; set; }
        public Workout? Workout { get; set; } // ← ДОБАВЛЕНО НАВИГАЦИОННОЕ СВОЙСТВО

        public string DisplayText => $"{Name} - {Sets}x{Reps} @ {Weight}kg";
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

    public class WorkoutProgram
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
    }

    public static class ProgramRepository
    {
        public static List<WorkoutProgram> Programs { get; } = new();

        static ProgramRepository()
        {
            var beginnerProgram = new WorkoutProgram
            {
                Name = "🔥 Новичок: Набор массы",
                Description = "3 тренировки в неделю. Акцент на базовые упражнения.",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Приседания со штангой", Description = "Базовое упражнение на ноги", Sets = 4, Reps = 8, Weight = 60 },
                    new Exercise { Name = "Жим лёжа", Description = "Базовое упражнение на грудь", Sets = 4, Reps = 8, Weight = 50 },
                    new Exercise { Name = "Тяга штанги в наклоне", Description = "Упражнение на спину", Sets = 4, Reps = 10, Weight = 55 }
                }
            };

            var cutProgram = new WorkoutProgram
            {
                Name = "⚡ Сушка за 4 недели",
                Description = "Кардио + силовые. Дефицит калорий.",
                Exercises = new List<Exercise>
                {
                    new Exercise { Name = "Бёрпи", Description = "Кардио + силовая", Sets = 4, Reps = 15, Weight = 0 },
                    new Exercise { Name = "Планка", Description = "Удержание кора", Sets = 3, Reps = 0, Weight = 0 },
                    new Exercise { Name = "Выпады с гантелями", Description = "Ноги + кардио", Sets = 3, Reps = 12, Weight = 10 }
                }
            };

            Programs.Add(beginnerProgram);
            Programs.Add(cutProgram);
        }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Workout> Workouts { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=workout_app.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Workout>()
                .HasMany(w => w.Exercises)
                .WithOne(e => e.Workout)
                .HasForeignKey(e => e.WorkoutId);
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue.ToString();
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && int.TryParse(strValue, out int result))
                return result;
            return 0;
        }
    }
}