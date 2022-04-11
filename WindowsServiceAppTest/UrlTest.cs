using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Security;

namespace WindowsFormsAppTest
{
    class UrlTest : ServiceBase
    {
        private List<UrlData> _urlDatas = new List<UrlData>();

        public UrlTest()
        {
            
        }

        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];

        public List<UrlData> GetUrls(EventLog eventLog)
        {
            DataTable dt = new DataTable();
            int rows_returned;
            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            using (SqlCommand cmd = connection.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {
                cmd.CommandText = "SELECT * FROM Url";
                cmd.CommandType = CommandType.Text;
                connection.Open();
                rows_returned = sda.Fill(dt);
                connection.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    _urlDatas.Add(new UrlData((int)dr[0], dr[1].ToString(), dr[2].ToString(), (int)dr[3], (int)dr[4], (int)dr[5]));
                }
            }
            return _urlDatas;
        }
    }
}
