using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace MultiFaceRec
{
    class SQLServerHelper
    {
        //singleton instance
        private static SQLServerHelper instance = new SQLServerHelper();
        public static SQLServerHelper getInstance()
        {
            return instance;
        }

        private SQLServerHelper()
        {
            //read config file
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@"Resources\databaseconfig.txt");

                string sever = "";
                string database = "";
                foreach (string line in lines)
                {
                    string[] strs = line.Split('=');
                    if (strs.Length == 2)
                    {
                        if (strs[0].Equals("serverName"))
                        {
                            sever = strs[1];
                        }
                        else if (strs[0].Equals("database"))
                        {
                            database = strs[1];
                        }
                    }
                }
                //initialize connection string
                connectionString = String.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", sever, database);
            }
            catch
            {
            }
        }

        private static string connectionString;

        private SqlConnection Connection()
        {
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            return connect;
        }

        public List<Face> getFaces()
        {
            SqlConnection connect = null;
            List<Face> faces = new List<Face>();
            try
            {
                connect = Connection();
                string commandString = "SELECT * FROM Face";

                SqlDataAdapter adap = new SqlDataAdapter(commandString, connect);
                DataTable dt = new DataTable();
                adap.Fill(dt);


                foreach (DataRow row in dt.Rows)
                {
                    Face f = new Face();
                    f.Id = (int)row["id"];
                    f.Name = (string)row["Name"];
                    f.Phone = (string)row["Phone"];
                    f.Email = (string)row["Email"];
                    f.Dob = (DateTime)row["Birthday"];
                    byte[] bytes = (byte[])row["Img"];
                    f.Img = Utility.GetImageFromBytes(bytes);

                    faces.Add(f);
                }
            }
            catch
            {
                faces = new List<Face>();
            }
            finally
            {
                if (connect != null && connect.State == System.Data.ConnectionState.Open)
                    connect.Close();
            }
            return faces;
        }

        public int insertFace(Face f)
        {
            SqlConnection connect = null;
            try
            {
                connect = Connection();
                string commandString = "Insert into Face (Name, Phone, Email, Birthday, Img) Values(@NAME, @PHONE, @EMAIL, @BIRTHDAY, @IMG)";

                SqlCommand cmd = new SqlCommand(commandString, connect);

                SqlParameter Param = new SqlParameter("@NAME", SqlDbType.NVarChar);
                Param.Value = f.Name;
                cmd.Parameters.Add(Param);

                Param = new SqlParameter("@PHONE", SqlDbType.NVarChar);
                Param.Value = f.Phone;
                cmd.Parameters.Add(Param);

                Param = new SqlParameter("@EMAIL", SqlDbType.NVarChar);
                Param.Value = f.Email;
                cmd.Parameters.Add(Param);

                Param = new SqlParameter("@BIRTHDAY", SqlDbType.DateTime);
                Param.Value = f.Dob;
                cmd.Parameters.Add(Param);

                Param = new SqlParameter("@IMG", SqlDbType.Image );
                Param.Value = Utility.GetBytesFromImage(f.Img);
                cmd.Parameters.Add(Param);

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (connect != null && connect.State == System.Data.ConnectionState.Open)
                    connect.Close();
            }
        }
    }
}
