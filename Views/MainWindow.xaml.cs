using System;
using System.Net;
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
using static Stationer.StationerDataSet;

namespace Stationer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Frame Frame;

        public static string localIP = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ?.ToString();

        public static int? cart_id;

        public static int? user_id;

        public static MainWindow mainWindow;

        public static Stationer.StationerDataSet stationerDataSet;

        public MainWindow()
        {
            mainWindow = this;
            InitializeComponent();
            Frame = mainFrame;
            Frame.Navigate(new ProductChoose(-1, null, true));
        }

        public static void paymentMethods(ComboBox comboBox)
        {
            List<string> paymentMethods = new List<string>{
                "Безготівковий розрахунок",
                "Накладний платіж",
                "Готівкою",
                "Онлайн оплата"
            };

            comboBox.ItemsSource = paymentMethods;
        }

        public static void deliveryMethods(ComboBox comboBox)
        {
            List<string> deliveryMethods = new List<string>{
                "Нова пошта",
                "Самовивіз з пункту видачі",
                "Транспортом компанії"
            };

            comboBox.ItemsSource = deliveryMethods;
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            var tag = b.Tag;
            mainFrame.Navigate(new SubcategoryChoose((int)tag, this));
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new CartAndPlaceOrder(this));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            // Загрузить данные в таблицу category. Можно изменить этот код как требуется.
            Stationer.StationerDataSetTableAdapters.categoryTableAdapter stationerDataSetcategoryTableAdapter = new Stationer.StationerDataSetTableAdapters.categoryTableAdapter();
            stationerDataSetcategoryTableAdapter.Fill(stationerDataSet.category);
            System.Windows.Data.CollectionViewSource categoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("categoryViewSource")));
            categoryViewSource.View.MoveCurrentToFirst();
            GetCartID();
        }

        public void GetCartID()
        {
            Stationer.StationerDataSetTableAdapters.cartTableAdapter stationerDataSetcartTableAdapter = new Stationer.StationerDataSetTableAdapters.cartTableAdapter();
            cart_id = stationerDataSetcartTableAdapter.GetCartID(localIP);
            if (cart_id != null)
            {
                UpdateCartSum();
                GetUserID();
            }
        }

        public void GetUserID()
        {
            Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetUserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
            StationerDataSet.userDataTable userRows = new StationerDataSet.userDataTable();
            stationerDataSetUserTableAdapter.GetUser(userRows, (int)cart_id);
            if (userRows.Count > 0)
            {
                user_id = Convert.ToInt32(userRows.Rows[0]["user_id"]);
                name.Text = userRows.Rows[0]["full_name"].ToString();
                login_button.Visibility = Visibility.Collapsed;
                account_button.Visibility = Visibility.Visible;
            }
        }

        private void LoginWindowCreate(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new login(this));
        }

        public bool Login(string username, string password)
        {
            Stationer.StationerDataSet stationerDataSet = MainWindow.stationerDataSet;
            Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetUserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
            StationerDataSet.userDataTable userRows = new StationerDataSet.userDataTable();
            stationerDataSetUserTableAdapter.Login(userRows, username, password, cart_id, localIP);
            if (userRows.Count > 0)
            {
                login_button.Visibility = Visibility.Collapsed;
                account_button.Visibility = Visibility.Visible;
                cart_id = Convert.ToInt32(userRows.Rows[0]["cart_id"]);
                user_id = Convert.ToInt32(userRows.Rows[0]["user_id"]);
                name.Text = userRows.Rows[0]["full_name"].ToString();
                return true;
            }
            return false;
        }

        public void ChangeName(string name)
        {
            login_button.Visibility = Visibility.Collapsed;
            account_button.Visibility = Visibility.Visible;
            this.name.Text = name;
        }

        public void ShowMenu(object sender, RoutedEventArgs e)
        {
            menuPopup.IsOpen = true;
        }

        public void HideMenu(object sender, RoutedEventArgs e)
        {
            if (!menuPopup.IsMouseOver && !account_button.IsMouseOver)
            {
                menuPopup.IsOpen = false;
            }
        }

        private void personalDataButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(new personalDataChange());
            menuPopup.IsOpen = false;
        }

        private void MyOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(new orders());
            menuPopup.IsOpen = false;
        }

        public static void CheckCart()
        {
            if (cart_id == null)
            {
                Stationer.StationerDataSetTableAdapters.cartTableAdapter stationerDataSetcartTableAdapter = new Stationer.StationerDataSetTableAdapters.cartTableAdapter();
                cart_id = Convert.ToInt32(stationerDataSetcartTableAdapter.CreateCart(MainWindow.localIP));
            }
        }

        public void UpdateCartSum(bool orderPlace = false)
        {
            if (orderPlace)
            {
                CartSumTextBlock.Text = "0 грн";
            } else
            {
                Stationer.StationerDataSetTableAdapters.cartTableAdapter stationerDataSetcartTableAdapter = new Stationer.StationerDataSetTableAdapters.cartTableAdapter();
                CartSumTextBlock.Text = stationerDataSetcartTableAdapter.CartSum((int)cart_id).ToString();
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(new ProductChoose(-1, searchTextBox.Text));
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Frame.Navigate(new ProductChoose(-1, searchTextBox.Text));
            }
        }

        private void MainPageButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(new ProductChoose(-1, null, true));
        }
    }
}
