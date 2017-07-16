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
        private string strConn = "Server=122.44.13.91; Port = 11059; Database=styler; Uid=root;Pwd=1;";

        static MySqlConnection connect = null;
        static MySqlCommand sqlcmd = null;
        static MySqlDataReader sqlread = null;

        static System.Drawing.Image imageFile = null;

        static FileStream fstream = null;
        static BinaryReader binread = null;
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
            if (sqlread != null)
                sqlread.Close();
            if (sqlcmd != null)
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
            return sqlread;
        }

        public FileStream getFileStream()
        {
            return fstream;
        }

        public BinaryReader getBinaryReader()
        {
            return binread;
        }

        public MemoryStream getMemoryStream()
        {
            return mstream;
        }


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
        }


        public void getDatabaseImage(System.Windows.Controls.Image img, int index, String tableName)
        {
            try
            {
                string query = "select image from " + tableName + " where galleryid = 3";

                sqlcmd = new MySqlCommand(query, connect);

                sqlread = sqlcmd.ExecuteReader();

                byte[] imageData = null; // MySQL에서 데이터를 받아올 byte타입의 배열 객체

                try
                {
                    while (sqlread.Read())
                    {
                        imageData = (byte[])sqlread[index];
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

        public void getTableRowsCount(String tableName)
        {
            String count = null;

            String query = "CALL GetCount ('" + tableName + "');";
            sqlcmd = new MySqlCommand(query, connect);
            sqlread = sqlcmd.ExecuteReader();
            while (sqlread.Read())
            {
                Console.WriteLine("{0}", sqlread["count"]);
                // count = Convert.ToString(dataReader["count"]);
            }
            //return count;
        }

        public void closeConnect()
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

        #region IMSI

        public List<ImageSource> getImage(string query)
        {
            try
            {
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
        #endregion

        public int ExecQuery()
        {

            return sqlcmd.ExecuteNonQuery();
        }

        public void ExecQueryDirect()
        {
            sqlcmd.ExecuteNonQuery();
        }

        public MySqlDataReader ExecQueryWithAnswer()
        {
            sqlread = sqlcmd.ExecuteReader();

            return sqlread;
        }

        public int ExecQuery_withImage()
        {
            sqlread = null;
            return sqlcmd.ExecuteNonQuery();
        }
    }
}
