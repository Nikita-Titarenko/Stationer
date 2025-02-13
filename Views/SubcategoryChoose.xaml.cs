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
    /// Логика взаимодействия для SubcategoryChoose.xaml
    /// </summary>
    public partial class SubcategoryChoose : Page
    {
        private MainWindow mainWindow;

        private int category_id;
        public SubcategoryChoose(int category_id, MainWindow mainWindow)
        {
            this.category_id = category_id;
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            var tag = b.Tag;
            MainWindow.Frame.Navigate(new ProductChoose((int)tag));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Stationer.StationerDataSet stationerDataSet = ((Stationer.StationerDataSet)(this.FindResource("stationerDataSet")));
            Stationer.StationerDataSetTableAdapters.categoryTableAdapter stationerDataSetcategoryTableAdapter = new Stationer.StationerDataSetTableAdapters.categoryTableAdapter();
            stationerDataSetcategoryTableAdapter.FillBy(stationerDataSet.category, category_id);
            System.Windows.Data.CollectionViewSource categoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("categoryViewSource")));
            Stationer.StationerDataSetTableAdapters.subcategoryTableAdapter stationerDataSetsubcategoryTableAdapter = new Stationer.StationerDataSetTableAdapters.subcategoryTableAdapter();
            stationerDataSetsubcategoryTableAdapter.FillByCategoryId(stationerDataSet.subcategory, category_id);
            System.Windows.Data.CollectionViewSource subcategoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("subcategoryViewSource")));
        }
    }
}
