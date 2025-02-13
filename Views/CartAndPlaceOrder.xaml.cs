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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Stationer;
using Stationer.StationerDataSetTableAdapters;
using static System.Net.Mime.MediaTypeNames;
using static Stationer.StationerDataSet;

namespace Stationer
{
    /// <summary>
    /// Логика взаимодействия для CartAndPlaceOrder.xaml
    /// </summary>
    public partial class CartAndPlaceOrder : Page
    {
        bool orderDetails = false;
        int? order_number;
        StationerDataSet stationerDataSet;
        Stationer.StationerDataSetTableAdapters.productTableAdapter stationerDataSetproductTableAdapter;
        Stationer.StationerDataSetTableAdapters.product_cartTableAdapter stationerDataSetproduct_cartTableAdapter;
        Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetuserTableAdapter;
        Stationer.StationerDataSetTableAdapters.orderTableAdapter stationerDataSetorderTableAdapter;
        bool emptyCart;
        public CartAndPlaceOrder(MainWindow mainWindow, bool orderDetails = false, int? order_number = null, string dateTime = null, int? totalQuantity = null, string totalPrice = null)
        {
            InitializeComponent();
            this.orderDetails = orderDetails;
            this.order_number = order_number;
            dateTimeTextBox.Text = dateTime;
            quantityTextBox.Text = totalQuantity.ToString();
            if (totalPrice != null)
            {
                priceTextBox.Text = totalPrice;
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseQuantity(sender, e, false);
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseQuantity(sender, e, true);
        }

        private void IncreaseOrDecreaseQuantity(object sender, RoutedEventArgs e, bool increase)
        {
            e.Handled = true;
            var button = sender as Button;
            var stackPanel = button.Parent as StackPanel;
            int quantity = ProductChoose.IncreaseOrDecreaseQuantity(sender, increase);
            if (quantity == -1)
            {
                return;
            }
            stationerDataSetproduct_cartTableAdapter.ChangeQuantityInCart(quantity, stackPanel.Tag.ToString(), (int)MainWindow.cart_id);
            MainWindow.mainWindow.UpdateCartSum();
            FillProductCart();
        }

        private void DeleteFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var stackPanel = button.Parent as StackPanel;
            stationerDataSetproduct_cartTableAdapter.DeleteFromCart((int)MainWindow.cart_id, stackPanel.Tag.ToString());
            FillProductCart();
            if (stationerDataSet.product.Rows.Count > 0)
            {
                EmptyCartTextlock.Visibility = Visibility.Collapsed;
            }
            MainWindow.mainWindow.UpdateCartSum();
            e.Handled = true;
        }

        private void Product_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Frame.Navigate(new ProductDetails((sender as Button).Tag.ToString()));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            MainWindow.paymentMethods(paymentMethodComboBox);
            MainWindow.deliveryMethods(deliveryMethodComboBox);
            List<string> deliveryTypes = new List<string>()
            {
                "Адресна доставка",
                "Номер відділення"
            };
            deliveryTypeComboBox.ItemsSource = deliveryTypes;

            if (MainWindow.cart_id == null)
            {
                cartEmpty();
                return;
            }
            stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            stationerDataSetproductTableAdapter = new Stationer.StationerDataSetTableAdapters.productTableAdapter();
            stationerDataSetproduct_cartTableAdapter = new Stationer.StationerDataSetTableAdapters.product_cartTableAdapter();
            if (orderDetails)
            {
                FillOrder();
            }
            else
            {
                FillPersonalData();
                FillProductCart();

                dateTimeTextBox.Visibility = Visibility.Collapsed;
                dateTimeTextBlock.Visibility = Visibility.Collapsed;
            }


            if (stationerDataSet.product.Rows.Count == 0)
            {
                cartEmpty();
            }

            System.Windows.Data.CollectionViewSource productViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource")));
            productViewSource.View.MoveCurrentToFirst();
        }

