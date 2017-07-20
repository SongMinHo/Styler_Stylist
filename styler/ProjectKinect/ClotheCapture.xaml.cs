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

        static int postureId;

        double[,] joints = new double[25, 3]; // 불러온 자세를 담은 배열

        static int[] fit = new int[25]; // 피트. 부위별로 맞았는지 안 맞았는지 체크하는 변수.

        Joint[] parts = new Joint[25]; // ..

        static int lastval = 0;

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
            postureId = ps.PostureId; // 현재 이 가져왔다는 걸 인식 못함! 이것만 해결되면 끗.

            Console.WriteLine("[ {0} ]", postureId);
            if (postureId == 0)
            {
                this.Close();
            }

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

                fit[i] = 0;
            }
            //값 초기화. 0은 비활성. 1은 활성. 활성이 22개 이상일 경우, 작동함.
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
                            //                        if (body.IsTracked)
                            //                        {
                            //                            lastval = 0;
                            //                            for (int i = 0; i < 25; i++)
                            //                            {
                            //                                parts[i] = body.Joints[(JointType)i];

                            //                                //DB에 입력된 자세의  X Y Z 를 가져와서 저장 시켜 높기

                            //                                // 현제 스켈레톤의 좌표를 가져와서  실시간으로 읽음 , 저장된 자세의 XYZ 값을 비교                             

                            //                                postureX = joints[i, 0];
                            //                                postureY = joints[i, 1];
                            //                                postureZ = joints[i, 2];

                            //                                //선택된 자세의 i번째 포인트 xyz를 가져옴

                            //                                x = convert_x(body.Joints[(JointType)i].Position.X); // 가져온 실시간 x 좌표
                            //                                y = convert_y(body.Joints[(JointType)i].Position.Y); // 가져온 실시간 y 좌표
                            //                                z = body.Joints[(JointType)i].Position.Z;  // 가져온 실시간 z 좌표

                            //                                if (((postureX - x) < 75 && (postureX - x) > -40) &&
                            //                                    ((postureY - y) < 40 && (postureY - y) > -40) &&
                            //                                    ((postureZ - z < 0.5) && (postureZ - z) > -0.5))
                            //                                {
                            //                                    fit[i] = 1;
                            //                                    lastval++;
                            //                                    canvas.DrawHand(parts[i], _sensor.CoordinateMapper);

                            //                                }
                            //                                else
                            //                                {
                            //                                    fit[i] = 0;

                            //                                }

                            //                             /*

                            //*/

                            //                          //      for (int j = 0; j < 25; j++)

                            //                                //   canvas.DrawHand(handLeft, _sensor.CoordinateMapper);

                            //                                // 찍은 이미지 db에 저장해야함

                            //                                // Console.WriteLine("자세 {0}번째가 일치하지 않음", i);
                            //                                //자세가 맞지 않으면 해당 부분을 반복

                            //                                //for (int j = 0; j < 25; j++)
                            //                                //    canvas.RedDrawHand(parts[j], _sensor.CoordinateMapper);
                            //                                //canvas.RedDrawHand(handLeft, _sensor.CoordinateMapper);

                            //                                // 저장된 자세의 (posture)xyz 값과 실시간의 xyz의 값의 차가
                            //                                // 일정 수치 이하 일때 사진을 촬영하여 DB에 저장

                            //                                // 사진은 여기에서 저장 코드 작성.
                            //                            }
                            //                            if (lastval >= 20)
                            //                            {
                            //                  //              Console.WriteLine("Done!");
                            //                                #region 3rd_Image

                            //                                BitmapEncoder encoder = new PngBitmapEncoder();
                            //                                BitmapSource image = (BitmapSource)camera.Source;
                            //                                byte[] imageData;

                            //                                BitmapFrame bf = BitmapFrame.Create(image);

                            //                                ScaleTransform st = new ScaleTransform();
                            //                                st.ScaleX = (double)1080 / (double)bf.PixelWidth;
                            //                                st.ScaleY = (double)1620 / (double)bf.PixelHeight;

                            //                                TransformedBitmap tb = new TransformedBitmap(bf, st);
                            //                                BitmapMetadata thumbMeta = new BitmapMetadata("jpg");
                            //                                thumbMeta.Title = "thumbnail";
                            //                                JpegBitmapEncoder jencoder = new JpegBitmapEncoder();
                            //                                jencoder.QualityLevel = 100;
                            //                                jencoder.Frames.Add(BitmapFrame.Create(tb, null, thumbMeta, null));

                            //                                using (MemoryStream ms = new MemoryStream())
                            //                                {
                            //                                    jencoder.Save(ms);

                            //                                    imageData = ms.ToArray();
                            //                                }

                            //                                string query = "insert into gallery(postureId, image) values(" + postureId + ", @Image)";//

                            //                                db.setCommand(query);
                            //                                db.setParams_InsertPostureImage(imageData);

                            //                                //하의는 0번째 배열을 참조.

                            //                                try
                            //                                {
                            //                                    int RowsAffected = db.ExecQuery_withImage();

                            //                                    if (RowsAffected > 0)
                            //                                    { Console.WriteLine("성공적으로 저장되었습니다."); }
                            //                                    else
                            //                                    { Console.WriteLine("오류가 발생하였습니다.1"); }
                            //                                }
                            //                                catch
                            //                                {
                            //                                    Console.WriteLine("오류가 발생하였습니다.");
                            //                                }
                            //                                #endregion
                            //                            }
                            //                            else
                            //                            {
                            //                                Console.WriteLine("Fail");
                            //                            }
                            //                        }

                            if (body.IsTracked)
                            {
                                lastval = 0;
                                for (int i = 0; i < 25; i++)
                                {
                                    parts[i] = body.Joints[(JointType)i];

                                    //DB에 입력된 자세의  X Y Z 를 가져와서 저장 시켜 높기

                                    // 현제 스켈레톤의 좌표를 가져와서  실시간으로 읽음 , 저장된 자세의 XYZ 값을 비교                             

                                    postureX = joints[i, 0];
                                    postureY = joints[i, 1];
                                    postureZ = joints[i, 2];

                                    //선택된 자세의 i번째 포인트 xyz를 가져옴

                                    x = convert_x(body.Joints[(JointType)i].Position.X); // 가져온 실시간 x 좌표
                                    y = convert_y(body.Joints[(JointType)i].Position.Y); // 가져온 실시간 y 좌표
                                    z = body.Joints[(JointType)i].Position.Z;  // 가져온 실시간 z 좌표

                                    if (((postureX - x) < 75 && (postureX - x) > -75) &&
                                        ((postureY - y) < 75 && (postureY - y) > -75) &&
                                        ((postureZ - z < 0.5) && (postureZ - z) > -0.5))
                                    {
                                        fit[i] = 1;
                                        lastval++;
                                       canvas.DrawHand(parts[i], _sensor.CoordinateMapper);
                                    }
                                    else
                                    {
                                        fit[i] = 0;
                                    }

                                    Console.Write(" {0}", fit[i]);

                        
                                }
                                if (lastval >= 20)
                                {
                                    Console.WriteLine("Done!");
                                    #region 3rd_Image

                                    BitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapSource image = (BitmapSource)camera.Source;
                                    byte[] imageData;

                                    BitmapFrame bf = BitmapFrame.Create(image);

                                    ScaleTransform st = new ScaleTransform();
                                    st.ScaleX = (double)1080 / (double)bf.PixelWidth;
                                    st.ScaleY = (double)1620 / (double)bf.PixelHeight;

                                    TransformedBitmap tb = new TransformedBitmap(bf, st);
                                    BitmapMetadata thumbMeta = new BitmapMetadata("jpg");
                                    thumbMeta.Title = "thumbnail";
                                    JpegBitmapEncoder jencoder = new JpegBitmapEncoder();
                                    jencoder.QualityLevel = 100;
                                    jencoder.Frames.Add(BitmapFrame.Create(tb, null, thumbMeta, null));

                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        jencoder.Save(ms);

                                        imageData = ms.ToArray();
                                    }

                                    string query = "insert into gallery(postureId, image, collection) values(" + postureId + ", @Image, 1)"; //

                                    db.setCommand(query);
                                    db.setParams_InsertPostureImage(imageData);

                                    //하의는 0번째 배열을 참조.

                                    try
                                    {
                                        int RowsAffected = db.ExecQuery_withImage();

                                        if (RowsAffected > 0)
                                        { Console.WriteLine("성공적으로 저장되었습니다."); }
                                        else
                                        { Console.WriteLine("오류가 발생하였습니다.1"); }
                                    }
                                    catch
                                    {
                                        Console.WriteLine("오류가 발생하였습니다.");
                                    }

                                    this.Close();
                                    #endregion
                                }
                                else
                                {
                                    Console.WriteLine("Fail");
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

        public void DBoff()
        {
            db = null;
        }

        #region 좌표변환
        public double convert_x(double val)
        {
            return (val + 1) * 540;
        }

        public double convert_y(double val)
        {
            return (1 - val) * 810;
        }
        #endregion

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