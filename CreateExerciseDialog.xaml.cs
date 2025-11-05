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
    /// Логика взаимодействия для CreateExerciseDialog.xaml
    /// </summary>
    public partial class CreateExerciseDialog : Window
    {
        public ExerciseCard CreatedExercise { get; private set; }
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

            if (!int.TryParse(RepsBox.Text, out int reps) || reps <= 0)
            {
                MessageBox.Show("Повторения должны быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SetsBox.Text, out int sets) || sets <= 0)
            {
                MessageBox.Show("Подходы должны быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(WeightBox.Text, out double weight) || weight < 0)
            {
                MessageBox.Show("Вес должен быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём упражнение
            CreatedExercise = new ExerciseCard
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
