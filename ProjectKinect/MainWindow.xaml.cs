using MySql.Data.MySqlClient;
using ProjectKinect.Model;
using ProjectKinect.Weather;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;



namespace ProjectKinect
{
    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        static DependencyProperty dp =
        DependencyProperty.Register("setImage", typeof(System.Drawing.Image), typeof(System.Windows.Controls.Image), null);

        static MySqlConnection connect = null;
        static MySqlCommand sqlcmd = null;
        static MySqlDataReader sqlread = null;

        static System.Drawing.Image imageFile = null;

        static FileStream fstream = null;
        static BinaryReader binread = null;
        static MemoryStream mstream = null;
    
        public MainWindow()
        {
            
            InitializeComponent();

            //string strConn =  "Server=localhost; Database=styler; Uid=root; Pwd=c00k!erun;";
            string strConn = "Server=122.44.13.91; Port = 11059; Database=styler; Uid=root;Pwd=1;";
            // Image Insert Ex.

            try
            {
                connect = new MySqlConnection(strConn);

                connect.Open();

                Console.WriteLine("DB와 연결 성공");
            }
            catch
            {
                Console.WriteLine("DB와 연결 안됨 Error");
            }

            Get_Weather();
            InitTimer();
            test2();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            PostureCapture ab = new PostureCapture();

            ab.ShowDialog();



        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            ClotheCapture cd = new ClotheCapture();
            cd.ShowDialog();
        }

        private async void Get_Weather()
        {
            List<WeatherDetails> weathers = await WeatherHelper.GetWeather();

            WeatherDetails weatherDetails = weathers.First();

            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(weatherDetails.WeatherIcon, UriKind.Relative);
            bi3.EndInit();
            ImgWeather.Stretch = Stretch.Fill;
            ImgWeather.Source = bi3;

            CurrentTemp.Text = weatherDetails.Temperature;
            MaxTemp.Text = weatherDetails.MaxTemperature;
            MinTemp.Text = weatherDetails.MinTemperature;
            Wind.Text = weatherDetails.WindSpeed;
            Dayofweek.Text = weatherDetails.WeatherDay;
        }

        public delegate void TempDelegate();
        public TempDelegate tempDelegate;
        Timer timer = null;

        private void InitTimer()
        {
            if (timer != null) return;
            TimerCallback timerCallback = new TimerCallback(ThreadFunc);
            timer = new Timer(timerCallback, null, 0, 1000);
        }

        private void ThreadFunc(Object stateInfo)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                tempDelegate += new TempDelegate(time_tick);
                Dispatcher.Invoke(DispatcherPriority.Normal, tempDelegate);
            }
        }

        private void time_tick()
        {
            Date.Text = System.DateTime.Now.ToString("yyyy년 MM월 dd일");
            Time.Text = System.DateTime.Now.ToString("tt hh:mm:ss");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClothesSelect CS = new ClothesSelect();
            CS.ShowDialog();
        }

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();

            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public void test2()
        {
            try
            {
                string query = "select image from gallery where galleryid = 3";

                sqlcmd = new MySqlCommand(query, connect);

                sqlread = sqlcmd.ExecuteReader();

                byte[] imageData = null;

                try
                {
                    while (sqlread.Read())
                    {
                        imageData = (byte[])sqlread[0];
                    }
                }
                catch
                {
                    Console.WriteLine("변환 중 오류 발생");
                }

                sqlread.Close();

                mstream = new MemoryStream(imageData);

                imageFile = System.Drawing.Image.FromStream(mstream);  // 이 시점인가?!

                Console.WriteLine(System.Drawing.Image.FromStream(mstream).GetType());
                Console.WriteLine("성공적으로 변환했습니다!");

                Bitmap bitmapFile = (Bitmap)imageFile;
                var handle = bitmapFile.GetHbitmap();

                //이미지 삽입

                Img.Source =
                    Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                Console.WriteLine("실패");
            }

        }
    }
}
