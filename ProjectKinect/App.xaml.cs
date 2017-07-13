using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectKinect
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    using Microsoft.Kinect.Wpf.Controls;
    public partial class App : Application
    {
        internal KinectRegion KinectRegion { get; set; }
    }
}
