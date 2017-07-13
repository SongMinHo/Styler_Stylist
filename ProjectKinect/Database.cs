﻿using Microsoft.Kinect;
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
    }
}
