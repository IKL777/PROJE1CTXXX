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
    /// Логика взаимодействия для WorkoutDetailsWindow.xaml
    /// </summary>
    public partial class WorkoutDetailsWindow : Window
    {
        public WorkoutDetailsWindow(Workout workout)
        {
            InitializeComponent();
            if (workout == null)
                return;

            if (workout == null) return;

            // Отображаем данные
            DateText.Text = $"Дата: {workout.Date:dd.MM.yyyy}";
            TypeText.Text = $"Тип: {workout.Type}";

            // Передаём упражнения в список
            ExercisesList.ItemsSource = workout.Exercises;
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
