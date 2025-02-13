using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Stationer.StationerDataSetTableAdapters;

namespace Stationer
{
    /// <summary>
    /// Логика взаимодействия для ProductChoose.xaml
    /// </summary>
    /// 
    public partial class ProductChoose : Page
    {
        private List<(int subcategory_attribute_id, string value)> activeFilters = new List<(int, string)>();

        private List<string> activeBrands = new List<string>();

        Stationer.StationerDataSet stationerDataSet;

        private string substring = null;

        private const string connectionString = "Data Source=DESKTOP-I2MBG2Q;Initial Catalog=Stationer;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        private int currentPage = 0;

        private const int productQuantityOnPage = 5;

        System.Windows.Data.CollectionViewSource productViewSource;

        Stationer.StationerDataSetTableAdapters.productTableAdapter stationerDataSetproductTableAdapter;

        private int? subcategory_id;

        private Button[] pageButtons;

        private bool pageLoad = false;

        private bool productsWithDiscounts;

        private string[] sortTypeAndDirection = { "dbo.MonthlySales(p.article)", "DESC"};
        public ProductChoose(int subcategory_id, string substring = null, bool productsWithDiscounts = false)
        {
            this.subcategory_id = subcategory_id;
            InitializeComponent();
            this.substring = substring;
            this.productsWithDiscounts = productsWithDiscounts;
        }

        private void createFilterPanel()
        {
            StringBuilder query = new StringBuilder($"SELECT DISTINCT p.name AS property_name, psa.value, psa.subcategory_attribute_id, sa.property_id, " +
                $"(SELECT COUNT(DISTINCT pr.article) FROM product pr LEFT JOIN product_subcategory_attribute psa2 ON pr.article = psa2.article WHERE psa2.value = psa.value AND psa2.subcategory_attribute_id = sa.subcategory_attribute_id) AS productCount " +
                $"FROM subcategory_attribute AS sa INNER JOIN property AS p ON sa.property_id = p.property_id LEFT OUTER JOIN product_subcategory_attribute AS psa ON sa.subcategory_attribute_id = psa.subcategory_attribute_id WHERE (sa.subcategory_id = @subcategory_id) ORDER BY property_name, psa.value");
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@subcategory_id", subcategory_id));
            DataRowCollection filterRows = ExecuteSql(query, parameters).Rows;

            int lastPropertyId = -1;
            StackPanel currentStack = null;

            foreach (DataRow row in filterRows)
            {
                int propertyId = Convert.ToInt32(row["property_id"]);

                if (lastPropertyId != propertyId)
                {
                    currentStack = new StackPanel();
                    lastPropertyId = propertyId;

                    TextBlock text = new TextBlock
                    {
                        Text = row["property_name"].ToString(),
                        Style = (Style)Application.Current.Resources["textBlock"]
                    };
                    filterPanel.Children.Add(text);
                }

                AddCheckBoxToStack(row, currentStack);

                if (IsLastRowOrNextProperty(row, filterRows, lastPropertyId))
                {
                    filterPanel.Children.Add(currentStack);
                }
            }

            CreateBrandCheckBoxes();
        }

        private void AddCheckBoxToStack(DataRow row, StackPanel stack)
        {
            CheckBox checkBox = new CheckBox
            {
                Content = row["value"].ToString() + $"({row["productCount"]})",
                Tag = row["subcategory_attribute_id"],
                Style = (Style)Application.Current.Resources["checkBox"]
            };

            checkBox.Checked += attributeCheckBoxChanged;
            checkBox.Unchecked += attributeCheckBoxChanged;

            stack.Children.Add(checkBox);
        }

        private bool IsLastRowOrNextProperty(DataRow row, DataRowCollection rows, int lastPropertyId)
        {
            return rows.IndexOf(row) + 1 >= rows.Count || Convert.ToInt32(rows[rows.IndexOf(row) + 1]["property_id"]) != lastPropertyId;
        }

