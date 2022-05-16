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

        string[] kraanWebservices = { "AuthService.svc",
                                      "CrmService.svc",
                                      "WorkflowService.svc",
                                      "MaterieelService.svc",
                                      "MaterieelbeheerService.svc",
                                      "UrenService.svc" };

        UrlXml _urlXml;
        KlantXml _klantXml;
        WebserviceXml _webserviceXml;
        KlantWebserviceXml _klantWebserviceXml;

        WebRequest _webRequest;
        Timer _timer;
        KrXml _krXml;
        LogFile _logFile = new LogFile();

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
                            if (klantWebservice.BasisUrl1)
                            {
                                basisUrl = klant.BasisUrl1;
                            }
                            else
                            {
                                basisUrl = klant.BasisUrl2;
                            }
                            foreach (Webservice webservice in _webservices)
                            {
                                if (webservice.Id == klantWebservice.Webservice)
                                {
                                    webService = webservice;
                                    url.Name = basisUrl + webservice.Name;
                                    _isSoap = webservice.Soap;
                                }
                            }
                            string urlforWebservice = url.Name;
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
                            if (webService.Name == "Kraan2Webservice")
                            {
                                UrlsTestKraan2Webservice(urlforWebservice);
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
                        MailClient.TestMail("TestAll", _text, _logFile.FilePath, _krXmlData.Email);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _logFile.AddTextToLogFile(ex.Message);
                }
                finally
                {
                    _urls.Clear();
                    _webservices.Clear();
                    _klanten.Clear();
                }
            }
            _timer.Start();
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

        private void UrlsTestKraan2Webservice(string urlName)
        {
            for (int i = 0; i < kraanWebservices.Length; i++)
            {
                Url newUrl = new Url();
                newUrl.Name = urlName + "/" + kraanWebservices[i];
                GetUrl(newUrl);
                _logFile.AddTextToLogFile("\n");
            }
        }

        private void CheckUrl(Url url)
        {
            string checkUrl = _webRequest.CheckUrl(url.Name);
            if (checkUrl.StartsWith("false"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = X " + checkUrl + "\n";
                _logFile.AddTextToLogFile(url.Name + "--> Webservice = X " + checkUrl);
            }
            else if (checkUrl.StartsWith("true"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = ✓" + "\n";
                _logFile.AddTextToLogFile(url.Name + " --> Webservice = ✓");
            }
            else
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> ex = " + checkUrl + "\n";
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
                    _result = _webRequest.Get24SalesData(url.Name);
                }
                else
                {
                    int plaatsSlech = url.Name.LastIndexOf("/");
                    string service = url.Name.Substring(plaatsSlech + 1, url.Name.Length - plaatsSlech - 1);
                    _result = JObject.Parse(_webRequest.GetWebRequestSoap(url.Name, service));
                }
            }
            else
            {
                _result = JObject.Parse(_webRequest.GetWebRequestRest(url.Name, isWebserviceVersion));
            }

            foreach (JProperty item in _result)
            {
                if (item.Name != "id")
                {
                    _logFile.AddTextToLogFile(item.Name + " = " + item.Value.ToString());
                }
                if (item.Name == "ex")
                {
                    _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> " + item.Value.ToString();
                }
            }
        }
    }
}
