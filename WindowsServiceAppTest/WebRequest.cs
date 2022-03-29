using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace WindowsFormsAppTest
{
    class WebRequest
    {
        private string webserviceVersie;
        private string kraanDll;
        private string kraanIni;
        private string kraanDatabase;

        private int positionKraanDll;
        private int positionKraanIni;
        private int positionDatabaseConnect;
        private int positionDatabaseMelding;

        private bool certIsGoed = false;

        WebClient _wc;

        public WebRequest()
        {
            _wc = new WebClient();
        }

        public string GetWebRequest(int id, string urlHttp, string url, string securityId = "")
        {
            string webRequestUrl = urlHttp + url + securityId;
            Uri uri = new Uri(webRequestUrl);
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(webRequestUrl) as HttpWebRequest;
                HttpClient client = new HttpClient();
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    X509Certificate cert = request.ServicePoint.Certificate;
                    X509Certificate2 cert2 = null;
                    if (cert != null)
                    {
                        cert2 = new X509Certificate2(cert);
                        certIsGoed = cert2.Verify();
                    }
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        client.BaseAddress = uri;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // List data response.
                        HttpResponseMessage response1 = client.GetAsync(uri).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        if (response1.IsSuccessStatusCode)
                        {
                            //Parse the response body.
                            string data = response1.Content.ReadAsStringAsync().Result; //Make sure to add a reference to System.Net.Http.Formatting.dll
                            if (securityId == string.Empty)
                            {
                                if (certIsGoed)
                                {
                                    return GetDataOfWebRequest(data, cert.GetExpirationDateString().ToString());
                                }
                                else
                                {
                                    return GetDataOfWebRequest(data, "");
                                }
                            }
                            int Pos1 = data.IndexOf('{');
                            int Pos2 = data.IndexOf('}');
                            data = data.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                            if (certIsGoed)
                            {
                                return "{" + data + ", id: '" + id + "', certVerValDatum: '" + cert.GetExpirationDateString().ToString() + "'}";
                            }
                            return "{" + data + ", id: '" + id + "'}";
                            
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                return @"{ ex: '" + ex.Message.ToString() + ex.Response.ToString() + "'}";
            }
        }

        private string GetDataOfWebRequest(string data, string verValDatum = "")
        {
            positionKraanDll = data.IndexOf("KraanDLL");
            positionKraanIni = data.IndexOf("Kraan.ini");
            positionDatabaseConnect = data.IndexOf("Database connectie");
            positionDatabaseMelding = data.IndexOf("Database melding");

            webserviceVersie = data.Substring(0, positionKraanDll);
            kraanDll = data.Substring(positionKraanDll, positionKraanIni - positionKraanDll);
            kraanIni = data.Substring(positionKraanIni, positionDatabaseConnect - positionKraanIni);
            kraanDatabase = data.Substring(positionDatabaseConnect, positionDatabaseMelding - positionDatabaseConnect);

            return @"{ WebserviceVersie: '" + webserviceVersie + "', KraanDll: '" + kraanDll + "', KraanIni: '" + kraanIni + "', KraanDatabase: '" + kraanDatabase + "', certVerValDatum: '" + verValDatum + "'}";
        }
    }
}
