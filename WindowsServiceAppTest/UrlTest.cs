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
        private List<UrlData> _urlDatasByForeignKeyWebservice = new List<UrlData>();
        private List<UrlData> _urlDatasByForeignKeyKlant = new List<UrlData>();
        private UrlData _urlDataById= new UrlData();

        private int eventId = 1;

        public UrlTest()
        {
            //GetUrls();
        }

        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];

        public List<UrlData> GetUrls(EventLog eventLog)
        {
            eventLog.WriteEntry("GetUrls", EventLogEntryType.Information, eventId++);
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
                    _urlDatas.Add(new UrlData((int)dr[0], dr[1].ToString(), dr[2].ToString(), (int)dr[3], (int)dr[4]));
                }
            }
            return _urlDatas;
        }
    }
}
