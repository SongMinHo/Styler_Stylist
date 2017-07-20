using MySql.Data.MySqlClient;
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
using ProjectKinect.HandTracking;
using System.Drawing.Printing;

using MySql.Data.MySqlClient;
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
using ProjectKinect.HandTracking;
using System.Drawing.Printing;


namespace ProjectKinect
{
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.ControlsBasics.DataModel;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// ClothesSelect.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClothesSelect : Window
    {
        SampleDataSource sampleDataSource = null;

        IList<int> values;

        PostureSelect ps = new PostureSelect();
        KinectSensor _sensor;
        IList<Body> _bodies;
        int postureId;

        private Database db = null;

        public ClothesSelect()
        {
            this.InitializeComponent();

            ps.ShowDialog();
            fullPosture.Source = ps.MyValue;
            postureId = ps.PostureId;

            if (postureId == null)
            {
                this.Close();
            }

            try
            {
                sampleDataSource = new SampleDataSource("Track", "call getClothesImage(" + postureId + ")");
                this.itemsControl.ItemsSource = sampleDataSource.GetGroup();
                values = sampleDataSource.GetValueGroup();
                DBon();
            }
            catch
            {
                Console.WriteLine("자세Id 못받았음");
            }

        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color

            fullPosture.Source = ps.MyValue;

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];
                                Joint Spine = body.Joints[JointType.SpineBase];
                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // Draw hands and thumbs

                                //DB에 입력된 자세의  X Y Z 를 가져와서 저장 시켜 높기

                                // 현제 스켈레톤의 좌표를 가져와서  실시간으로 읽음 , 저장된 자세의 XYZ 값을 비교                             

                                // 사진 찍기
                                //BitmapEncoder encoder = new PngBitmapEncoder();
                                //BitmapSource image = (BitmapSource)camera.Source;
                                //// create frame from the writable bitmap and add to encoder
                                //encoder.Frames.Add(BitmapFrame.Create(image));

                                canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                //  canvas.DrawHand(handLeft, _sensor.CoordinateMapper);

                                // 찍은 이미지 db에 저장해야함
                            }
                            else
                            {
                                // Console.WriteLine("자세 {0}번째가 일치하지 않음", i);
                                // 자세가 맞지 않으면 해당 부분을 반복

                                // for (int j = 0; j < 25; j++)
                                // canvas.RedDrawHand(parts[j], _sensor.CoordinateMapper);
                                // canvas.RedDrawHand(handLeft, _sensor.CoordinateMapper);
                            }

                            //저장된 자세의 (posture)xyz 값과 실시간의  xyz의 값의 차가 3000 이하 일때 사진을 촬영하여 DB에 저장
                            //사진은 여기에서 저장 코드 작성.
                        }
                    }
                }
            }
        }
   
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {

            var button = (Button)e.OriginalSource;
            SampleDataItem sampleDataItem = button.DataContext as SampleDataItem;

            int uniqueId = Int32.Parse(sampleDataItem.UniqueId);

            int category = values[uniqueId - 1];

            if (sampleDataItem != null && sampleDataItem.NavigationPage != null)
            {
                if (category == 1)
                {
                    shirtImage.Source = sampleDataItem.ImageSource;
                }
                else if (category == 2)
                {
                    pantsImage.Source = sampleDataItem.ImageSource;
                }
                else
                {
                }
            }
            Console.WriteLine("옷입힘");
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            //backButton.Visibility = System.Windows.Visibility.Hidden;
            //navigationRegion.Content = this.kinectRegionGrid;

            this.Close();
        }
        private ImageSource _myValue;

        //public ImageSource MyValue
        //{
        //    get { return _myValue; }
        //    set { _myValue = value; }x9\4h4
        //}

        public void DBon()
        {
            if (db == null)
                db = new Database();
        }

        public void DBoff()
        {
            db = null;
        }
    }
}
