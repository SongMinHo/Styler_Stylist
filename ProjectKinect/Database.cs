using Microsoft.Kinect;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private string strConn = "Server=122.44.13.91; Port = 11059; Database=styler; Uid=root;Pwd=1;";

        private static MySqlConnection connect = null;
        private static MySqlCommand sqlcmd = null;

        static MySqlDataReader sqlreader = null;

        static System.Drawing.Image imageFile = null;

        static FileStream fstream = null;
        static BinaryReader breader = null;
        static MemoryStream mstream = null;

        public MySqlConnection getConnection()
        {
            return connect;
        }

        public MySqlCommand getCommand()
        {
            return sqlcmd;
        }

        public void setCommand(string query)
        {
            if(sqlcmd != null)
                sqlcmd = null;
            sqlcmd = new MySqlCommand(query, connect);
        }

        public void setParams_InsertPostureImage(Byte[] imageData) // Parameter 설정.
        {
            sqlcmd.Parameters.Add("@Image", MySqlDbType.LongBlob);
            sqlcmd.Parameters["@Image"].Value = imageData;
        }

        public MySqlDataReader getDataReader()
        {
            return sqlreader;
        }

        public FileStream getFileStream()
        {
            return fstream;
        }

        public BinaryReader getBinaryReader()
        {
            return breader;
        }

        public MemoryStream getMemoryStream()
        {
            return mstream;
        }

        public Database()
        {
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

        public bool ExecQuery(string query)
        {
            sqlcmd = new MySqlCommand(query, connect);

            sqlreader = sqlcmd.ExecuteReader();



            return true;
        }

        public int ExecQuery_withImage(string query)
        {
                return sqlcmd.ExecuteNonQuery();
        }

        /*
         * {
         *  mstream = new MemoryStream(imageData); // imageData를 MemoryStream에 넣음.
            Console.WriteLine("변환 성공");
            return System.Drawing.Image.FromStream(mstream);
            }
        */

        public void getDatabaseImage(System.Windows.Controls.Image img)
        {
            try
            {
                string query = "select image from gallery where galleryid = 3";

                sqlcmd = new MySqlCommand(query, connect);

                sqlreader = sqlcmd.ExecuteReader();

                byte[] imageData = null; // MySQL에서 데이터를 받아올 byte타입의 배열 객체

                try
                {
                    while (sqlreader.Read())
                    {
                        imageData = (byte[])sqlreader[0];
                    }
                }
                catch
                {
                    Console.WriteLine("변환 중 오류 발생");
                }

                sqlreader.Close();

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
        public void CloseConnect()
        {
            connect.Close();
        }

        public List<ImageSource> getImage()
        {
            try
            {
                string query = "select image from ClothesPosture";
                List<ImageSource> imgList = new List<ImageSource>();
                sqlcmd = new MySqlCommand(query, connect);

                sqlread = sqlcmd.ExecuteReader();

                byte[] imageData = null; // MySQL에서 데이터를 받아올 byte타입의 배열 객체

                try
                {
                    while (sqlread.Read())
                    {
                        imageData = (byte[])sqlread[0];

                        mstream = new MemoryStream(imageData); // imageData를 MemoryStream에 넣음.

                        imageFile = System.Drawing.Image.FromStream(mstream);

                        Bitmap bitmapFile = (Bitmap)imageFile;
                        var handle = bitmapFile.GetHbitmap();
                        imgList.Add(Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                    }
                    Console.WriteLine("리스트 저장 완료");
                }
                catch
                {
                    Console.WriteLine("변환 중 오류 발생");
                }

                sqlread.Close();

                return imgList;
            }
            catch
            {
                Console.WriteLine("변환 실패");
                return null;
            }
        }
    }
}
