using MySql.Data.MySqlClient;
using ProjectKinect.Model;
using ProjectKinect.Weather;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ProjectKinect;

using System.IO.Ports;
using System.ComponentModel;

namespace ProjectKinect
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Get_Weather();
            InitTimer();
            NewSerial();

        }
        private BackgroundWorker _worker = new BackgroundWorker();
        private SerialComm serial = new SerialComm();
        private bool _continue;

        PostureCapture posturebutton = new PostureCapture();

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            PostureCapture ab = new PostureCapture();
            ab.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClothesSelect CS = new ClothesSelect();
            CS.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ClotheCapture cd = new ClotheCapture();
            cd.ShowDialog();
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _continue = true;
            if (serial.IsOpen == true)
            {
                //열린 포트 닫고
                serial.CloseComm();
            }

            serial.OpenComm("COM9", 9600, 8, StopBits.One, Parity.None, Handshake.None);
            
            if (serial.IsOpen == true)
            {
                //계속 체크
                while (true)
                {
                    if (SerialComm.strBuffer != null)
                    {
                        while (true)
                        {
                            int i = Convert.ToInt32(SerialComm.strBuffer);
                            switch (i)
                            {
                                case 1:
                                    Console.WriteLine("Case 1");
                                    _worker.ReportProgress(1);
                                    SerialComm.strBuffer = null;
                                    break;
                                case 2:
                                    Console.WriteLine("Case 2");
                                    _worker.ReportProgress(2);
                                    SerialComm.strBuffer = null;
                                    break;
                                case 3:
                                    Console.WriteLine("case 3");
                                    _worker.ReportProgress(3);
                                    SerialComm.strBuffer = null;
                                    break;
                                case 4:
                                    Console.WriteLine("case 4");
                                    _worker.ReportProgress(4);
                                    SerialComm.strBuffer = null;
                                    break;

                                    //default:
                                    //    Console.WriteLine("엿이나먹으라지");
                                    //    SerialComm.strBuffer = null;
                                    //    break;
                            } 
                        } 
                        //_continue = false;
                    }
                    //else Console.WriteLine("입력값 없음.");
                } 
            }
            else
            {
                Console.WriteLine("연결된 포트가 없습니다");
                serial.CloseComm();
            }
        }

        private void Invoke(Button button)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //serial.CloseComm();
            }));
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //ReportProgress 메서드가 호출되었을 때 실행될 코드

            if (e.ProgressPercentage == 1)
            {
                Invoke(MyPose);
            }
            else if (e.ProgressPercentage == 2)
            {
                Invoke(ClothesMatching);
            }
            else if (e.ProgressPercentage == 3)
            {
                Invoke(ClothesShot);
            }
            else if (e.ProgressPercentage == 4)
            {
                Invoke(posturebutton.Screenshot);
            }
        }

        public void NewSerial()
        {
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.WorkerReportsProgress = true;
            
            _worker.RunWorkerAsync();
        }
    }


}