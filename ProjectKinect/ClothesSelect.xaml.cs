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
using System.Windows.Shapes;
using System.IO;
namespace ProjectKinect
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.ControlsBasics.DataModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    /// <summary>
    /// ClothesSelect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClothesSelect : Window
    {

       // private WriteableBitmap colorBitmap = null;
       
        public ClothesSelect()
        {
         

            this.InitializeComponent();

            PostureSelect ps = new PostureSelect();
                ps.ShowDialog();
              fullPosture.Source = ps.MyValue;

                var sampleDataSource = SampleDataSource.GetGroup("Group-1");
                this.itemsControl.ItemsSource = sampleDataSource;
            

            
            }



        //public imagesource imagesource
        //{
        //    get
        //    {
        //        return this.colorbitmap;
        //    }
        //}



        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {

            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null)
            {

                shirtImage.Source =  sampleDataItem.ImageSource;


            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            backButton.Visibility = System.Windows.Visibility.Hidden;
            navigationRegion.Content = this.kinectRegionGrid;
        }
        private ImageSource _myValue;

        //public ImageSource MyValue
        //{
        //    get { return _myValue; }
        //    set { _myValue = value; }
        //}
    }
}
