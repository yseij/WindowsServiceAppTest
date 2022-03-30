using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WindowsFormsAppTest
{
    class KlantTest
    {
        public KlantTest()
        {

        }

        private string ConnectieDB => ConfigurationManager.AppSettings["connectieString"];


        public List<KlantData> GetKlantData()
        {
            List<KlantData> klantDatas = new List<KlantData>();

            DataTable dt = new DataTable();
            int rows_returned;

            using (SqlConnection connection = new SqlConnection(ConnectieDB))
            using (SqlCommand cmd = connection.CreateCommand())
            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
            {

                cmd.CommandText = "SELECT * FROM Klant";
                cmd.CommandType = CommandType.Text;
                connection.Open();
                rows_returned = sda.Fill(dt);
                connection.Close();

                foreach (DataRow dr in dt.Rows)
                {
                    klantDatas.Add(new KlantData((int)dr[0], dr[1].ToString()));
                }
            }
            return klantDatas;
        }
    }
}
