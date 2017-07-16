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

        static string postureId = "10";

        double[,] joints = new double[25, 3]; // 불러온 자세를 담은 배열

        double postureX = 0, postureY = 0, postureZ = 0; // 불러오는 자세의 xyz 좌표

        double x, y, z;// 현제 실시간 xyz 좌표

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
            PostureSelect ps = new PostureSelect();
            ps.ShowDialog();
            postureImage.Source = ps.MyValue;
            postureId = ps.postureId; // 현재 이 가져왔다는 걸 인식 못함! 이것만 해결되면 끗.
            Console.WriteLine("[ ttt{0} ]", postureId);
            DBon();

            string query = "call getjointdata(" + postureId + ");";
            db.setCommand(query);

            MySqlDataReader reader = db.ExecQueryWithAnswer();

            for (int i = 0; i < 25; i++)
            {
                reader.Read();

                joints[i, 0] = (double)reader["kinectx"];
                joints[i, 1] = (double)reader["kinecty"];
                joints[i, 2] = (double)reader["kinectz"];
                Console.WriteLine("{0}번 스켈레톤 X : {1} , Y : {2} , Z : {3}", i, joints[i, 0], joints[i, 1], joints[i, 2]);
            }

            //for (int i = 0; i < 25; i++)
            //{

            /*
                while (reader.Read())
                {
                //joints[i, 0] = (double)reader["kinectx"];
                //joints[i, 1] = (double)reader["kinecty"];
                //joints[i, 2] = (double)reader["kinectz"];

                joints[i, 0] = reader.GetDouble(0);
                joints[i, 1] = reader.GetDouble(1);
                joints[i, 2] = reader.GetDouble(2);


                Console.WriteLine("{0}번 스켈레톤 X : {1} , Y : {2} , Z : {3}", i++, joints[i, 0], joints[i, 1], joints[i, 2]);


                if (i == 24) break;
                }
            */

            //for (int i = 0; i < 25; i++)
            //{
            //    Console.WriteLine("{0}번 스켈레톤 X : {1} , Y : {2} , Z : {3}", i, joints[i, 0], joints[i, 1], joints[i, 2]);
            //}
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
                            


                                //DB에 입력된 자세의  X Y Z 를 가져와서 저장 시켜 높기

                                // 현제 스켈레톤의 좌표를 가져와서  실시간으로 읽음 , 저장된 자세의 XYZ 값을 비교                             
                                for (int i = 0; i < 25; i++)
                                {
                                    postureX = joints[i, 0];
                                    postureY = joints[i, 1];
                                    postureZ = joints[i, 2];

                                    //선택된 자세의 i번째 포인트 xyz를 가져옴


                                    x = body.Joints[(JointType)i].Position.X * 10000; // 가져온 실시간 x 좌표
                                    y = body.Joints[(JointType)i].Position.Y * 10000; // 가져온 실시간 y 좌표
                                    z = body.Joints[(JointType)i].Position.Z * 10000;  // 가져온 실시간 z 좌표
                                 
                                    if ((postureX - x) < 3000 && (postureX - x )> -3000 && (postureY - y )< 3000 && (postureY - y )> -3000 && (postureZ - z )< 3000 && (postureZ - z )> -3000)
                                    {
                                        // 사진 찍기
                                        BitmapEncoder encoder = new PngBitmapEncoder();
                                        BitmapSource image = (BitmapSource)camera.Source;
                                        // create frame from the writable bitmap and add to encoder
                                        encoder.Frames.Add(BitmapFrame.Create(image));

                                        canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                        canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                     

                                        // 찍은 이미지 db에 저장해야함
                                    }
                                    else
                                    {
                                        // Console.WriteLine("자세 {0}번째가 일치하지 않음", i);
                                        //자세가 맞지 않으면 해당 부분을 반복

                                        canvas.RedDrawHand(handRight, _sensor.CoordinateMapper);
                                        canvas.RedDrawHand(handLeft, _sensor.CoordinateMapper);



                                    }
                                    //저장된 자세의 (posture)xyz 값과 실시간의  xyz의 값의 차가 3000 이하 일때 사진을 촬영하여 DB에 저장
                                    //사진은 여기에서 저장 코드 작성.
                                }
                                
                            }
                        }
                    }
                }
            }
        }

        public void DBon()
        {
            if (db == null)
                db = new Database();
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

        }
    }
}