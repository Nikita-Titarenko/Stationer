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
    /// Логика взаимодействия для login.xaml
    /// </summary>
    public partial class login : Page
    {
        MainWindow mainWindow;
        public login(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.Login(loginTextBox.Text, passwordTextBox.Text))
            {
                mainWindow.UpdateCartSum();
                MainWindow.Frame.Navigate(new ProductChoose(-1, null, true));
            } else
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                exceptionTextBlock.Text = "Неправильний логін або пароль";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Frame.Navigate(new register(mainWindow));
        }
    }
}
