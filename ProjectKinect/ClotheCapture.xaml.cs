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

                                //CameraSpacePoint LeftHandPoint = handLeft.Position;
                                //CameraSpacePoint RightHandPoint = handRight.Position;

                                //tblRightHandState.Text = string.Format("( {0}, {1}, {2} )", LeftHandPoint.X, LeftHandPoint.Y, LeftHandPoint.Z);
                                //tblLeftHandState.Text = string.Format("( {0}, {1}, {2} )", RightHandPoint.X, RightHandPoint.Y, RightHandPoint.Z);
                                //imagecapture = false;




                                //DB에 입력된 자세의  X Y Z 를 가져와서 저장 시켜 높기

                                double postureX = 0, postureY = 0, postureZ = 0; // 불러오는 자세의 xyz 좌표
                                double x, y, z;// 현제 실시간 xyz 좌표



                                // 현제 스켈레톤의 좌표를 가져와서  실시간으로 읽음 , 저장된 자세의 XYZ 값을 비교                             
                                for (int i = 0; i < 25; i++)
                                {
                                    //선택된 자세의 i번째 포인트 xyz를 가져옴
                                    //postureX=
                                    //   postureY=
                                    //  postureZ=


                                    x = body.Joints[(JointType)i].Position.X * 10000; // 가져온 실시간 x 좌표
                                    y = body.Joints[(JointType)i].Position.Y * 10000; // 가져온 실시간 y 좌표
                                    z = body.Joints[(JointType)i].Position.Z * 10000;  // 가져온 실시간 z 좌표

                                    //저장된 자세의 (posture)xyz 값과 실시간의  xyz의 값의 차가 3000 이하 일때 사진을 촬영하여 DB에 저장
                                    if (postureX - x < 3000 && postureY - y < 3000 && postureZ - z < 3000)
                                    {

                                        // 사진 찍기
                                        BitmapEncoder encoder = new PngBitmapEncoder();
                                        BitmapSource image = (BitmapSource)camera.Source;
                                        // create frame from the writable bitmap and add to encoder
                                        encoder.Frames.Add(BitmapFrame.Create(image));


                                        // 찍은 이미지 db에 저장해야함

                                    }

                                    else
                                    {
                                        i = i - 1; //자세가 맞지 않으면 해당 부분을 반복 

                                    }

                                }


                                break;


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

                string query = "call insertclothesimage" +
                 "(" + dum_name + " , " + dum_cat + " , @Image , " + dum_posId + ");";

                db.getCommand().Parameters.Add("@Image", MySqlDbType.LongBlob);
                db.getCommand().Parameters["@Image"].Value = ImageData;

                int RowsAffected = db.ExecQuery_withImage(query);

                if (RowsAffected > 0)
                {
                    Console.WriteLine("성공적으로 저장되었습니다.");
                }
                else
                {
                    Console.WriteLine("오류가 발생하였습니다.");
                }
            }
            imagecapture = true;

            /*
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

            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            //fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            byte[] ImageData = br.ReadBytes((int)fs.Length);
            //encoder.Save(fs);

        //            string dum_name = "옷이라능";
        //            string dum_cat = "하의";
        //            int dum_posId = 1;
        //            //임시로 이름 / postureId 등이 저장되어 있음

        //            string query = "call insertclothesimage"+
        //                "("+ dum_name +" , "+ dum_cat +" , @Image , "+ dum_posId +")";

        //cmd.Parameters.Add("@Image", MySqlDbType.LongBlob);
        //            cmd.Parameters["@Image"].Value = ImageData;

        //            if(Database.ExecQuery_withImage(query))
        //            {
        //                Console.WriteLine("성공적으로 저장되었습니다.");
        //            }
        //            else
        //            {
        //                Console.WriteLine("오류가 발생하였습니다.");
        //            }
        //    }
        //    imagecapture = true;
        */
        }
    }
}