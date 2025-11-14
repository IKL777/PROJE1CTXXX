using Class1;
using PR;
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
            public List<Exercise> Exercises { get; set; } = new List<Exercise>();
        }

        public WorkoutProgramsWindow()
        {
            InitializeComponent();
            LoadPrograms();
        }
        private void LoadPrograms()
        {
            var programs = ProgramRepository.Programs.Select(p => new GotovieKompleksi.ProgramItem
            {
                Name = p.Name,
                Description = p.Description,
                Exercises = p.Exercises
            }).ToList();

            ProgramsList.ItemsSource = programs;
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GotovieKompleksi.ProgramItem program)
            {
                var detailsWindow = new GotovieKompleksi(program);
                detailsWindow.ShowDialog();
            }
        }

    }
}
