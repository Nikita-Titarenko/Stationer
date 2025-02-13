using System;
using System.Collections.Generic;
using System.Globalization;
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
using Stationer.StationerDataSetTableAdapters;

namespace Stationer
{
    /// <summary>
    /// Логика взаимодействия для ProductDetails.xaml
    /// </summary>
    public partial class ProductDetails : Page
    {
        string article;
        Stationer.StationerDataSet stationerDataSet;

        Stationer.StationerDataSetTableAdapters.commentTableAdapter stationerDataSetCommentTableAdapter;
        public ProductDetails(string article)
        {
            this.article = article;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            Stationer.StationerDataSetTableAdapters.productTableAdapter stationerDataSetproductTableAdapter = new Stationer.StationerDataSetTableAdapters.productTableAdapter();
            stationerDataSetproductTableAdapter.FillByArticle(stationerDataSet.product, article);
            System.Windows.Data.CollectionViewSource productViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource")));

            Stationer.StationerDataSetTableAdapters.propertyTableAdapter stationerDataSetpropertyTableAdapter = new Stationer.StationerDataSetTableAdapters.propertyTableAdapter();
            stationerDataSetpropertyTableAdapter.FillByArticle(stationerDataSet.property, article);

            stationerDataSetCommentTableAdapter = new Stationer.StationerDataSetTableAdapters.commentTableAdapter();
            stationerDataSetCommentTableAdapter.FillByArticleDESC(stationerDataSet.comment, article);
        }

        private void PutInCartButton_Click(object sender, RoutedEventArgs e)
        {
            ProductChoose.PutInCart(sender, e);
        }

        private void IncreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            ProductChoose.IncreaseOrDecreaseQuantity(sender, true);
        }

        private void DecreaseQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            ProductChoose.IncreaseOrDecreaseQuantity(sender, false);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sortDirection = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag.ToString();
            if (sortDirection == "ASC")
            {
                stationerDataSetCommentTableAdapter.FillByArticleASC(stationerDataSet.comment, article);
            } else if (sortDirection == "DESC")
            {
                stationerDataSetCommentTableAdapter.FillByArticleDESC(stationerDataSet.comment, article);
            }
        }
    }

    public class RatingStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int commentRating = (int)value;

            int buttonRating = int.Parse(parameter.ToString());

            if (buttonRating <= commentRating)
            {
                return (Style)Application.Current.Resources["yellowStarButton"];
            }

            return (Style)Application.Current.Resources["whiteStarButton"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
