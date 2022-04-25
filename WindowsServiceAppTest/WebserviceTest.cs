using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    class WebserviceTest
    {
        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];

        public List<WebServiceData> GetWebServiceData()
        {
            List<WebServiceData> webServiceDatas = new List<WebServiceData>();

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
                    webServiceDatas.Add(new WebServiceData((int)dr[0], dr[1].ToString(), (bool)dr[2]));
                }
            }
            return webServiceDatas;
        }
    }
}
