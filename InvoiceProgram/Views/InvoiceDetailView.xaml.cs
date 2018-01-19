using InvoiceProgram.Models;
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

namespace InvoiceProgram.Views
{
    /// <summary>
    /// InvoiceDetailView.xaml 的互動邏輯
    /// </summary>
    public partial class InvoiceDetailView : UserControl
    {
        public InvoiceDetailView()
        {
            InitializeComponent();
        }

        private void DataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }

        private void DataGrid_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.MessageBox.Show("HI~");
        }
    }
}
