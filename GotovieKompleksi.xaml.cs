using Class1;
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

namespace PR
{
    /// <summary>
    /// Логика взаимодействия для GotovieKompleksi.xaml
    /// </summary>
    public partial class GotovieKompleksi : Window
    {
        public class ProgramItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<Exercise> Exercises { get; set; } = new List<Exercise>();
        }
        public GotovieKompleksi(ProgramItem program)
        {
            InitializeComponent();
            this.DataContext = program;
        }
        
    }
}
