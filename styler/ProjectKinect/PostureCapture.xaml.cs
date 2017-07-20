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
using System.Drawing;
using System.Drawing.Drawing2D;
using ProjectKinect;
using MySql.Data.MySqlClient;

using System.IO.Ports;
using System.ComponentModel;

namespace ProjectKinect
{
    using System;

    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Windows.Threading;
    using System.Timers;
    using System.Windows.Forms;
    using System.Threading.Tasks;

    /// <summary>
    /// PostureCapture.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostureCapture : Window
    {
        private CoordinateMapper coordinateMapper = null;
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        private WriteableBitmap colorBitmap = null;
        bool imagecapture = false;
        private Database db = null;
        private int countstack = 0;
        

        

        public PostureCapture()
        {
            InitializeComponent();

            DBon();
        }
        //private BackgroundWorker _worker = new BackgroundWorker();
        //private SerialComm serial = new SerialComm();
        

        #region Event handlers

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
                            if (imagecapture == true)
                            {
                                #region 1st_Posture
                                string query = "call createpostureandgetid()";
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
                                        " , " + convert_x(body.Joints[(JointType)i].Position.X) +
                                        " , " + convert_y(body.Joints[(JointType)i].Position.Y) +
                                        " , " + body.Joints[(JointType)i].Position.Z + ");";

                                        db.setCommand(query2);
                                        db.ExecQueryDirect(); //
                                    }
                                    Console.WriteLine("Level 2 Complete.");
                                    #endregion

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

                                    // Encoder 내의 Image가 Image화

                                    query = "call insertpostureimage(" + postureId + " , @Image);";

                                    db.setCommand(query);
                                    db.setParams_InsertPostureImage(imageData);

                                    int RowsAffected = db.ExecQuery_withImage();

                                    if (RowsAffected > 0)
                                    { Console.WriteLine("성공적으로 저장되었습니다."); }
                                    else
                                    { Console.WriteLine("오류가 발생하였습니다."); }
                                    imagecapture = false;

                                    Close();

                                    #endregion
                                }
                            }

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


                                        var t = Task.Run(async delegate
                                        {
                                            await Task.Delay(3000);
                                            return 42;
                                        });
                                        t.Wait();
                                        

                                        // 찍는 거에 카운트 추가해야 함. 그리고 찍힐 때 화면상 표시 필요.

                                        #region 1st_Posture
                                        string query = "call createpostureandgetid()";
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
                                                " , " + convert_x(body.Joints[(JointType)i].Position.X) +
                                                " , " + convert_y(body.Joints[(JointType)i].Position.Y) +
                                                " , " + body.Joints[(JointType)i].Position.Z + ");";

                                                db.setCommand(query2);
                                                db.ExecQueryDirect(); //
                                            }
                                            Console.WriteLine("Level 2 Complete.");
                                            #endregion

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

                                            // Encoder 내의 Image가 Image화

                                            query = "call insertpostureimage(" + postureId + " , @Image);";

                                            db.setCommand(query);
                                            db.setParams_InsertPostureImage(imageData);

                                            int RowsAffected = db.ExecQuery_withImage();

                                            if (RowsAffected > 0)
                                            { Console.WriteLine("성공적으로 저장되었습니다."); }
                                            else
                                            { Console.WriteLine("오류가 발생하였습니다."); }

                                            Close();

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process procName in System.Diagnostics.Process.GetProcesses())
            {
                string procNm = procName.ToString();
                procNm = procNm.Replace("System.Diagnostics.Process (", "");
                procNm = procNm.Replace(")", "");
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
    }
}
