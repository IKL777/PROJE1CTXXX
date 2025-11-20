using System;
using System.Windows;
using Class1;
using System.Globalization;

namespace PROJECT
{
    public partial class CreateExerciseDialog : Window
    {
        public Exercise? CreatedExercise { get; private set; } // ← было: ExerciseCard

        public CreateExerciseDialog()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название упражнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(RepsBox.Text, out int reps) || reps <= 0 || reps > 150)
            {
                MessageBox.Show("Повторения должны быть положительным числом.  И в диапазоне от 0 до 150 ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SetsBox.Text, out int sets) || sets <= 0 || sets > 40)
            {
                MessageBox.Show("Подходы должны быть положительным числом. И в диапазоне от 0 до 40", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var numberFormat = CultureInfo.CurrentCulture.NumberFormat;   // Корректная обработка разделителя для double
            if (!double.TryParse(WeightBox.Text, out double weight) || weight < 0 || weight > 600)
            {
                MessageBox.Show("Вес должен быть неотрицательным числом.  И в диапазоне от 0 до 600", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём упражнение
            CreatedExercise = new Exercise // ← было: new ExerciseCard
            {
                Name = NameBox.Text.Trim(),
                Description = DescriptionBox.Text.Trim(),
                Reps = reps,
                Sets = sets,
                Weight = weight,
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}