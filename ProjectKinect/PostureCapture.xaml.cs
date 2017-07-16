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
using ProjectKinect;
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

    /// <summary>
    /// PostureCapture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostureCapture : Window
    {
        private CoordinateMapper coordinateMapper = null;

        private Database db = null;

      //  private static CBody cBody; // 캡쳐의 순간, 모든 좌표값을 저장해줄 변수.

        #region Members

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        private WriteableBitmap colorBitmap = null;
        bool imagecapture = false;

        #endregion

        #region Constructor

        /*
        string[] jointName =
        {
            "SpineBase", "SpineMid", "Neck", "Head", "ShoulderLeft",
            "ElbowLeft", "WristLeft", "HandLeft", "ShoulderRight", "ElbowRight",
            "WristRight", "HandRight", "HipLeft", "KneeLeft", "AnkleLeft",
            "FootLeft", "", "", "", "",
            "", "", "", "", ""
        };
        */

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

                                //if (imagecapture == true)
                                //{

                                //DepthSpacePoint testLeftArm = this.coordinateMapper.MapCameraPointToDepthSpace(body.Joints[JointType.HandLeft].Position);
                                //    float LeftZarm = body.Joints[JointType.HandLeft].Position.Z;
                                // tblRightHandState.Text = string.Format("( { 0:0.00}, { 1:0.00} ,{ 2:0.00} )", body.Joints[JointType.HandLeft].Position.X, body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.HandLeft].Position.Z);

                
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
                                        #region 1st_Posture
                                        string query = "call createpostureandgetid" +
                                            "(" + body.Joints[JointType.SpineShoulder].Position.X * 10000 +
                                            " , " + body.Joints[JointType.SpineShoulder].Position.Y * 10000 +
                                            " , " + body.Joints[JointType.SpineBase].Position.X * 10000 +
                                            " , " + body.Joints[JointType.SpineBase].Position.Y * 10000 + ");";
                                        db.setCommand(query);
                                        #endregion

                                        #region 2nd_JointPoint
                                        if (db.ExecQueryWithAnswer() != null)
                                        {
                                            db.getDataReader().Read();

                                            Console.WriteLine("Level 1 Complete.");

                                            int postureId = (int)db.getDataReader()["postureId"];
                                            Console.WriteLine("{0}", postureId);

                                            for (int i = 0; i < 25; i++)
                                            {
                                                string query2 = "call insertjointtoposture" +
                                                "( " + postureId + " " +
                                                " , " + i +
                                                " , " + body.Joints[(JointType)i].Position.X * 10000 +
                                                " , " + body.Joints[(JointType)i].Position.Y * 10000 +
                                                " , " + body.Joints[(JointType)i].Position.Z * 10000 + ");";

                                                db.setCommand(query2);
                                                db.ExecQueryDirect(); //
                                            }
                                            Console.WriteLine("Level 2 Complete.");
                                            #endregion

                                            #region 3rd_Image
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
                                            

                                        
                                            br = new BinaryReader(fs);

                                            byte[] imageData = br.ReadBytes((int)fs.Length);
                                            //encoder.Save(fs);

                                            query = "call insertpostureimage(" + postureId + " , @Image);";

                                            db.setCommand(query);
                                            db.setParams_InsertPostureImage(imageData);

                                            int RowsAffected = db.ExecQuery_withImage();

                                            if (RowsAffected > 0)
                                            { Console.WriteLine("성공적으로 저장되었습니다."); }
                                            else
                                            { Console.WriteLine("오류가 발생하였습니다."); }

                                            #endregion
                                        }
                                        else
                                        {
                                            //Error
                                        }

                                        imagecapture = false;




                                        break;
                                        }
                                        //}
                                }
                            }
                    }
                }
            }
        }

        #endregion

        public ImageSource ImageSource
        {
            get
            {
                return this.colorBitmap;
            }
        }

        public void Screenshot_Click(object sender, RoutedEventArgs e) // 1. 스켈레톤 받아오기
        {
            DBon();

            imagecapture = true;
        }

    

        public void DBon()
        {
            if (db == null)
                db = new Database();
        }
    }
 
}


//Posture.cs





//이건 Database.cs에 넣으시면 됩니다.