using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        Geometry _selectedGeometry;

        public Geometry SelectedGeometry
        {
            get { return _selectedGeometry; }
            set
            {
                _selectedGeometry = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedGeometry"));
            }
        }

        public Window1()
        {
            InitializeComponent();

            this.DataContext = this;

            List<String> items = new List<string>();
            items.Add("Path Geometry");
            items.Add("Ellipse");
            items.Add("Rectangle");
            items.Add("Line");

            ComboBoxSelectedGeo.SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged);
            ComboBoxSelectedGeo.ItemsSource = items;

            ComboBoxSelectedGeo.SelectedIndex = 0;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Geometry geo = null;
            switch (ComboBoxSelectedGeo.SelectedIndex)
            {
                default:
                case 0:
                    geo = this.Resources["PathGeometry"] as PathGeometry;
                    break;
                case 1:
                    geo = this.Resources["EllipseGeometry"] as EllipseGeometry;
                    break;

                case 2:
                    geo = this.Resources["RectangleGeometry"] as RectangleGeometry;
                    break;

                case 3:
                    geo = this.Resources["LineGeometry"] as LineGeometry;
                    break;
            }

            if (geo == null)
                return;

            SelectedGeometry = geo;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
