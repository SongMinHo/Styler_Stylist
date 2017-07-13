using Microsoft.Kinect;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectKinect
{
    class Database
    {
        private static string strCon = "SERVER = 122.44.13.91; PORT = 11059 ; DATABASE = styler; UID = root; PWD = 1";
        MySqlConnection con = new MySqlConnection(strCon);

        private void ConnectDatabase()
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
        }

        private List<String> jointList = new List<string>();

        private void AddJointList()
        {
            jointList.Add("AnkleLeft");
            jointList.Add("AnkleRight");
            jointList.Add("ElbowLeft");
            jointList.Add("ElbowRight");
            jointList.Add("FootLeft");
            jointList.Add("FootRight");
            jointList.Add("HandLeft");
            jointList.Add("HandRight");
            jointList.Add("HandTipLeft");
            jointList.Add("HandTipRight");
            jointList.Add("Head");
            jointList.Add("HipLeft");
            jointList.Add("HipRight");
            jointList.Add("KneeLeft");
            jointList.Add("KneeRight");
            jointList.Add("Neck");
            jointList.Add("ShoulderLeft");
            jointList.Add("ShoulderRight");
            jointList.Add("SpineBase");
            jointList.Add("SpineMid");
            jointList.Add("SpineShoulder");
            jointList.Add("ThumbLeft");
            jointList.Add("ThumbRight");
            jointList.Add("WristLeft");
            jointList.Add("WristRight");
        }

        public void InsertPosture(Body body)
        {
            ConnectDatabase();
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

            foreach (JointType jointType in joints.Keys)
            {
                CameraSpacePoint position = joints[jointType].Position;
                String Query = string.Format("INSERT INTO JointPoint VALUES ({1}, {2}, {3}, {4})", jointType, position.X, position.Y, position.Z);
                MySqlCommand cmd = new MySqlCommand(Query, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
