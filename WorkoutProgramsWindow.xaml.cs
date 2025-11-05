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
    /// Логика взаимодействия для WorkoutProgramsWindow.xaml
    /// </summary>
    public partial class WorkoutProgramsWindow : Window
    {
        public class ProgramItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public WorkoutProgramsWindow()
        {
            InitializeComponent();
            LoadPrograms();
        }
        private void LoadPrograms()
        {
            var programs = new List<ProgramItem>
            {
                new ProgramItem { Name = "Новичок: Набор массы", Description = "3 тренировки в неделю, акцент на базовые упражнения" },
                new ProgramItem { Name = "Сушка за 4 недели", Description = "Кардио + силовые, дефицит калорий" },
                new ProgramItem { Name = "Выносливость: Бег + Силовая", Description = "5 тренировок в неделю" }
            };

            ProgramsList.ItemsSource = programs;
        }
        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProgramItem program)
            {
                MessageBox.Show(program.Description, program.Name);
            }
        }
    }
}
