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

        public void closeConnect()
        {
            connect.Close();
        }

        public List<ImageSource> getImage(string query) // 이미지를 받아옴.
        {
            try
            {
                List<ImageSource> imgList = new List<ImageSource>();
                setCommand(query);
                sqlread = ExecQueryWithAnswer();

                byte[] imageData = null; // MySQL에서 데이터를 받아올 byte타입의 배열 객체

                try
                {
                    while (sqlread.Read())
                    {
                        imageData = (byte[])sqlread["image"];

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

        public IList<ImageTrack> getImageTrack(string query) // 이미지와 하나의 값을 받아옴.
        {
            try
            {
                IList<ImageTrack> imgTrack = new List<ImageTrack>();
                setCommand(query);
                sqlread = ExecQueryWithAnswer();

                byte[] imageData = null;

                try
                {
                    while (sqlread.Read())
                    {
                        ImageTrack track;
                        //getClothesImagePartial, getClothesImageAndPt, getPostureImage
                        imageData = (byte[])sqlread["image"];

                        mstream = new MemoryStream(imageData); // imageData를 MemoryStream에 넣음.

                        imageFile = System.Drawing.Image.FromStream(mstream);

                        Bitmap bitmapFile = (Bitmap)imageFile;
                        var handle = bitmapFile.GetHbitmap();

                        track = new ImageTrack(Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()), (int)sqlread[0]);

                        imgTrack.Add(track);
                    }
                    Console.WriteLine("리스트 저장 완료");
                }
                catch
                {
                    Console.WriteLine("변환 중 오류 발생");
                }

                sqlread.Close();

                return imgTrack;
            }
            catch
            {
                Console.WriteLine("변환 실패");
                return null;
            }
        }

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
