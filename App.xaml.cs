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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загружаем пользователя из файла
            CurrentUser = User.LoadFromJson();

            // Если нет — создаём дефолтного
            if (CurrentUser == null)
            {
                CurrentUser = new User
                {
                    Username = "1",
                    Age = 25,
                    Height = 175.0m,
                    Weight = 68.0m,
                    Gender = "Мужской",
                    FitnessLevel = "Новичок",
                    Goal = "Набор массы",
                    Equipment = new List<string> { "Гантели", "Штанга" },
                    WorkoutsPerWeek = 3,
                    PreferredDuration = 45
                };
            }
        }



    }

}
