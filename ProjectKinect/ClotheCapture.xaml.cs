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

using MySql.Data.MySqlClient;

namespace ProjectKinect
{
    using System;

    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    

    public partial class ClotheCapture : Window
    {
        private CoordinateMapper coordinateMapper = null;

        private Database db = null;
        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        private WriteableBitmap colorBitmap = null;
        bool imagecapture = false;

        #endregion
        public ClotheCapture()
        {
            InitializeComponent();     
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

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

                                CameraSpacePoint LeftHandPoint = handLeft.Position;
                                CameraSpacePoint RightHandPoint = handRight.Position;

                                //   if (imagecapture == true)
                                //    {
                                tblRightHandState.Text = string.Format("( {0}, {1}, {2} )", LeftHandPoint.X, LeftHandPoint.Y, LeftHandPoint.Z);
                                tblLeftHandState.Text = string.Format("( {0}, {1}, {2} )", RightHandPoint.X, RightHandPoint.Y, RightHandPoint.Z);
                                imagecapture = false;
                                //    }


                                string rightHandState = "-";
                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
                                        break;

                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                    //    if (camera.Source != null)
                                     //   {
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


                                   //     }


                                        break;
                                }
                                tblLeftHandState.Text = rightHandState;

                            }
                        }
                    }
                }
            }
        }


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

                FileStream fs;
                BinaryReader br;
                
                using (fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                 fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    //fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                 br = new BinaryReader(fs);

                 byte[] ImageData = br.ReadBytes((int)fs.Length);
                 //encoder.Save(fs);

                 string dum_name = "옷이라능";
                 string dum_cat = "하의";
                 int dum_posId = 1;
                 //임시로 이름 / postureId 등이 저장되어 있음

                 string query = "call insertclothesimage"+
                  "("+ dum_name +" , "+ dum_cat +" , @Image , "+ dum_posId +");";

                 db.getCommand().Parameters.Add("@Image", MySqlDbType.LongBlob);
                 db.getCommand().Parameters["@Image"].Value = ImageData;

                int RowsAffected = db.ExecQuery_withImage(query);

                if (RowsAffected > 0)
                { Console.WriteLine("성공적으로 저장되었습니다."); }
                else
                { Console.WriteLine("오류가 발생하였습니다."); }
            }
            imagecapture = true;
        }
    }
}