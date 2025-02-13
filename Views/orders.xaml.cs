using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для orders.xaml
    /// </summary>
    public partial class orders : Page
    {
        public orders()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Stationer.StationerDataSet stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            Stationer.StationerDataSetTableAdapters.orderTableAdapter stationerDataSetOrderTableAdapter = new Stationer.StationerDataSetTableAdapters.orderTableAdapter();
            stationerDataSetOrderTableAdapter.FillByUserId(stationerDataSet.order, MainWindow.user_id);
            System.Windows.Data.CollectionViewSource orderViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("orderViewSource")));
            orderViewSource.View.MoveCurrentToFirst();
        }

        public void orderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            // Знайти батьківський Grid
            Grid grid = FindParent<Grid>(button);
            if (grid != null)
            {
                var textBlocks = FindVisualChildren<TextBlock>(grid).ToList();
                var dateTimeTextBlock = textBlocks.ElementAtOrDefault(1);
                var totalQuantityTextBlock = textBlocks.ElementAtOrDefault(2);
                var totalPriceTextBlock = textBlocks.ElementAtOrDefault(3);

                if (dateTimeTextBlock != null && totalQuantityTextBlock != null && totalPriceTextBlock != null)
                {
                    string dateTime = dateTimeTextBlock.Text;
                    string totalQuantity = totalQuantityTextBlock.Text;
                    string totalPrice = totalPriceTextBlock.Text;

                    MainWindow.Frame.Navigate(new CartAndPlaceOrder(MainWindow.mainWindow, true,
                        Convert.ToInt32(button.Tag), dateTime, Convert.ToInt32(totalQuantity), totalPrice));
                }
                else
                {
                    MessageBox.Show("Не вдалося знайти дані для обраного замовлення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                {
                    yield return tChild;
                }

                foreach (T descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
