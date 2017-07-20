using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using Microsoft.Samples.Kinect.ControlsBasics.DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using ProjectKinect;

namespace ProjectKinect
{
    /// <summary>
    /// PostureSelect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostureSelect : Window
    {

        SampleDataSource sampleDataSource = null;


        IList<int> values;

        public PostureSelect()
        {
            sampleDataSource = new SampleDataSource("Track", "call getPostureImage()"); // 원문 select postureId image from gallery

            InitializeComponent();
            KinectRegion.SetKinectRegion(this, kinectRegion);

            App app = ((App)Application.Current);
            app.KinectRegion = kinectRegion;

            // Use the default sensor
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

            //// Add in display content

            // SampleDataSource ㄳㄲ

            this.itemsControl.ItemsSource = sampleDataSource.GetGroup();

            values = sampleDataSource.GetValueGroup();
        }


        /// <summary>
        /// Handle a button click from the wrap panel.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;
            if (sampleDataItem != null && sampleDataItem.NavigationPage != null)
            {
                MyValue = sampleDataItem.ImageSource;
                int uniqueId = Int32.Parse(sampleDataItem.UniqueId);
                Console.WriteLine(Int32.Parse(sampleDataItem.UniqueId));
                PostureId = values[uniqueId - 1];

                this.Close();



                //this.Close();
                //      this.navigationRegion.
                //    backButton.Visibility = System.Windows.Visibility.Visible;
                //    navigationRegion.Content = Activator.CreateInstance(sampleDataItem.NavigationPage);
            }

            else
            {
                this.kinectRegion.InputPointerManager.CompleteGestures();

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle the back button click.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>

        private ImageTrack _imageTrack;

        public ImageTrack MyTrack
        {
            get { return _imageTrack; }
            set { _imageTrack = value; }
        }

        private ImageSource _imageSource;

        public ImageSource MyValue
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }

        private int _postureid;

        public int PostureId
        {
            get { return _postureid; }
            set { _postureid = value; }
        }


    }

}
