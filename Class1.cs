using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;

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
        public int UserId { get; set; }
        public User User { get; set; }

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

    public class Exercise : INotifyPropertyChanged
    {
        private int _sets;
        private int _reps;
        private double _weight;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int Sets
        {
            get => _sets;
            set { _sets = value; OnPropertyChanged(); }
        }

        public int Reps
        {
            get => _reps;
            set { _reps = value; OnPropertyChanged(); }
        }

        public double Weight
        {
            get => _weight;
            set { _weight = value; OnPropertyChanged(); }
        }

        public int? WorkoutId { get; set; }
        public Workout? Workout { get; set; }

        public string DisplayText => $"{Name} - {Sets}x{Reps} @ {Weight}kg";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        public decimal Weight { get; set; }

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

            // Явно настраиваем отношения с каскадным удалением
            modelBuilder.Entity<Workout>()
                .HasMany(w => w.Exercises)
                .WithOne(e => e.Workout)
                
                .HasForeignKey(e => e.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade); // ← Каскадное удаление при удалении тренировки

            modelBuilder.Entity<Workout>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId); // ← СВЯЗЬ С ПОЛЬЗОВАТЕЛЕМ

            // Указываем, что упражнение может существовать без тренировки
            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Workout)
                .WithMany(w => w.Exercises)
                .HasForeignKey(e => e.WorkoutId)
                .IsRequired(false); // ← Внешний ключ необязательный
        }
    }

    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue.ToString();
            if (value is double doubleValue)
                return doubleValue.ToString("0.##"); // 2 знака после запятой
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (int.TryParse(strValue, out int intResult))
                    return intResult;
                if (double.TryParse(strValue, out double doubleResult))
                    return doubleResult;
            }
            return 0;
        }
    }
    public class WeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double weight)
                return weight.ToString("0.##");
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, out double result))
            {
                // Валидация диапазона
                if (result < 0)
                    throw new FormatException("Вес не может быть отрицательным");
                if (result > 500)
                    throw new FormatException("Максимальный вес: 500 кг");
                return result;
            }
            throw new FormatException("Введите число");
        }
    }

}