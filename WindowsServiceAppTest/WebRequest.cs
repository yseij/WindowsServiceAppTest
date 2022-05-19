using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace WindowsServiceAppTest
{
    class WebRequest
    {
        private bool _certIsGoed = false;

        WebClient _wc;

        public WebRequest()
        {
            _wc = new WebClient();
        }

        public string GetWebRequestRest(string host, bool isWebserviceVersion)
        {
            string url = host;
            Uri uri = new Uri(url);
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(host) as HttpWebRequest;
                HttpClient client = new HttpClient();
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    X509Certificate cert = GetCertificate(request);
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        client.BaseAddress = uri;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response1 = client.GetAsync(uri).Result;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        string data = response1.Content.ReadAsStringAsync().Result;
                        return GetData(response1, isWebserviceVersion, data, cert);
                    }
                    return @"{ ex: '" + "krijg geen data terug" + "'}";
                }
            }
            catch (Exception ex)
            {
                return @"{ ex: '" + ex.Message.ToString() + "'}";
            }
        }

        private string GetData(HttpResponseMessage response1,
                               bool isWebserviceVersion,
                               string data,
                               X509Certificate cert)
        {
            if (response1.IsSuccessStatusCode)
            {
                if (_certIsGoed)
                {
                    if (isWebserviceVersion)
                    {
                        return GetWebserviceVersionDataOfWebRequest(data, cert.GetExpirationDateString().ToString());
                    }
                    else
                    {
                        return GetDataOfWebRequest(data, cert.GetExpirationDateString().ToString());
                    }
                }
                else
                {
                    if (isWebserviceVersion)
                    {
                        return GetWebserviceVersionDataOfWebRequest(data, "");
                    }
                    else
                    {
                        return GetDataOfWebRequest(data, "");
                    }
                }
            }
            return @"{ ex: '" + "krijg geen data terug" + "'}";
        }

        private string GetDataOfWebRequest(string data, string verValDatum = "Niet goed")
        {
            int Pos1 = data.IndexOf('{');
            int Pos2 = data.IndexOf('}');
            data = data.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
            if (_certIsGoed)
            {
                return "{" + data + ", \"certVerValDatum\": " + "\"" + verValDatum + "\"" + "}";
            }
            return data;
        }

        private string GetWebserviceVersionDataOfWebRequest(string data, string verValDatum = "")
        {
            int positionKraanDll = data.IndexOf("KraanDLL");
            int positionKraanIni = data.IndexOf("Kraan.ini");
            int positionDatabaseConnect = data.IndexOf("Database connectie");
            int positionDatabaseMelding = data.IndexOf("Database melding");

            string webserviceVersie = data.Substring(0, positionKraanDll);
            string kraanDll = data.Substring(positionKraanDll, positionKraanIni - positionKraanDll);
            string kraanIni = data.Substring(positionKraanIni, positionDatabaseConnect - positionKraanIni);
            string kraanDatabase = data.Substring(positionDatabaseConnect, positionDatabaseMelding - positionDatabaseConnect);

            return @"{ WebserviceVersie: '" + webserviceVersie + "', KraanDll: '" + kraanDll + "', KraanIni: '" + kraanIni + "', KraanDatabase: '" + kraanDatabase + "', certVerValDatum: '" + verValDatum + "'}";
        }

        public string CheckUrl(string host)
        {
            Uri uri = new Uri(host);
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response1 = client.GetAsync(uri).Result;
                if (response1.IsSuccessStatusCode)
                {
                    return "true";
                }
                return "false met statuscode: " + response1.StatusCode;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private X509Certificate GetCertificate(HttpWebRequest request)
        {
            X509Certificate cert = request.ServicePoint.Certificate;
            X509Certificate2 cert2 = null;
            if (cert != null)
            {
                cert2 = new X509Certificate2(cert);
                _certIsGoed = cert2.Verify();
            }
            return cert;
        }


        public string GetWebRequestSoap(string host, string service)
        {
            string result = string.Empty;

            switch (service)
            {
                case "AuthService.svc":
                    return CheckUrlAuthService(host);
                case "CrmService.svc":
                    result = GetVersionCrmService(host);
                    break;
                case "WorkflowService.svc":
                    result = GetVersionWorkFlowService(host);
                    break;
                case "UrenService.svc":
                    result = GetVersionUrenService(host);
                    break;
                //case "MaterieelService.svc":
                //    result = GetVersionMaterieelService(host);
                //    break;
                //case "MaterieelbeheerService.svc":
                //    result = GetVersionMaterieelbeheerService(host);
                //    break;
                default:
                    return @"{ ex: '" + " deze service bestaat niet " + "'}"; ;

            }
            return result;
        }

        public string CheckUrlAuthService(string host)
        {
            Uri uri = new Uri(host);
            HttpClient client = new HttpClient();
            HttpWebRequest request = HttpWebRequest.Create(host) as HttpWebRequest;
            X509Certificate cert = GetCertificate(request);
            try
            {
                HttpResponseMessage response1 = client.GetAsync(uri).Result;
                if (response1.IsSuccessStatusCode)
                {
                    if (_certIsGoed)
                    {
                        return @"{ status: '" + "Werkt" + "', certVerValDatum: '" + cert.GetExpirationDateString().ToString() + "'}";
                    }
                    else
                    {
                        return @"{ status: '" + "Werkt" + "', certVerValDatum: '" + "Niet goed" + "'}";
                    }
                }
                return "Werkt niet met statuscode: " + (int)response1.StatusCode + " = " + response1.StatusCode;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string GetVersionCrmService(string host)
        {
            string result;

            CrmWebservice.CrmServiceClient clientCrm;
            clientCrm = NewCrmService(host);
            clientCrm.Open();
            try
            {
                result = clientCrm.GetVersion();
                result = GetDataOfWebRequestSoap(result, host);
            }
            catch (Exception ex)
            {
                result = @"{ ex: '" + ex.Message.ToString() + "'}";
            }
            clientCrm.Close();
            return result;
        }

        private string GetVersionWorkFlowService(string host)
        {
            string result;

            WorkFlowWebservice.WorkflowServiceClient clientWorkflow;
            clientWorkflow = NewWorkFlowService(host);
            clientWorkflow.Open();
            try
            {
                result = clientWorkflow.GetVersion();

                HttpWebRequest request = HttpWebRequest.Create(host) as HttpWebRequest;
                X509Certificate cert = GetCertificate(request);

                result = GetDataOfWebRequestSoap(result, host);
            }
            catch (Exception ex)
            {
                result = @"{ ex: '" + ex.Message.ToString() + "'}"; ;
            }
            clientWorkflow.Close();
            return result;
        }

        private string GetVersionUrenService(string host)
        {
            string result;

            UrenWebservice.UrenServiceClient clientUren;
            clientUren = NewUrenService(host);
            clientUren.Open();
            try
            {
                result = clientUren.GetVersion();
                result = GetDataOfWebRequestSoap(result, host);
            }
            catch (Exception ex)
            {
                result = @"{ ex: '" + ex.Message.ToString() + "'}"; ;
            }
            clientUren.Close();
            return result;
        }


        //private string GetVersionMaterieelService(string host)
        //{
        //    string result;

        //    MaterieelWebservice.MaterieelServiceClient clientMaterieel;
        //    clientMaterieel = NewMateriaalService(host);
        //    clientMaterieel.Open();
        //    try
        //    {
        //        result = clientMaterieel.GetVersion();
        //        result = GetDataOfWebRequestSoap(result, host);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = @"{ ex: '" + ex.Message.ToString() + "'}"; ;
        //    }
        //    clientMaterieel.Close();
        //    return result;
        //}

        //private string GetVersionMaterieelbeheerService(string host)
        //{
        //    string result;
        //    MaterieelBeheerWebservice.MaterieelBeheerServiceClient clientMaterieelbeheer;
        //    clientMaterieelbeheer = NewMaterieelbeheerService(host);
        //    clientMaterieelbeheer.Open();
        //    try
        //    {
        //        result = clientMaterieelbeheer.GetVersion();
        //        result = GetDataOfWebRequestSoap(result, host);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = @"{ ex: '" + ex.Message.ToString() + "'}"; ;
        //    }
        //    clientMaterieelbeheer.Close();
        //    return result;
        //}

        private CrmWebservice.CrmServiceClient NewCrmService(string host)
        {
            BasicHttpBinding binding = CreateBinding("CrmService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new CrmWebservice.CrmServiceClient(binding, epa);
        }

        //private MaterieelWebservice.MaterieelServiceClient NewMateriaalService(string host)
        //{
        //    BasicHttpBinding binding = CreateBinding("MaterieelService");
        //    EndpointAddress epa = CreateEndpointAddress(host, "");

        //    return new MaterieelWebservice.MaterieelServiceClient(binding, epa);
        //}

        //private MaterieelBeheerWebservice.MaterieelBeheerServiceClient NewMaterieelbeheerService(string host)
        //{
        //    BasicHttpBinding binding = CreateBinding("Materieelbeheer");
        //    EndpointAddress epa = CreateEndpointAddress(host, "");

        //    return new MaterieelBeheerWebservice.MaterieelBeheerServiceClient(binding, epa);
        //}

        private UrenWebservice.UrenServiceClient NewUrenService(string host)
        {
            BasicHttpBinding binding = CreateBinding("UrenService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new UrenWebservice.UrenServiceClient(binding, epa);
        }

        private WorkFlowWebservice.WorkflowServiceClient NewWorkFlowService(string host)
        {
            BasicHttpBinding binding = CreateBinding("WorkflowService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new WorkFlowWebservice.WorkflowServiceClient(binding, epa);
        }

        private Sales24.MessageServiceSoapClient NewSales24Client(string host)
        {

            BasicHttpBinding binding = CreateBinding("MessageServiceSoap");
            EndpointAddress epa = CreateEndpointAddress(host, "messageservicesoap.svc");

            return new Sales24.MessageServiceSoapClient(binding, epa);
        }

        private BasicHttpBinding CreateBinding(string bindingName)
        {
            BasicHttpBinding serviceBinding = new BasicHttpBinding();
            serviceBinding.Name = bindingName;
            serviceBinding.Security.Mode = BasicHttpSecurityMode.Transport;
            serviceBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            return serviceBinding;
        }

        private EndpointAddress CreateEndpointAddress(string host, string endPointName = "")
        {
            string endPointString = host;
            if (host.ToLower().Contains("messageservicesoap.svc") || host.ToLower().Contains("messageservicesoap31.svc"))
            { return new EndpointAddress(host); }
            if (endPointString.Last() != '/')
            {
                endPointString += '/';
            }
            endPointString = endPointString + endPointName;

            EndpointAddress epa = new EndpointAddress(endPointString);
            return epa;
        }

        public dynamic Get24SalesData(string host)
        {
            using (Sales24.MessageServiceSoapClient client = NewSales24Client(host))
            {
                try
                {
                    client.Open();
                    Sales24.MessageServiceMessage message = new Sales24.MessageServiceMessage();
                    message.MsgType = "CST_KRAAN_VERSION";
                    bool succes = client.PostMessage(null, null, ref message);

                    HttpWebRequest request = HttpWebRequest.Create(host) as HttpWebRequest;
                    X509Certificate cert = GetCertificate(request);

                    if (_certIsGoed)
                    {
                        string data = "{\"" + message.Text[0]
                        .Replace("\r\n", "\",\"")
                        .Replace(": ", "\": \"")
                        .Replace(@"\", " ")
                        .Replace("Versie\": \"", "Versie: ") + "\", \"certVerValDatum\": " + "\"" + cert.GetExpirationDateString().ToString() + "\"" + "}";
                        client.Close();
                        return JObject.Parse(data);
                    }
                    else
                    {
                        string data = "{\"" + message.Text[0]
                        .Replace("\r\n", "\",\"")
                        .Replace(": ", "\": \"")
                        .Replace(@"\", " ")
                        .Replace("Versie\": \"", "Versie: ") + "\", \"certVerValDatum\": " + "\"" + "Niet goed" + "\"" + "}";
                        client.Close();
                        return JObject.Parse(data);
                    }
                }
                catch (Exception ex)
                {
                    return @"{ ex: '" + ex.Message.ToString() + "'}";
                }
                
            }
        }

        private string GetDataOfWebRequestSoap(string result, string host)
        {
            HttpWebRequest request = HttpWebRequest.Create(host) as HttpWebRequest;
            X509Certificate cert = GetCertificate(request);

            string data = result.Replace("----", "");
            int positionWebserviceVersie = data.IndexOf("Webservice versie");
            int positionDevExpressVersie = data.IndexOf("DevExpress versie");
            int positionDatabaseVersie = data.IndexOf("DatabaseVersie");

            string webserviceVersie = data.Substring(positionWebserviceVersie, positionDevExpressVersie - positionWebserviceVersie);
            string devExpressVersie = data.Substring(positionDevExpressVersie, positionDatabaseVersie - positionDevExpressVersie);
            string dataBaseVersie = data.Substring(positionDatabaseVersie, data.Length - positionDatabaseVersie);

            if (cert == null)
            {
                return "{ \"Webservice Versie\": " + "\"" + webserviceVersie.Split(':')[1]
                + "\"" + ", \"DevExpress versie\": " + "\"" + devExpressVersie.Split(':')[1]
                + "\"" + ", \"DatabaseVersie\": " + "\"" + dataBaseVersie.Split(':')[1]
                + "\"" + ", \"certVerValDatum\": " + "\"" + "Niet goed" + "\"" + "}";
            }
            else
            {
                return "{ \"Webservice Versie\": " + "\"" + webserviceVersie.Split(':')[1]
                + "\"" + ", \"DevExpress versie\": " + "\"" + devExpressVersie.Split(':')[1]
                + "\"" + ", \"DatabaseVersie\": " + "\"" + dataBaseVersie.Split(':')[1]
                + "\"" + ", \"certVerValDatum\": " + "\"" + cert.GetExpirationDateString().ToString() + "\"" + "}";
            }
        }
    }
}
