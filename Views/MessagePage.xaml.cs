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
    /// Логика взаимодействия для orderCreated.xaml
    /// </summary>
    public partial class MessagePage : Page
    {
        public MessagePage(int orderNumber)
        {
            InitializeComponent();
            messageTextBlock.Text = $"Ваше замовлення за номером {orderNumber} було успішно створено. Ви можете його переглянути у Ваших замовленнях якщо ви зареєстрований користувач.";
        }

        public MessagePage(bool commentCreated)
        {
            InitializeComponent();
            if (commentCreated) {
                messageTextBlock.Text = $"Ваше коментар був успішно створений";
            }
        }

        private void mainPageButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Frame.Navigate(new ProductChoose(-1, null, true));
        }
    }
}
