using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для CommentCreate.xaml
    /// </summary>
    public partial class CommentCreate : Page
    {
        private string article;

        private int order_number;

        private StationerDataSet stationerDataSet;

        StationerDataSetTableAdapters.commentTableAdapter stationerDataSetCommentTableAdapter;

        Button[] rateButtons;

        private int rating = 0;
        public CommentCreate(string article, int order_number)
        {
            this.article = article;
            this.order_number = order_number;
            InitializeComponent();
        }

        private void productButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RateButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            rating = Convert.ToInt32(button.Tag);
            ratingTextBlock.Text = "Оцінка: " + rating;
            rateButtonChange(rating);
        }

        private void rateButtonChange(int rating)
        {
            for (int i = 0; i < rating; i++)
            {
                rateButtons[i].Style = (Style)Application.Current.Resources["yellowStarButton"];
            }

            for (int i = rating; i < rateButtons.Length; i++)
            {
                rateButtons[i].Style = (Style)Application.Current.Resources["whiteStarButton"];
            }
        }

        private void RateButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            int grade = Convert.ToInt32(button.Tag);
            rateButtonChange(grade);
        }

        private void RateButton_MouseLeave(object sender, MouseEventArgs e)
        {
            rateButtonChange(rating);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            StationerDataSetTableAdapters.productTableAdapter stationerDataSetproductTableAdapter = new Stationer.StationerDataSetTableAdapters.productTableAdapter();
            stationerDataSetproductTableAdapter.FillByOrderNumberAndArticle(stationerDataSet.product, order_number, article);
            System.Windows.Data.CollectionViewSource productViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productViewSource")));

            stationerDataSetCommentTableAdapter = new Stationer.StationerDataSetTableAdapters.commentTableAdapter();
            StationerDataSet.commentDataTable commentRows = new StationerDataSet.commentDataTable();
            stationerDataSetCommentTableAdapter.FillByArticelAndOrderNumber(commentRows, article, order_number);

            rateButtons = new Button[] { rateButton1, rateButton2, rateButton3, rateButton4, rateButton5 };

            for (int i = 0; i < rateButtons.Length; i++)
            {
                rateButtons[i].MouseEnter += RateButton_MouseEnter;
                rateButtons[i].MouseLeave += RateButton_MouseLeave;
                rateButtons[i].Click += RateButton_Click;
            }

            if (commentRows.Count > 0)
            {
                commentTextBox.Text = commentRows[0]["content"].ToString();
                advantagesTextBox.Text = commentRows[0]["advantages"].ToString();
                disadvantagesTextBox.Text = commentRows[0]["disadvantages"].ToString();
                string rating = commentRows[0]["rating"].ToString();
                ratingTextBlock.Text = "Оцінка: " + rating;
                rateButtonChange(Convert.ToInt32(rating));
                createCommentButton.Visibility = Visibility.Collapsed;
                createCommentTextBlock.Text = "Ваш відгук на товар";
                commentTextBox.IsReadOnly = true;
                advantagesTextBox.IsReadOnly = true;
                disadvantagesTextBox.IsReadOnly = true;
                statButtonPanel.IsEnabled = false;
            } 
        }

        private void createCommentButton_Click(object sender, RoutedEventArgs e)
        {
            string comment = commentTextBox.Text;

            if (comment == "")
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                return;
            }

            if (rating == 0)
            {
                exceptionTextBlock.Visibility = Visibility.Visible;
                return;
            }

            
            string advantages = advantagesTextBox.Text;
            string disadvantages = disadvantagesTextBox.Text;
            advantages = advantages == "" ? null : advantages;
            disadvantages = disadvantages == "" ? null : disadvantages;
            stationerDataSetCommentTableAdapter.InsertComment(comment, rating, advantages, disadvantages, article, order_number);
            MainWindow.Frame.Navigate(new MessagePage(true));
        }
    }
}
