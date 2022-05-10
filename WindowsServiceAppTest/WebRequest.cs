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

        public string GetWebRequestRest(Guid id, string host, bool isWebserviceVersion)
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
                        if (response1.IsSuccessStatusCode)
                        {
                            //Parse the response body.
                            //Make sure to add a reference to System.Net.Http.Formatting.dll
                            if (isWebserviceVersion)
                            {
                                if (_certIsGoed)
                                {
                                    return GetDataOfWebRequest(data, cert.GetExpirationDateString().ToString());
                                }
                                else
                                {
                                    return GetDataOfWebRequest(data, "");
                                }
                            }
                            else
                            {
                                int Pos1 = data.IndexOf('{');
                                int Pos2 = data.IndexOf('}');
                                data = data.Substring(Pos1 + 1, Pos2 - Pos1 - 1);
                                if (_certIsGoed)
                                {
                                    return "{" + data + "', certVerValDatum: '" + cert.GetExpirationDateString().ToString() + "'}";
                                }
                            }
                        }
                    }
                    return @"{ ex: '" + "krijg geen data terug" + "'}";
                }
            }
            catch (Exception ex)
            {
                return @"{ ex: '" + ex.Message.ToString() + "'}";
            }
        }

        private string GetDataOfWebRequest(string data, string verValDatum = "")
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
            string result = "";

            //YouriWebserviceCrm.CrmServiceClient clientAuth;
            YouriWebserviceCrm.CrmServiceClient clientCrm;
            YouriWebserviceWorkFlow.WorkflowServiceClient clientWorkflow;
            YouriWebserviceUren.UrenServiceClient clientUren;
            //YouriWebserviceMaterieel.Ma clientMaterieel;
            //YouriWebserviceWeb.WebServiceClient clientWeb;

            switch (service)
            {
                //case "AuthService.svc":
                //    clientAuth = NewAuthService(host);
                //    clientAuth.Open();
                //    result = clientAuth.GetVersion().ToString();
                //    clientAuth.Close();
                //    break;
                case "CrmService.svc":
                    clientCrm = NewCrmService(host);
                    clientCrm.Open();
                    result = clientCrm.GetVersion();
                    clientCrm.Close();
                    break;
                case "WorkflowService.svc":
                    clientWorkflow = NewWorkFlowService(host);
                    clientWorkflow.Open();
                    result = clientWorkflow.GetVersion();
                    clientWorkflow.Close();
                    break;
                case "UrenService.svc":
                    clientUren = NewUrenService(host);
                    clientUren.Open();
                    result = clientUren.GetVersion();
                    clientUren.Close();
                    break;
                //case "MaterieelService.svc":
                //    clientMaterieel = NewMateriaalService(host);
                //    clientMaterieel.Open();
                //    result = clientMaterieel.GetVersion();
                //    clientMaterieel.Close();
                //    break;
                //case "Webservice.svc":
                //    clientMaterieel = NewWebSerivce(host);
                //    clientMaterieel.Open();
                //    result = clientMaterieel.GetVersion().ToString();
                //    clientMaterieel.Close();
                //    break;
                default:
                    return @"{ ex: '" + " deze service bestaat niet " + "'}"; ;

            }
            return GetDataOfWebRequestSoap(result);
        }

        //private YouriWebserviceAuth.AuthServiceClient NewAuthService(string host)
        //{
        //    BasicHttpBinding binding = CreateBinding("AuthService");
        //    EndpointAddress epa = CreateEndpointAddress(host, "AuthService.svc");

        //    return new YouriWebserviceAuth.AuthServiceClient(binding, epa);
        //}

        private YouriWebserviceCrm.CrmServiceClient NewCrmService(string host)
        {
            BasicHttpBinding binding = CreateBinding("CrmService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new YouriWebserviceCrm.CrmServiceClient(binding, epa);
        }

        //private YouriWebserviceMaterieel.MaterieelServiceClient NewMateriaalService(string host)
        //{
        //    BasicHttpBinding binding = CreateBinding("MaterieelService");
        //    EndpointAddress epa = CreateEndpointAddress(host, "MaterieelService.svc");

        //    return new YouriWebserviceMaterieel.MaterieelServiceClient(binding, epa);
        //}

        private YouriWebserviceUren.UrenServiceClient NewUrenService(string host)
        {
            BasicHttpBinding binding = CreateBinding("UrenService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new YouriWebserviceUren.UrenServiceClient(binding, epa);
        }

        private YouriWebserviceWorkFlow.WorkflowServiceClient NewWorkFlowService(string host)
        {
            BasicHttpBinding binding = CreateBinding("WorkflowService");
            EndpointAddress epa = CreateEndpointAddress(host, "");

            return new YouriWebserviceWorkFlow.WorkflowServiceClient(binding, epa);
        }

        //private YouriWebserviceWorkFlow.WorkflowServiceClient NewWebSerivce(string host)
        //{
        //    BasicHttpBinding binding = CreateBinding("Webservice");
        //    EndpointAddress epa = CreateEndpointAddress(host, "Webservice.svc");

        //    return new YouriWebserviceWorkFlow.WorkflowServiceClient(binding, epa);
        //}

        private Sales24.MessageServiceSoapClient NewSales24Client(string host)
        {

            BasicHttpBinding binding = CreateBinding("MessageServiceSoap");
            EndpointAddress epa = CreateEndpointAddress(host, "messageservicesoap.svc");

            return new Sales24.MessageServiceSoapClient(binding, epa);
        }

        private Sales31.MessageServiceSoapClient NewSales31Client(string host)
        {
            BasicHttpBinding bindings = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            bindings.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            bindings.MaxReceivedMessageSize = 2147483647;
            var elements = bindings.CreateBindingElements();
            elements.Find<SecurityBindingElement>().EnableUnsecuredResponse = true;
            elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
            CustomBinding cusbinding = new CustomBinding(elements);
            EndpointAddress epa = CreateEndpointAddress(host, "messageservicesoap31.svc");

            return new Sales31.MessageServiceSoapClient(cusbinding, epa);
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
                string testResultaat = "Geen verbinding mogelijk.";
                client.Open();
                Sales24.MessageServiceMessage message = new Sales24.MessageServiceMessage();
                message.MsgType = "CST_KRAAN_VERSION";
                bool succes = client.PostMessage(null, null, ref message);

                testResultaat = "Er is een beveiligde verbinding gemaakt met de Sales Messageservice ..." + Environment.NewLine;
                //testResultaat += "\r\nURL: " + CreateEndpointAddress(host, "messageservicesoap.svc").Uri + Environment.NewLine;
                testResultaat += "\r\nURL: " + CreateEndpointAddress(host).Uri + Environment.NewLine;
                testResultaat = testResultaat + message.Text[0];
                if (message != null)
                {
                    var data = "{\"" + message.Text[0]
                    .Replace("\r\n", "\",\"")
                    .Replace(": ", "\": \"")
                    .Replace(@"\", " ")
                    .Replace("Versie\": \"", "Versie: ") + "\"}";

                    client.Close();
                    return JObject.Parse(data);
                }
                return null;
            }
        }

        public dynamic Get31SalesData(string host, string TxtBxUsername, string TxtBxPassword)
        {
            using (Sales31.MessageServiceSoapClient client = NewSales31Client(host))
            {
                string testResultaat = "Geen beveiligde verbinding mogelijk.\r\n";
                client.ClientCredentials.UserName.UserName = TxtBxUsername.Trim();
                client.ClientCredentials.UserName.Password = TxtBxPassword.Trim();
                client.Open();
                Sales31.MessageType message = new Sales31.MessageType();
                message.MsgProperties = new Sales31.MessagePropertiesType();
                message.MsgProperties.MsgType = "CST_KRAAN_VERSION";

                Sales31.MessageResponseType antwoord = client.PostMessage(null, message);
                if (antwoord.Message.MsgContent != null)
                {
                    testResultaat = "Er is een beveiligde verbinding gemaakt met de Sales Messageservice ..." + Environment.NewLine;
                    testResultaat += "\r\nURL: " + CreateEndpointAddress(host, "messageservicesoap31.svc").Uri;
                    testResultaat = testResultaat + antwoord.Message.MsgContent;
                    testResultaat = antwoord.Message.MsgContent;

                    var data = "{\""
                        + antwoord.Message.MsgContent.Trim()
                        .Replace("\r\n", "\", \"")
                        .Replace(": ", "\": \"")
                        .Replace(@"\", " ")
                        .Replace("application\": \"", "application: ")
                        .Replace("Versie\": \"", "Versie: ")
                        + "\"}";
                    client.Close();
                    return JObject.Parse(data);
                }
                client.Close();
                return null;
            }
        }

        private string GetDataOfWebRequestSoap(string result)
        {
            string data = result.Replace("----", "");
            int positionWebserviceVersie = data.IndexOf("Webservice versie");
            int positionDevExpressVersie = data.IndexOf("DevExpress versie");
            int positionDatabaseVersie = data.IndexOf("DatabaseVersie");

            string webserviceVersie = data.Substring(positionWebserviceVersie, positionDevExpressVersie - positionWebserviceVersie);
            string devExpressVersie = data.Substring(positionDevExpressVersie, positionDatabaseVersie - positionDevExpressVersie);
            string databaseVersie = data.Substring(positionDatabaseVersie, data.Length - positionDatabaseVersie);
            return "{ \"Webservice Versie\": " + "\"" + webserviceVersie.Split(':')[1] + "\"" + ", \"DevExpress versie\": " + "\"" + devExpressVersie.Split(':')[1] + "\"" + ", \"DatabaseVersie\": " + "\"" + devExpressVersie.Split(':')[1] + "\"" + "}";
        }
    }
}