        private void FillOrder()
        {
            stationerDataSetorderTableAdapter = new Stationer.StationerDataSetTableAdapters.orderTableAdapter();

            StationerDataSet.orderDataTable orderRows = new StationerDataSet.orderDataTable();
            stationerDataSetorderTableAdapter.FillByOrderNumber(orderRows, (int)order_number);
            fullNameTextBox.Text = orderRows.Rows[0]["full_name"].ToString();
            phoneTextBox.Text = orderRows.Rows[0]["phone"].ToString();
            emailTextBox.Text = orderRows.Rows[0]["email"].ToString();
            deliveryMethodComboBox.Text = orderRows.Rows[0]["delivery_method"].ToString();
            paymentMethodComboBox.Text = orderRows.Rows[0]["payment_method"].ToString();
            deliveryTypeComboBox.Text = orderRows.Rows[0]["delivery_type"].ToString();
            commentTextBox.Text = orderRows.Rows[0]["order_comment"].ToString();
            cityTextBox.Text = orderRows.Rows[0]["city"].ToString();
            apartmentNumberTextBox.Text = orderRows.Rows[0]["apartment_number"].ToString();
            houseNumberTextBox.Text = orderRows.Rows[0]["house_number"].ToString();
            regionTextBox.Text = orderRows.Rows[0]["region"].ToString();
            streetTextBox.Text = orderRows.Rows[0]["street"].ToString();
            branchNumberTextBox.Text = orderRows.Rows[0]["branch_number"].ToString();
            orderStatusTextBox.Text = orderRows.Rows[0]["order_status"].ToString();
            orderStatusTextBox.Visibility = Visibility.Visible;
            orderStatusTextBlock.Visibility = Visibility.Visible;
            stationerDataSetproductTableAdapter.FillByOrderNumber(stationerDataSet.product, (int)order_number);

            PlaceOrder.Visibility = Visibility.Collapsed;
            orderTextBlock.Text = $"Замовленна за номером {order_number}";

            disableOrderChange();
        }

        private void FillProductCart()
        {
            stationerDataSetproductTableAdapter.FillByCart(stationerDataSet.product, (int)MainWindow.cart_id);

            Stationer.StationerDataSetTableAdapters.cartTableAdapter cartTableAdapter = new StationerDataSetTableAdapters.cartTableAdapter();

            priceTextBox.Text = cartTableAdapter.CartSum((int)MainWindow.cart_id).ToString();
            quantityTextBox.Text = cartTableAdapter.CartQuantity((int)MainWindow.cart_id).ToString();
            if (priceTextBox.Text == "")
            {
                priceTextBox.Text = quantityTextBox.Text = "0";
            }
        }

        private void FillPersonalData()
        {
            if (MainWindow.user_id != null)
            {
                stationerDataSetuserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
                StationerDataSet.userDataTable userRows = new StationerDataSet.userDataTable();
                stationerDataSetuserTableAdapter.FillBy(userRows, (int)MainWindow.user_id);
                fullNameTextBox.Text = userRows.Rows[0]["full_name"].ToString();
                phoneTextBox.Text = userRows.Rows[0]["phone"].ToString();
                emailTextBox.Text = userRows.Rows[0]["email"].ToString();
                deliveryMethodComboBox.Text = userRows.Rows[0]["delivery_method"].ToString();
                paymentMethodComboBox.Text = userRows.Rows[0]["payment_method"].ToString();
            }
        }

        private void cartEmpty()
        {
            emptyCart = true;
            EmptyCartTextlock.Visibility = Visibility.Visible;
            createCartPdf.Visibility = Visibility.Collapsed;
        }

