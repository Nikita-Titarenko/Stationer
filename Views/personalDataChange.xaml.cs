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
    /// Логика взаимодействия для personalDataChange.xaml
    /// </summary>
    public partial class personalDataChange : Page
    {
        public personalDataChange()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = fullNameTextBox.Text;
            string phone = phoneTextBox.Text;
            string email = emailTextBox.Text;
            string gender = genderComboBox.Text;
            if (gender == "")
            {
                gender = null;
            }
            string paymentMethod = paymentMethodTextBox.Text;
            string deliveryMethod = deliveryMethodTextBox.Text;
            bool dateInput = false;
            DateTime birthayDate;
            if (DateTime.TryParse(birthdayDateTextBox.Text, out birthayDate))
            {
                dateInput = true;
            } else if (birthdayDateTextBox.Text != "")
            {
                birthdayDateTextBlock.Text += " (неправильна дата)";
                return;
            }
            Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetUserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
            stationerDataSetUserTableAdapter.UpdateUser(fullName, phone, gender, dateInput ? birthdayDateTextBox.Text : null, paymentMethod, deliveryMethod, email, (int)MainWindow.user_id);
            MainWindow.Frame.Navigate(new ProductChoose(-1, null, true));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.paymentMethods(paymentMethodTextBox);
            MainWindow.deliveryMethods(deliveryMethodTextBox);
            Stationer.StationerDataSet stationerDataSet = MainWindow.stationerDataSet;
            Stationer.StationerDataSetTableAdapters.userTableAdapter stationerDataSetuserTableAdapter = new Stationer.StationerDataSetTableAdapters.userTableAdapter();
            StationerDataSet.userDataTable userRows = new StationerDataSet.userDataTable();
            stationerDataSetuserTableAdapter.FillBy(userRows, (int)MainWindow.user_id);
            fullNameTextBox.Text = userRows.Rows[0]["full_name"].ToString();
            phoneTextBox.Text = userRows.Rows[0]["phone"].ToString();
            paymentMethodTextBox.Text = userRows.Rows[0]["payment_method"].ToString();
            deliveryMethodTextBox.Text = userRows.Rows[0]["delivery_method"].ToString();
            genderComboBox.Text = userRows.Rows[0]["gender"].ToString();

            if (userRows.Rows[0]["birthday_date"] != DBNull.Value)
            {
                birthdayDateTextBox.Text = Convert.ToDateTime(userRows.Rows[0]["birthday_date"]).ToString("dd.MM.yyyy");
            }
            emailTextBox.Text = userRows.Rows[0]["email"].ToString();
        }
    }
}
