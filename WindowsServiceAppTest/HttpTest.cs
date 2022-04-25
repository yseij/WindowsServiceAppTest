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
    }
}
