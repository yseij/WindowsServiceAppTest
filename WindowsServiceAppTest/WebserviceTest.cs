using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WindowsFormsAppTest
{
    class WebserviceTest
    {
        private List<WebServiceData> _webServiceDatas = new List<WebServiceData>();
        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];

        public List<WebServiceData> GetWebServiceDatas(bool reload = false)
        {
            if (reload)
            {
                _webServiceDatas.Clear();
                GetWebServices();
            }

            return _webServiceDatas;
        }

        public void GetWebServices()
        {

            DataTable dt = new DataTable();
            int rows_returned;

            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            using (SqlCommand cmd = connection.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {

                cmd.CommandText = "SELECT * FROM WebService";
                cmd.CommandType = CommandType.Text;
                connection.Open();
                rows_returned = sda.Fill(dt);
                connection.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    _webServiceDatas.Add(new WebServiceData((int)dr[0], dr[1].ToString(), (bool)dr[2]));
                }
            }
        }
    }
}
