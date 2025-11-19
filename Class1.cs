using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;



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
        public static string DataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PROJECT", "user.json");

        public static User LoadFromJson()
        {
            if (!File.Exists(DataFilePath)) return null;

            string json = File.ReadAllText(DataFilePath);
            return JsonConvert.DeserializeObject<User>(json);
        }

        public void SaveToJson()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DataFilePath));
            string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(DataFilePath, json);
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
                Name = "Калистеника",
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
                    new Exercise { Name = "Планка", Description = "Удержание кора", Sets = 3, Reps = 10, Weight = 0 },
                    new Exercise { Name = "Выпады с гантелями", Description = "Ноги + кардио", Sets = 3, Reps = 12, Weight = 10 }
                }
            };

            var TyaniTolkai_den_1 = new WorkoutProgram
            {
                Name = "Тяни-толкай для дома день 1",
                Description = "Тренировка: грудь и трицепс",
                Exercises = new List<Exercise>
                {
                    new Exercise {Name="Отжимания от пола ", Description="Грудь / Трицепс", Sets=3, Reps=12, Weight=0},
                    new Exercise {Name="Узкие отжимания (АЛМАЗНЫЕ)", Description="Трицепс / Средняя часть грудной мышцы / Передние дельты / локтевой состав", Sets=3, Reps=12, Weight=0},
                    new Exercise {Name="Обратные отжимания", Description="трицепсы / нижняя часть груди", Sets=3 , Reps=12 , Weight=0 }
                }
            };

            var TyaniTolkai_den_2 = new WorkoutProgram
            {
                Name = "Тяни-толкай для дома день 2",
                Description = "Тренировка: ноги",
                Exercises = new List<Exercise>
                {
                    new Exercise {Name="Приседания «пистолетик» (пистолеты) ", Description="Квадрицепс(передняя поверхность бедра) + Ягодичные мышцы(большая ягодичная)", Sets=3, Reps=10, Weight=0},
                    new Exercise {Name="Выпады назад с весом", Description="Квадрицепс (передняя поверхность бедра — особенно опорной ноги) + Ягодичные мышцы.", Sets=3, Reps=10, Weight=8},
                    new Exercise {Name="Ягодичный мостик с подъёмом на одну ногу", Description="Большая ягодичная мышца (подходы для 1 ноги)", Sets= 3, Reps=12 , Weight=16}
                }
            };

            var TyaniTolkai_den_3 = new WorkoutProgram
            {
                Name = "Тяни-толкай для дома день 3",
                Description = "Тренировка: дельты.",
                Exercises = new List<Exercise>
                {
                    new Exercise {Name="Подъёмы рук в стороны с гантелями", Description="Средние пучки дельтовидной мышцы", Sets=3, Reps=15, Weight=8},
                    new Exercise {Name="Армейский жим стоя гантелями", Description="Все три пучка дельтовидной мышцы (передний, средний, задний)", Sets=3, Reps=10, Weight=8},
                    new Exercise {Name = "Отведение рук назад", Description = "Мышцы верхней части спины (малая и большая круглые мышцы, ромбовидные) / Трапеция (нижняя часть).", Sets=3, Reps=15, Weight=8}
                }
            };

            var TyaniTolkai_den_4 = new WorkoutProgram
            {
                Name = "Тяни-толкай для дома день 4",
                Description = "Тренировка: спина и бицепс.",
                Exercises = new List<Exercise>
                {
                    new Exercise {Name=" Подтягивания на турнике ", Description="Широчайшие мышцы спины (создают 'крылья').", Sets=3, Reps=10, Weight=0},
                    new Exercise {Name="Тяга гантелей в наклоне", Description="Широчайшие мышцы спины / Ромбовидные и трапециевидные мышцы.", Sets=3, Reps=10, Weight=8},
                    new Exercise {Name=" Сгибание рук", Description="Бицепс (двуглавая мышца плеча).", Sets=3, Reps=10,Weight=16}
                }
            };

            Programs.Add(beginnerProgram);
            Programs.Add(cutProgram);
            Programs.Add(TyaniTolkai_den_1);
            Programs.Add(TyaniTolkai_den_2);
            Programs.Add(TyaniTolkai_den_3);
            Programs.Add(TyaniTolkai_den_4);
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

    public class ProgressToColorConverter : IValueConverter //Конвертор для цвета 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string comparisonText)
            {
                if (comparisonText.Contains("↑") || comparisonText.Contains("увелич"))
                    return new SolidColorBrush(Colors.LightGreen); // Улучшение
                if (comparisonText.Contains("↓") || comparisonText.Contains("уменьш"))
                    return new SolidColorBrush(Colors.LightPink); // Ухудшение
            }
            return new SolidColorBrush(Colors.White); // Нейтральный
        }

        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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