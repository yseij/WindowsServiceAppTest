using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsAppTest
{
    class HttpTest
    {
        public HttpTest()
        {

        }

        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];


        public List<HttpData> GetHttpData()
        {
            List<HttpData> htptDatas = new List<HttpData>();

            DataTable dt = new DataTable();
            int rows_returned;

            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            using (SqlCommand cmd = connection.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {

                cmd.CommandText = "SELECT * FROM Http";
                cmd.CommandType = CommandType.Text;
                connection.Open();
                rows_returned = sda.Fill(dt);
                connection.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    htptDatas.Add(new HttpData((int)dr[0], dr[1].ToString()));
                }
            }

            return htptDatas;
        }

        public List<HttpData> GetHttpDataByHttpName(string name)
        {
            List<HttpData> htptDatas = new List<HttpData>();

            DataTable dt = new DataTable();
            int rows_returned;

            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            {
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Http where name like '%" + name + "%'";
                SqlDataAdapter sda = new SqlDataAdapter(cmd);

                connection.Open();
                rows_returned = sda.Fill(dt);
                connection.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    htptDatas.Add(new HttpData((int)dr[0], dr[1].ToString()));
                }
            }

            return htptDatas;
        }

        public void AddHttp(string name)
        {
            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            {
                connection.Open();
                var sql = "INSERT INTO [dbo].[Http] ([Name]) VALUES (@Name)";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void UpdateHttp(int id, string name)
        {
            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            {
                connection.Open();
                var sql = "UPDATE Http SET Name = @Name " + "where id =" + id;
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void DeleteHttp(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            {
                connection.Open();
                var sql = "DELETE FROM Http where id =" + id;
                using (var cmd = new SqlCommand(sql, connection))
                    cmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
