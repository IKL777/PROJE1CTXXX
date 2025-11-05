using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Class1;

namespace PROJECT
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Запускаем окно регистрации
            RegistrationWindow registerWindow = new RegistrationWindow();
            registerWindow.Show();
        }
        public static Cursor CustomCursor { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var uri = new Uri("/Cursor/biceps.cur");
                var stream = Application.GetResourceStream(uri)?.Stream;
                if (stream != null)
                    CustomCursor = new Cursor(stream);
            }
            catch { /* игнорировать — будет стандартный курсор */ }
        }
    }

}
