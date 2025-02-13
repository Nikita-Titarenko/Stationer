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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stationer
{
    /// <summary>
    /// Логика взаимодействия для register.xaml
    /// </summary>
    public partial class register : Page
    {
        public register(Window mainWindow)
        {
            InitializeComponent();
        }

        public void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;
            string password = passwordTextBox.Text;
            string fullName = fullNameTextBox.Text;
            string phone = phoneTextBox.Text;
            string email = emailTextBox.Text;
            if (login == "" || password == "" || fullName == "" || phone == "" || email == "")
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                exceptionTextBlock.Text = "Не всі поля заповнені";
                return;
            }
            Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetUserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
            MainWindow.CheckCart();
            MainWindow.user_id = Convert.ToInt32(stationerDataSetUserTableAdapter.Register(fullName, phone, email, login, password, MainWindow.cart_id));
            if (MainWindow.user_id == 0)
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                exceptionTextBlock.Text = "Такий логін вже існує. Введіть інший логін";
            } else
            {
                MainWindow.mainWindow.ChangeName(fullName);
                MainWindow.Frame.Navigate(new ProductChoose(-1, null, true));
            }
        }

        private void register_loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
