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
using static PR.GotovieKompleksi;

namespace PROJECT
{
    /// <summary>
    /// Логика взаимодействия для WorkoutProgramsWindow.xaml
    /// </summary>
    public partial class WorkoutProgramsWindow : Window
    {

        public WorkoutProgramsWindow()
        {
            InitializeComponent();
            LoadPrograms();
        }
        private void LoadPrograms()
        {
            ProgramsList.ItemsSource = ProgramRepository.Programs;
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is WorkoutProgram program)
            {
                var detailsWindow = new GotovieKompleksi(program);
                detailsWindow.ShowDialog();
            }
        }

    }
}