        private void SetPriceFilter(double min, double max)
        {
            minPrice.Text = min.ToString(CultureInfo.InvariantCulture);
            maxPrice.Text = max.ToString(CultureInfo.InvariantCulture);
            minPrice.TextChanged += minPrice_TextChanged;
            maxPrice.TextChanged += maxPrice_TextChanged;
        }

        private void CreateBrandCheckBoxes()
        {
            StationerDataSet.productDataTable productRows = new StationerDataSet.productDataTable();
            stationerDataSetproductTableAdapter.FillDistinctBrand(productRows, subcategory_id);

            foreach (DataRow row in productRows)
            {
                CheckBox check = new CheckBox
                {
                    Content = row["brand"].ToString(),
                    Style = (Style)Application.Current.Resources["checkBox"]
                };
                brachCheckBoxes.Children.Add(check);
                check.Checked += brandCheckBoxChaged;
                check.Unchecked += brandCheckBoxChaged;
            }
        }

        private DataTable ExecuteSql(StringBuilder query, List<SqlParameter> parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query.ToString(), connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable result = new DataTable();
                adapter.Fill(result);
                return result;
            }
        }

        private void attributeCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.IsChecked.Value)
            {
                activeFilters.Add((Convert.ToInt32(checkBox.Tag), checkBox.Content.ToString().Substring(0, checkBox.Content.ToString().IndexOf("("))));
            } else
            {
                activeFilters.Remove((Convert.ToInt32(checkBox.Tag), checkBox.Content.ToString().Substring(0, checkBox.Content.ToString().IndexOf("("))));
            }
            if (pageLoad)
            {
                displayProducts();
            }
        }

        private void brandCheckBoxChaged(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.IsChecked.Value)
            {
                activeBrands.Add(checkBox.Content.ToString());
            }
            else
            {
                activeBrands.Remove(checkBox.Content.ToString());
            }
            if (pageLoad) {
                displayProducts();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePageData();
            displayProducts();
            pageLoad = true;
        }

        private void InitializePageData()
        {
            stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            StationerDataSet.subcategoryDataTable subcategoryRows = new StationerDataSet.subcategoryDataTable();
            Stationer.StationerDataSetTableAdapters.subcategoryTableAdapter subcategoryTableAdapter = new Stationer.StationerDataSetTableAdapters.subcategoryTableAdapter();
            subcategoryTableAdapter.FillBySubcategory(subcategoryRows, (int)subcategory_id);

            stationerDataSetproductTableAdapter = new Stationer.StationerDataSetTableAdapters.productTableAdapter();
            productViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource")));

            if (productsWithDiscounts)
            {
                HandleDiscountedProducts(subcategoryRows);
                return;
            }

            if (substring == null)
            {
                HandleSubcategoryProducts(subcategoryRows);
            }
            else
            {
                HandleSearchResults(subcategoryRows);
            }
        }

        private void HandleDiscountedProducts(StationerDataSet.subcategoryDataTable subcategoryRows)
        {
            filterPanel.Visibility = Visibility.Collapsed;
            sortPanel.Visibility = Visibility.Collapsed;
            titleTextBlock.Text = "Акційні товари";

            int pageCount = PageCountCalculate((int)stationerDataSetproductTableAdapter.CountProductsByDiscounts());
            CreatePageButtons(pageCount);
        }

        private void HandleSubcategoryProducts(StationerDataSet.subcategoryDataTable subcategoryRows)
        {
            titleTextBlock.Text = subcategoryRows[0]["name"].ToString();
            subcategoryDescriptionTextBlock.Text = subcategoryRows[0]["description"].ToString();
            
            int pageCount = PageCountCalculate(Convert.ToInt32(subcategoryRows[0]["productCount"]));
            CreatePageButtons(pageCount);
            productCount.Text = " (" + subcategoryRows[0]["productCount"] + ")";
            createFilterPanel();
            SetPriceFilter(Convert.ToDouble(subcategoryRows[0]["minPrice"]), Convert.ToDouble(subcategoryRows[0]["maxPrice"].ToString()));
        }

        private void HandleSearchResults(StationerDataSet.subcategoryDataTable subcategoryRows)
        {
            titleTextBlock.Text = $"Результати пошуку '{substring}' у всіх категоріях";
            filterPanel.Visibility = Visibility.Collapsed;

            int pageCount = PageCountCalculate((int)stationerDataSetproductTableAdapter.CountProductsBySubstring(substring));
            CreatePageButtons(pageCount);
        }

        private int PageCountCalculate(int productCount)
        {
            return Convert.ToInt32(Math.Ceiling((Convert.ToDouble(productCount) / productQuantityOnPage)));
        }

        private void CreatePageButtons(int pageCount)
        {
            pageButtons = new Button[pageCount];
            for (int i = 0; i < pageCount; i++) {
                Button button = new Button();
                button.Content = i + 1;
                button.FontSize = 30;
                button.Width = 50;
                button.Height = 50;
                button.Background = Brushes.Transparent;
                button.Click += pageChangeButton_Click;
                pageButtonsStackPanel.Children.Add(button);
                pageButtons[i] = button;
            }
            if (pageCount != 0)
            {
                pageButtons[0].Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF018BC8"));
            }
        }

        private void pageChangeButton_Click(object sender, RoutedEventArgs e)
        {
            pageButtons[currentPage].Background = Brushes.Transparent;
            Button button = sender as Button;
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF018BC8"));
            currentPage = Convert.ToInt32(button.Content) - 1;
            displayProducts();
        }

        private void displayProducts()
        {
            StringBuilder query = new StringBuilder($"SELECT DISTINCT p.article, dbo.CheckAvailability(p.stock_quantity) AS availability, dbo.MonthlySales(p.article) AS monthly_sales, p.creation_date, p.name, p.price, dbo.PriceWithDiscount(p.price, p.discount_rate) AS price_discount, p.stock_quantity, p.initial_quantity, p.discount_rate" +
                $" FROM product AS p LEFT JOIN product_subcategory_attribute AS psa ON psa.article = p.article WHERE ");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(maxPrice.Text) && !string.IsNullOrEmpty(minPrice.Text))
            {
                query.Append("dbo.PriceWithDiscount(p.price, p.discount_rate) BETWEEN @minPrice AND @maxPrice AND ");
                parameters.Add(new SqlParameter("@minPrice", minPrice.Text));
                parameters.Add(new SqlParameter("@maxPrice", maxPrice.Text));
            }

            if (substring != null)
            {
                query.Append("UPPER(p.name) LIKE UPPER(@substring) ");
                parameters.Add(new SqlParameter("@substring", $"%{substring}%"));
            }
            else if (subcategory_id != -1)
            {
                query.Append("p.subcategory_id = @subcategory_id ");
                parameters.Add(new SqlParameter("@subcategory_id", subcategory_id));
            }
            else
            {
                query.Append("p.discount_rate IS NOT NULL ");
            }

            if (activeBrands.Count > 0 || activeFilters.Count > 0)
            {
                query.Append(" AND (");
            }

            bool firstRow = true;

            AppendBrandConditions(query, parameters, ref firstRow);
            AppendFilterConditions(query, parameters, ref firstRow);

            if (activeBrands.Count > 0 || activeFilters.Count > 0)
            {
                query.Append(")");
            }

            if (sortTypeAndDirection[0] == "dbo.MonthlySales(p.article)")
            {
                query.Append($" ORDER BY {sortTypeAndDirection[0]} {sortTypeAndDirection[1]} ");
            }
            else
            {
                query.Append($" ORDER BY p.{sortTypeAndDirection[0]} {sortTypeAndDirection[1]} ");
            }

            query.Append($" OFFSET @startIndex ROWS FETCH NEXT @productQuantity ROWS ONLY");
            parameters.Add(new SqlParameter("@startIndex", currentPage * productQuantityOnPage));
            parameters.Add(new SqlParameter("@productQuantity", productQuantityOnPage));

            productViewSource.Source = ExecuteSql(query, parameters).DefaultView;
        }

        private void AppendFilterConditions(StringBuilder query, List<SqlParameter> parameters, ref bool firstRow)
        {
            int count = 1;
            if (activeFilters.Count > 0)
            {
                foreach (var item in activeFilters)
                {
                    if (!firstRow)
                    {
                        query.Append(" OR ");
                    }

                    query.Append($"(psa.subcategory_attribute_id = @subcategory_attribute_id{count} AND psa.value = @value{count})");

                    parameters.Add(new SqlParameter("@subcategory_attribute_id" + count, item.subcategory_attribute_id));
                    parameters.Add(new SqlParameter("@value" + count++, item.value));

                    firstRow = false;
                }
            }
        }

        private void AppendBrandConditions(StringBuilder query, List<SqlParameter> parameters, ref bool firstRow)
        {
            int count = 1;
            if (activeBrands.Count > 0)
            {
                foreach (var item in activeBrands)
                {
                    if (!firstRow)
                    {
                        query.Append(" OR ");
                    }

                    query.Append($"brand = @brand{count}");

                    parameters.Add(new SqlParameter("@brand" + count++, item));

                    firstRow = false;
                }
            }
        }


        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseQuantity(sender, false);
            e.Handled = true;
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseQuantity(sender, true);
            e.Handled = true;
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Frame.Navigate(new ProductDetails((sender as Button).Tag.ToString()));
        }

        

        public void PutInCart_Click(object sender, RoutedEventArgs e)
        {
            PutInCart(sender, e);

            e.Handled = true;
        }

        public static void PutInCart(object sender, RoutedEventArgs e)
        {
            MainWindow.CheckCart();

            var button = sender as Button;

            var stackPanel = button.Parent as StackPanel;

            var quantityTextBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();

            int currentQuantity = int.Parse(quantityTextBox.Text);

            Stationer.StationerDataSetTableAdapters.product_cartTableAdapter stationerDataSetproduct_cartTableAdapter = new Stationer.StationerDataSetTableAdapters.product_cartTableAdapter();
            int addInCart = Convert.ToInt32(stationerDataSetproduct_cartTableAdapter.AddInCart((int)MainWindow.cart_id, button.Tag.ToString(), currentQuantity));
            if (addInCart != -1)
            {
                MessageBox.Show("Цього товару немає в заданій кількості. Кількість цього товару на складі: " + addInCart, "Помилка при обранні товару");
            }
            else
            {
                MainWindow.mainWindow.UpdateCartSum();
            }
        }

        public static int IncreaseOrDecreaseQuantity(object sender, bool increase)
        {
            var button = sender as Button;

            var stackPanel = button.Parent as StackPanel;


            var quantityTextBox = stackPanel.Children.OfType<TextBox>().FirstOrDefault();
            if (quantityTextBox != null)
            {
                int currentQuantity = int.Parse(quantityTextBox.Text);
                int change = increase ? 1 : -1;
                if (currentQuantity > 1 || change != -1)
                {
                    quantityTextBox.Text = (currentQuantity + change).ToString();
                    return currentQuantity + change;
                }
            }
            return -1;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sortTypeAndDirection = (((sender as ComboBox).SelectedItem as ComboBoxItem).Tag.ToString()).Split('/');
            if (pageLoad)
            {
                displayProducts();
            }
        }

        private void minPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            displayProducts();
        }

        private void maxPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            displayProducts();
        }
    }

    public class ArticleToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                string article = value.ToString();

                return new Uri($"/Images/{article}.jpg", UriKind.Relative);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null; 
        }
    }
}