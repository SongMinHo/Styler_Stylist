using Microsoft.Kinect;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectKinect
{
    class Database
    {
        private static string strCon = "SERVER = 122.44.13.91; PORT = 11059 ; DATABASE = styler; UID = root; PWD = 1";
        MySqlConnection con = new MySqlConnection(strCon);

        public void ConnectDatabase()
        {
            try
            {
                con.Open();
                Console.WriteLine("DB 연결 완료");
            }
            catch (MySqlException e)
            {
                con.Close();
                Console.WriteLine("DB 연결 실패" + " (" + e.Message + ")");
            }
            //finally
            //{
            //    con.Close();
            //}
        }

        public void test()
        {
            ConnectDatabase();
            String query = "SELECT * FROM Clothes";
            MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while(dataReader.Read())
            {
                Console.WriteLine("{0}, {1}, {2}", dataReader["clothesId"], dataReader["name"], dataReader["category"]);
            }
            con.Close();
        }

        //System.Drawing.Bitmap img;
        //public System.Drawing.Bitmap test2()
        //{
        //    ConnectDatabase();
        //    String query = "SELECT * FROM Gallery WHERE GALLERYID = 3";
        //    MySqlCommand cmd = new MySqlCommand(query, con);
        //    MySqlDataReader dataReader = cmd.ExecuteReader();
        //    byte[] data = null;
        //    while (dataReader.Read())
        //    {
        //        data = (byte[])dataReader[0];
        //        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
        //        {
        //            img = new System.Drawing.Bitmap(ms);
        //        }
        //    }

        //    con.Close();
        //    return img;
        //}

       

        public static BitmapImage BitmapImageFromBytes(byte[] bytes)
        {
            BitmapImage image = null;
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(bytes);
                stream.Seek(0, SeekOrigin.Begin);
                System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                image = new BitmapImage();
                image.BeginInit();
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.StreamSource.Seek(0, SeekOrigin.Begin);
                image.EndInit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
            return image;
        }

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();

            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