        private void deliveryMethodTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox.SelectedValue.ToString() == "Транспортом компанії")
            {
                showOrHideAddress(true);

                deliveryTypeTextBlock.Visibility = Visibility.Collapsed;
                deliveryTypeComboBox.Visibility = Visibility.Collapsed;
            }
            else if (comboBox.SelectedValue.ToString() == "Нова пошта")
            {
                deliveryTypeComboBox.Visibility = Visibility.Visible;
                deliveryTypeTextBlock.Visibility = Visibility.Visible;

                showOrHideAddress(false);
            }
            else if (comboBox.SelectedValue.ToString() == "Самовивіз з пункту видачі")
            {
                deliveryTypeTextBlock.Visibility = Visibility.Collapsed;
                deliveryTypeComboBox.Visibility = Visibility.Collapsed;
                showOrHideAddress(false);
            }
        }

        private void disableOrderChange()
        {
            dateTimeTextBox.IsReadOnly = true;
            priceTextBox.IsReadOnly = true;
            quantityTextBox.IsReadOnly = true;
            fullNameTextBox.IsReadOnly = true;
            phoneTextBox.IsReadOnly = true;
            emailTextBox.IsReadOnly = true;
            apartmentNumberTextBox.IsReadOnly = true;
            streetTextBox.IsReadOnly = true;
            houseNumberTextBox.IsReadOnly = true;
            branchNumberTextBox.IsReadOnly = true;
            regionTextBox.IsReadOnly = true;
            cityTextBox.IsReadOnly = true;
            commentTextBox.IsReadOnly = true;
            paymentMethodComboBox.IsEditable = true;
            deliveryMethodComboBox.IsEditable = true;
            deliveryTypeComboBox.IsEditable = true;
            paymentMethodComboBox.IsReadOnly = true;
            deliveryMethodComboBox.IsReadOnly = true;
            deliveryTypeComboBox.IsReadOnly = true;
        }

        private void showOrHideAddress(bool show)
        {
            Visibility visibility;

            if (show)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }

            regionTextBlock.Visibility = visibility;
            regionTextBox.Visibility = visibility;
            cityTextBlock.Visibility = visibility;
            cityTextBox.Visibility = visibility;
            streetTextBlock.Visibility = visibility;
            streetTextBox.Visibility = visibility;
            apartmentNumberTextBlock.Visibility = visibility;
            apartmentNumberTextBox.Visibility = visibility;
            houseNumberTextBlock.Visibility = visibility;
            houseNumberTextBox.Visibility = visibility;

            branchNumberTextBlock.Visibility = Visibility.Collapsed;
            branchNumberTextBox.Visibility = Visibility.Collapsed;
        }

        private void deliveryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox.SelectedValue.ToString() == "Адресна доставка")
            {
                showOrHideAddress(true);
            }
            else if (comboBox.SelectedValue.ToString() == "Номер відділення")
            {
                cityTextBlock.Visibility = Visibility.Visible;
                cityTextBox.Visibility = Visibility.Visible;
                branchNumberTextBlock.Visibility = Visibility.Visible;
                branchNumberTextBox.Visibility = Visibility.Visible;

                regionTextBlock.Visibility = Visibility.Collapsed;
                regionTextBox.Visibility = Visibility.Collapsed;
                streetTextBlock.Visibility = Visibility.Collapsed;
                streetTextBox.Visibility = Visibility.Collapsed;
                apartmentNumberTextBlock.Visibility = Visibility.Collapsed;
                apartmentNumberTextBox.Visibility = Visibility.Collapsed;
                houseNumberTextBlock.Visibility = Visibility.Collapsed;
                houseNumberTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            if (emptyCart)
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                exceptionTextBlock.Text = "Ваш кошик пустий";
                return;
            }
            try
            {
                Stationer.StationerDataSetTableAdapters.orderTableAdapter stationerDataSetOrderTableAdapter = new Stationer.StationerDataSetTableAdapters.orderTableAdapter();
                string fullName = fullNameTextBox.Text;
                string paymentMethod = paymentMethodComboBox.Text;
                string deliveryMethod = deliveryMethodComboBox.Text;
                string deliveryType = deliveryTypeComboBox.SelectedItem?.ToString();
                string email = emailTextBox.Text;
                string phone = phoneTextBox.Text;
                string region = null;
                string city = null;
                string street = null;
                int? houseNumber = null;
                int? apartmentNumber = null;
                int? branchNumber = null;
                string comment = commentTextBox.Text;

                if (fullName == "" || paymentMethod == "" || deliveryMethod == "" || email == "" || phone == "")
                {
                    exceptionTextBlock.Visibility = Visibility.Visible;
                    exceptionTextBlock.Text = "Неправильно введені дані";
                    return;
                }

                if (comment == "")
                {
                    comment = null;
                }

                if (deliveryMethod == "Транспортом компанії" || ((deliveryMethod == "Нова пошта") && (deliveryType == "Адресна доставка")))
                {
                    region = regionTextBox.Text;
                    city = cityTextBox.Text;
                    street = streetTextBox.Text;
                    houseNumber = Convert.ToInt32(houseNumberTextBox.Text);
                    apartmentNumber = Convert.ToInt32(apartmentNumberTextBox.Text);
                }

                if (deliveryMethod == "Нова пошта" && deliveryType == "Номер відділення")
                {
                    branchNumber = Convert.ToInt32(branchNumberTextBox.Text);
                }
                int orderNumber = Convert.ToInt32(stationerDataSetOrderTableAdapter.PlaceOrder(paymentMethod, deliveryType, deliveryMethod, comment, email, region,
                city, street, apartmentNumber, houseNumber, fullName, phone, branchNumber, MainWindow.user_id, MainWindow.cart_id));
                if (orderNumber == -1)
                {
                    MessageBox.Show("Не всі обрані товари є в наявності", "Помилка при здійсненні замовлення");
                } else
                {
                    MainWindow.mainWindow.UpdateCartSum(true);
                    MainWindow.Frame.Navigate(new MessagePage(orderNumber));
                }
            }
            catch
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                exceptionTextBlock.Text = "Неправильно введені дані";
            }

        }

        private void createCommentButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow.Frame.Navigate(new CommentCreate((sender as Button).Tag.ToString(), (int)order_number));
            e.Handled = true;
        }

        private void createCartPdf_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "Кошик товарів",
                Filter = "PDF Files (*.pdf)|*.pdf",
            };

            if (orderDetails)
            {
                saveFileDialog.FileName = "Замовлення " + order_number;
            }
            else
            {
                saveFileDialog.FileName = "Кошик товарів";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                createPdf(filePath);
            }

        }

        private void createPdf(string filePath)
        {
            PdfDocument pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "Кошик товарів";
            PdfPage page = new PdfPage();
            pdfDocument.AddPage(page);

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont commonFont = new XFont("Times New Roman", 8);
            XFont font = commonFont;
            XFont boldFont = new XFont("Times New Roman", 8, XFontStyleEx.Bold);
            XFont titleFont = new XFont("Times New Roman", 12, XFontStyleEx.Bold);
            double x = 50;
            double y = 50;
            double lineHeight = 20;
            double maxWidth = page.Width - 100;
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Center;

            FillOrderOrCartDetails(gfx, font, ref x, ref y, page, lineHeight, format, titleFont);

            List<(string, string)> productsDetails = new List<(string, string)>
    {
        ("Назва товару: ", "name"),
        ("Артикул товару: ", "article"),
        ("Ціна: ", "formattedPrice_discount"),
        ("Знижка: ", "formattedDiscount_rate"),
        ("Стара ціна: ", "formattedPrice"),
        ("Загальна ціна: ", "totalPrice"),
        ("Кількість: ", "quantity"),
        ("Торгова марка: ", "brand"),
    };

            productDataTable productRows = new productDataTable();
            if (orderDetails)
            {
                stationerDataSetproductTableAdapter.FillByOrderNumber(productRows, (int)order_number);
            }
            else
            {
                stationerDataSetproductTableAdapter.FillByCart(productRows, (int)MainWindow.cart_id);
            }

            gfx.DrawString("Інформація про товари", titleFont, XBrushes.Black, new XPoint(page.Width / 2, y), format);
            y += lineHeight * 2;

            bool withoutDiscount = false;
            for (int i = 0; i < productRows.Count; i++)
            {
                font = boldFont;
                foreach ((string title, string columnName) in productsDetails)
                {
                    if (withoutDiscount)
                    {
                        withoutDiscount = false;
                        continue;
                    }
                    if (columnName != "")
                    {
                        string value = productRows.Rows[i][columnName].ToString();
                        if (columnName != "formattedDiscount_rate" || value != "")
                        {
                            if (y + font.GetHeight() > page.Height - 50)
                            {
                                page = pdfDocument.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                y = 50;
                            }

                            DrawWrappedText(gfx, title, value, font, ref x, ref y, maxWidth, page);
                        }
                        else
                        {
                            withoutDiscount = true;
                        }
                    }
                    font = commonFont;
                }
                y += 20;
            }

            pdfDocument.Save(filePath);
        }

        private void DrawWrappedText(XGraphics gfx, string propertyName, string propertyValue, XFont font, ref double x, ref double y, double maxWidth, PdfPage page)
        {
            gfx.DrawString(propertyName, font, XBrushes.Black, new XPoint(x, y));
            int indent = ((int)maxWidth / 2);
            gfx.DrawString(propertyValue, font, XBrushes.Black, new XPoint(indent, y));
            y += 20;
        }

        private void FillOrderOrCartDetails(XGraphics gfx, XFont font, ref double x, ref double y, PdfPage page, double lineHeight, XStringFormat format, XFont titleFont)
        {
            string pageTitle;
            double maxWidth = page.Width - 100;
            List<(string, string)> orderOrCartDetails;

            if (orderDetails)
            {
                pageTitle = "ЗВІТ ЗАМОВЛЕННЯ";
                orderOrCartDetails = new List<(string, string)>
        {
            ("Номер замовлення: ", order_number.ToString()),
            (dateTimeTextBlock.Text, dateTimeTextBox.Text),
            (priceTextBlock.Text, priceTextBox.Text),
            (quantityTextBlock.Text, quantityTextBox.Text),
            (fullNameTextBlock.Text, fullNameTextBox.Text),
            (phoneTextBlock.Text, phoneTextBox.Text),
            (emailTextBlock.Text, emailTextBox.Text),
            (delivertMethodTextBlock.Text, deliveryMethodComboBox.Text),
            (deliveryTypeTextBlock.Text, deliveryTypeComboBox.Text),
            (paymentMethodTextBlock.Text, paymentMethodComboBox.Text),
            (commentTextBlock.Text, commentTextBox.Text),
            (regionTextBlock.Text, regionTextBox.Text),
            (cityTextBlock.Text, cityTextBox.Text),
            (streetTextBlock.Text, streetTextBox.Text),
            (houseNumberTextBlock.Text, houseNumberTextBox.Text),
            (apartmentNumberTextBlock.Text, apartmentNumberTextBox.Text),
            (branchNumberTextBlock.Text, branchNumberTextBox.Text)
        };
            }
            else
            {
                pageTitle = "КОШИК";

                orderOrCartDetails = new List<(string, string)>
        {
            (priceTextBlock.Text, priceTextBox.Text),
            (quantityTextBlock.Text, quantityTextBox.Text),
                };
            }

            gfx.DrawString(pageTitle, titleFont, XBrushes.Black, new XPoint(page.Width / 2, y), format);
            y += lineHeight * 2;

            foreach ((string title, string value) in orderOrCartDetails)
            {
                if (value != "")
                {
                    DrawWrappedText(gfx, title, value, font, ref x, ref y, maxWidth, page);
                }
            }
        }
    }
}