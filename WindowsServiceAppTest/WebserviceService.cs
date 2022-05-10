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
                            for (int i = 0; i < 2; i++)
                            {
                                if (i == 0)
                                {
                                    CheckUrl(url);
                                }
                                else
                                {
                                    GetWebserviceVersion(url);
                                    _logFile.AddTextToLogFile(Environment.NewLine);
                                }
                            }
                        }
                        foreach (Url url1 in _urls)
                        {
                            if (url1.KlantId == klant.Id)
                            {
                                GetUrl(url1);
                                _logFile.AddTextToLogFile(Environment.NewLine);
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

        private void CheckUrl(Url url)
        {
            string checkUrl = _webRequest.CheckUrl(url.Name);
            if (checkUrl.StartsWith("false"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = " + checkUrl + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + " --> Webservice = " + checkUrl + Environment.NewLine);
            }
            else if (checkUrl.StartsWith("true"))
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> Webservice = true" + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + " --> Webservice = true" + Environment.NewLine);
            }
            else
            {
                _text += url.Name.Replace(_https, "").Replace(_http, "") + " --> ex = " + checkUrl + Environment.NewLine;
                _logFile.AddTextToLogFile(url.Name + " --> ex = " + checkUrl + Environment.NewLine);
            }
        }

        private void GetWebserviceVersion(Url url)
        {
            if (!_isSoap)
            {
                url.Name += "/GetWebserviceVersion";
            }
            _logFile.AddTextToLogFile(url.Name.Replace(_https, "").Replace(_http, ""));
            CheckResult(url, true);
        }

        private void GetUrl(Url url)
        {
           
            _logFile.AddTextToLogFile(url.Name + Environment.NewLine);
            CheckResult(url, false);
        }

        private void GetUrls()
        {
            _urls = _urlXml.GetAll();
        }

        private void GetWebservices()
        {
            _webservices = _webserviceXml.GetAll();
        }

        private void GetKlanten()
        {
            _klanten = _klantXml.GetAll();
        }

        private void GetKlantWebservices()
        {
            _klantWebservices = _klantWebserviceXml.GetAll();
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
                _result = JObject.Parse(_webRequest.GetWebRequestRest(url.Id, url.Name, isWebserviceVersion));
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
