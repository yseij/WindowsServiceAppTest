using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Timers;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    public partial class WebserviceService : ServiceBase
    {
        private List<Url> _urls = new List<Url>();
        private List<Webservice> _webservices = new List<Webservice>();
        private List<Klant> _klanten = new List<Klant>();
        private List<KlantWebservice> _klantWebservices = new List<KlantWebservice>();
        private KrXmlData _krXmlData = new KrXmlData();

        private string _text = string.Empty;
        private bool _isSoap = false;
        private dynamic _result;
        private string _https = "https:";
        private string _http = "http:";

        string[] kraan2Webservices = { "AuthService.svc",
                                      "CrmService.svc",
                                      "WorkflowService.svc",
                                      "MaterieelService.svc",
                                      "MaterieelbeheerService.svc",
                                      "UrenService.svc" };

        string[] kraanSalesService = { "MessageServiceSoap.svc",
                                       "MessageServiceSoap31.svc"};

        UrlXml _urlXml;
        KlantXml _klantXml;
        WebserviceXml _webserviceXml;
        KlantWebserviceXml _klantWebserviceXml;

        WebRequest _webRequest;
        Timer _timer;
        KrXml _krXml;
        LogFile _logFile;

        public WebserviceService()
        {

            InitializeComponent();
            _timer = new Timer();
            _webRequest = new WebRequest();
            _krXml = new KrXml();

            _urlXml = new UrlXml();
            _klantXml = new KlantXml();
            _webserviceXml = new WebserviceXml();
            _klantWebserviceXml = new KlantWebserviceXml();
        }

        protected override void OnStart(string[] args)
        {
            _krXmlData = _krXml.GetDataOfXmlFile();
            _timer.Interval = _krXmlData.TijdService;
            _timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            _timer.Start();
        }

        protected override void OnStop()
        {
        }

        protected override void OnContinue()
        {
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            _logFile = new LogFile();
            _timer.Stop();
             _krXmlData = _krXml.GetDataOfXmlFile();
            _timer.Interval = _krXmlData.TijdService;
            if (_krXmlData.ServiceAanOfUit == "aan")
            {
                try
                {
                    GetUrls();
                    GetWebservices();
                    GetKlanten();
                    GetKlantWebservices();

                    _logFile.MakeLogFile("AllTestDoorService", _krXmlData.SaveLogFilePlace);
                    foreach (Klant klant in _klanten)
                    {
                        string basisUrl = string.Empty;
                        Url url = new Url();
                        Webservice webService = new Webservice();
                        List<KlantWebservice> klantWebservices = _klantWebservices.FindAll(k => k.Klant == klant.Id);
                        _logFile.AddTitleToLogFile(klant.Name);
                        foreach (KlantWebservice klantWebservice in klantWebservices)
                        {
                            foreach (Webservice webservice in _webservices)
                            {
                                if (webservice.Id == klantWebservice.Webservice)
                                {
                                    webService = webservice;
                                    url.Name = basisUrl + webservice.Name;
                                    _isSoap = webservice.Soap;
                                }
                            }
                            if (klantWebservice.BasisUrl1)
                            {
                                basisUrl = klant.BasisUrl1;
                                url.Name = basisUrl + webService.Name;
                                SoapOfRestTest(url, webService, klantWebservice);
                            }
                            if (klantWebservice.BasisUrl2)
                            {
                                basisUrl = klant.BasisUrl2;
                                url.Name = basisUrl + webService.Name;
                                SoapOfRestTest(url, webService, klantWebservice);
                            }
                        }
                        foreach (Url url1 in _urls)
                        {
                            if (url1.KlantId == klant.Id)
                            {
                                KlantWebservice klantWebservice = _klantWebserviceXml.GetByKlantWebserviceId(url1.KlantWebserviceId, _krXmlData.PlaceDb);
                                string basisUrl1 = FindBasisUrl(klantWebservice, klant);
                                Webservice webservice = FindWebservice(klantWebservice);
                                Url newUrl = new Url();
                                newUrl.Id = url1.Id;
                                newUrl.Name = basisUrl + webService.Name + "/" + url1.Name;
                                newUrl.KlantId = url1.KlantId;
                                newUrl.KlantWebserviceId = klantWebservice.Id;
                                GetUrl(newUrl);
                                _logFile.AddTextToLogFile("\n");
                            }
                        }
                    }
                    if (_text != "")
                    {
                        MailClient.TestMail("TestAll", _text, _logFile.FilePath, _krXmlData);
                    }
                }
                catch (Exception ex)
                {
                    _logFile.AddTextToLogFile(ex.Message);
                }
                finally
                {
                    _text = string.Empty;
                    _urls.Clear();
                    _webservices.Clear();
                    _klanten.Clear();
                    _klantWebservices.Clear();
                }
            }
            _timer.Start();
        }

        private void SoapOfRestTest(Url url, Webservice webService, KlantWebservice klantWebservice)
        {
            if (webService.Name == "KraanHomeDNA")
            {
                url.Name += "/HomeDna.svc/GetWebserviceVersion";
                GetUrl(url);
            }
            if (webService.Name == "Kraan2Webservices")
            {
                UrlsTestKraan2Webservice(url);
            }
            else if (webService.Name == "KraanSalesService")
            {
                UrlsTestKraanSalesService(url, klantWebservice);
            }
            else if (webService.Name == "KraanWerkbonWebservice")
            {
                url.Name += "/Webservice.svc";
                GetUrl(url);
            }
            else if (webService.Name == "KraanHandheld")
            {
                url.Name += "/HandheldService.svc/rest/GetVersion";
                GetUrl(url);
            }
            else if (!_isSoap)
            {
                CheckUrlAndGetWebserviceVersion(url);
            }
            else
            {
                GetUrl(url);
            }
        }

        private void UrlsTestKraanSalesService(Url url, KlantWebservice klantWebservice)
        {
            for (int i = 0; i < kraanSalesService.Length; i++)
            {
                Url newUrl = new Url();
                newUrl.Name = url.Name + "/" + kraanSalesService[i];
                newUrl.KlantWebserviceId = klantWebservice.Id;
                GetUrl(newUrl);
            }
        }

        private void UrlsTestKraan2Webservice(Url url)
        {
            for (int i = 0; i < kraan2Webservices.Length; i++)
            {
                Url newUrl = new Url();
                newUrl.Name = url.Name + "/" + kraan2Webservices[i];
                GetUrl(newUrl);
                newUrl.Name = string.Empty;
            }
        }

        private void CheckUrlAndGetWebserviceVersion(Url url)
        {
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    CheckUrl(url);
                }
                else
                {
                    GetWebserviceVersion(url);
                    _logFile.AddTextToLogFile("\n");
                }
            }
        }

        private string FindBasisUrl(KlantWebservice klantWebservice, Klant klant)
        {
            if (klantWebservice.BasisUrl1)
            {
                return klant.BasisUrl1;
            }
            else
            {
                return klant.BasisUrl2;
            }
        }

        private Webservice FindWebservice(KlantWebservice klantWebservice)
        {
            List<Webservice> webServices = _webserviceXml.GetAll(_krXmlData.PlaceDb);
            foreach (Webservice webservice in webServices)
            {
                if (webservice.Id == klantWebservice.Webservice)
                {
                    _isSoap = webservice.Soap;
                    return webservice;
                }
            }
            return null;
        }

        private void CheckUrl(Url url)
        {
            string checkUrl = _webRequest.CheckUrl(url.Name);
            if (checkUrl.StartsWith("false"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = X " + checkUrl + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + "--> Webservice = X " + checkUrl);
            }
            else if (checkUrl.StartsWith("true"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = ✓" + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + " --> Webservice = ✓");
            }
            else
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> ex = " + checkUrl + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + " --> ex = " + checkUrl);
            }
        }

        private void GetWebserviceVersion(Url url)
        {
            if (!_isSoap)
            {
                url.Name += "/GetWebserviceVersion";
            }
            _logFile.AddTextToLogFile(url.Name);
            CheckResult(url, true);
        }

        private void GetUrl(Url url)
        {
            _logFile.AddTextToLogFile(url.Name);
            CheckResult(url, false);
        }

        private void GetUrls()
        {
            _urls = _urlXml.GetAll(_krXmlData.PlaceDb);
        }

        private void GetWebservices()
        {
            _webservices = _webserviceXml.GetAll(_krXmlData.PlaceDb);
        }

        private void GetKlanten()
        {
            _klanten = _klantXml.GetAll(_krXmlData.PlaceDb);
        }

        private void GetKlantWebservices()
        {
            _klantWebservices = _klantWebserviceXml.GetAll(_krXmlData.PlaceDb);
        }

        private void CheckResult(Url url, bool isWebserviceVersion)
        {
            if (_isSoap && url.Name.EndsWith(".svc"))
            {
                if (url.Name.Contains("MessageServiceSoap31.svc"))
                {
                    _result = JObject.Parse(@"{ ex: '" + "Deze service heeft een inlog nodig dus die moet je appart testen" + "'}");
                }
                else if (url.Name.Contains("MessageServiceSoap.svc"))
                {
                    try
                    {
                        _result = _webRequest.Get24SalesData(url.Name);
                    }
                    catch (Exception ex)
                    {
                        _text += "ging iets mis met het testen van de volgende url:" + url.Name.Replace(_https, "").Replace(_http, "") + " --> " + ex.Message + Environment.NewLine;
                        _logFile.AddTextToLogFile("ging iets mis met het testen van de volgende url:" + url.Name + " --> " + ex.Message);
                    }
                }
                else
                {
                    int plaatsSlech = url.Name.LastIndexOf("/");
                    string service = url.Name.Substring(plaatsSlech + 1, url.Name.Length - plaatsSlech - 1);
                    try
                    {
                        _result = JObject.Parse(_webRequest.GetWebRequestSoap(url.Name, service));
                    }
                    catch (Exception ex)
                    {
                        _text += "ging iets mis met het testen van de volgende url:" + url.Name.Replace(_https, "").Replace(_http, "") + " --> " + ex.Message + Environment.NewLine;
                        _logFile.AddTextToLogFile("ging iets mis met het testen van de volgende url:" + url.Name + " --> " + ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    _result = JObject.Parse(_webRequest.GetWebRequestRest(url.Name, isWebserviceVersion));
                }
                catch (Exception ex)
                {
                    _text += "ging iets mis met het testen van de volgende url:" + url.Name.Replace(_https, "").Replace(_http, "") + " --> " + ex.Message + Environment.NewLine;
                    _logFile.AddTextToLogFile("ging iets mis met het testen van de volgende url:" + url.Name + " --> " + ex.Message);
                }
            }

            foreach (JProperty item in _result)
            {
                if (item.Name != "id")
                {
                    _logFile.AddTextToLogFile(item.Name + " = " + item.Value.ToString());
                }
                if (item.Name == "ex")
                {
                    _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> " + item.Value.ToString() + Environment.NewLine;
                }
            }
        }
    }
}
