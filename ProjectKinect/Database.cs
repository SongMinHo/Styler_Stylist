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
        static MySqlConnection connect = null;
        static MySqlCommand sqlcmd = null;
        static MySqlDataReader sqlread = null;

        static System.Drawing.Image imageFile = null;

        static FileStream fstream = null;
        static BinaryReader binread = null;
        static MemoryStream mstream = null;

        public Database()
        {
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
        }

        public void test()
        {
            String query = "SELECT * FROM Clothes";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                Console.WriteLine("{0}, {1}, {2}", dataReader["clothesId"], dataReader["name"], dataReader["category"]);
            }
            connect.Close();
        }

        public void getDatabaseImage(System.Windows.Controls.Image img)
        {
            try
            {
                string query = "select image from gallery where galleryid = 3";

                sqlcmd = new MySqlCommand(query, connect);

                sqlread = sqlcmd.ExecuteReader();

                byte[] imageData = null; // MySQL에서 데이터를 받아올 byte타입의 배열 객체

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

                mstream = new MemoryStream(imageData); // imageData를 MemoryStream에 넣음.

                imageFile = System.Drawing.Image.FromStream(mstream);

                Bitmap bitmapFile = (Bitmap)imageFile;
                var handle = bitmapFile.GetHbitmap();

                img.Source =
                    Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                Console.WriteLine("변환 실패");
            }
        }
    }
}
