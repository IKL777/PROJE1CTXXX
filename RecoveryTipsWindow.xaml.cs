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
    /// Логика взаимодействия для RecoveryTipsWindow.xaml
    /// </summary>
    public partial class RecoveryTipsWindow : Window
    {
        private User _currentUser;
        public RecoveryTipsWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            CalculateAndDisplayMacros();
        }
        private void CalculateAndDisplayMacros()
        {
            if (_currentUser == null)
            {
                CaloriesText.Text = "Данные пользователя не загружены";
                return;
            }

            try
            {
                double heightInCm = (double)_currentUser.Height;
                double weightInKg = (double)_currentUser.Weight;
                // 1. Расчёт базового метаболизма (формула Миффлина-Сан Жеора)
                double bmr;
                if (_currentUser.Gender?.ToLower() == "мужской" || _currentUser.Gender?.ToLower() == "male")
                {
                    bmr = 10 * weightInKg + 6.25 * heightInCm - 5 * _currentUser.Age + 5;
                }
                else
                {
                    bmr = 10 * weightInKg + 6.25 * heightInCm - 5 * _currentUser.Age - 161;
                }

                // 2. Коэффициент активности
                double activityFactor = _currentUser.WorkoutsPerWeek switch
                {
                    0 => 1.2,
                    1 => 1.375,
                    2 => 1.375,
                    3 => 1.55,
                    4 => 1.55,
                    5 => 1.725,
                    6 => 1.725,
                    7 => 1.9,
                    _ => 1.55 // значение по умолчанию
                };

                // 3. Коррекция по цели
                double goalFactor = _currentUser.Goal?.ToLower() switch
                {
                    "набрать мышечную массу" => 1.15,
                    "похудеть" => 0.85,
                    "поддерживать форму" => 1.0,
                    _ => 1.0
                };

                // 4. Итоговые калории
                double calories = bmr * activityFactor * goalFactor;
                calories = Math.Round(calories / 50) * 50; // округляем до 50 ккал

                // 5. Расчёт макронутриентов
                double protein = Math.Round(weightInKg * 1.8); // 1.8г белка на кг веса
                double fat = Math.Round(weightInKg * 0.9); // 0.9г жиров на кг веса
                double proteinCalories = protein * 4;
                double fatCalories = fat * 9;
                double carbsCalories = calories - proteinCalories - fatCalories;
                double carbs = Math.Round(carbsCalories / 4);

                if (carbsCalories < 0)
                {
                    // Сначала снижаем жиры
                    fat = Math.Round(weightInKg * 0.7);
                    fatCalories = fat * 9;
                    carbsCalories = calories - proteinCalories - fatCalories;

                    if (carbsCalories < 0)
                    {
                        // Затем снижаем белки
                        protein = Math.Round(weightInKg * 1.5);
                        proteinCalories = protein * 4;
                        carbsCalories = calories - proteinCalories - fatCalories;

                        if (carbsCalories < 0)
                        {
                            // Крайний случай — углеводы = 0
                            carbsCalories = 0;
                            // Можно добавить лог или предупреждение в интерфейс
                        }
                    }
                }

                // 6. Отображение результатов
                CaloriesText.Text = $"{(int)calories} ккал/день";
                ProteinText.Text = $"{(int)protein} г/день";
                FatText.Text = $"{(int)fat} г/день";
                CarbsText.Text = $"{(int)carbs} г/день";
            }
            catch (Exception ex)
            {
                CaloriesText.Text = $"Ошибка расчёта: {ex.Message}";
                ProteinText.Text = "";
                FatText.Text = "";
                CarbsText.Text = "";
            }
        }
    }
}
