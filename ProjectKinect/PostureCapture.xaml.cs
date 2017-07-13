using Microsoft.Kinect;
using ProjectKinect.HandTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ProjectKinect
{
    using System;

    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;



    /// <summary>
    /// PostureCapture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostureCapture : Window
    {
        private CoordinateMapper coordinateMapper = null;
        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        private WriteableBitmap colorBitmap = null;
        bool imagecapture = false;

        #endregion

        #region Constructor

        public PostureCapture()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            int a;

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }

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
                                canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                canvas.DrawHand(handLeft, _sensor.CoordinateMapper);

                                if (imagecapture == true)
                                {

                                    //DepthSpacePoint testLeftArm = this.coordinateMapper.MapCameraPointToDepthSpace(body.Joints[JointType.HandLeft].Position);
                                //    float LeftZarm = body.Joints[JointType.HandLeft].Position.Z;
                                    tblRightHandState.Text = string.Format("( { 0:0.00}, { 1:0.00} ,{ 2:0.00} )", body.Joints[JointType.HandLeft].Position.X, body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.HandLeft].Position.Z);

                                    imagecapture = false;
                                }


                            }
                        }
                    }
                }
            }
        }

        #endregion

        int a;


        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public void Screenshot_Click(object sender, RoutedEventArgs e)
        {
            if (camera.Source != null)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                BitmapSource image = (BitmapSource)camera.Source;
                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(image));
                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }


            }
            imagecapture = true;
        }
    }
}