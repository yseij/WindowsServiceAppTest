using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    public partial class WebserviceService : ServiceBase
    {
        private List<UrlData> _urlDatas = new List<UrlData>();
        private List<WebServiceData> _webServiceDatas = new List<WebServiceData>();
        private List<KlantData> _klantenDatas = new List<KlantData>();
        private List<HttpData> _httpDatas = new List<HttpData>();
        private KrXmlData _krXmlData = new KrXmlData();

        string _text = string.Empty;
        string _webserviceName = string.Empty;
        string _urlHttp = string.Empty;
        bool _isSoap = false;
        private dynamic _result;

        HttpTest _httptest;
        WebRequest _webRequest;
        UrlTest _urltest;
        WebserviceTest _webserviceTest;
        KlantTest _klantTest;
        Timer _timer;
        KrXml _krXml;

        public WebserviceService()
        {

            InitializeComponent();
            _timer = new Timer();
            _httptest = new HttpTest();
            _webserviceTest = new WebserviceTest();
            _klantTest = new KlantTest();
            _urltest = new UrlTest();
            _webRequest = new WebRequest();
            _krXml = new KrXml();
        }

        protected override void OnStart(string[] args)
        {
            _krXmlData = _krXml.GetDataOfXmlFile();
            ConfigurationManager.AppSettings["connectieString"] = @"data source =" + _krXmlData.ServerNaam + "; Initial Catalog = KraanTestTool; Integrated Security = True";
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
                    GetHttp();

                    LogFile logFile = new LogFile();
                    logFile.MakeLogFile("AllTestDoorService", _krXmlData.SaveLogFilePlace);

                    foreach (UrlData urlData in _urlDatas)
                    {
                        logFile.AddTitleToLogFile(urlData.Name);

                        GetHttpOfUrl(urlData, logFile);
                        GetWebserviceOfUrl(urlData, logFile);
                        GetKlantOfUrl(urlData, logFile);
                        CheckResult(urlData, logFile);
                    }
                    if (_text != "")
                    {
                        MailClient.TestMail("TestAll", _text, logFile.FilePath, _krXmlData.Email);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    _urlDatas.Clear();
                    _webServiceDatas.Clear();
                    _klantenDatas.Clear();
                }
            }
            _timer.Start();
        }

        private void GetUrls()
        {
            _urlDatas = _urltest.GetUrls();
        }

        private void GetWebservices()
        {
            _webServiceDatas = _webserviceTest.GetWebServiceData();
        }

        private void GetKlanten()
        {
            _klantenDatas = _klantTest.GetKlantData();
        }

        private void GetHttp()
        {
            _httpDatas = _httptest.GetHttpData();
        }

        private void GetHttpOfUrl(UrlData urlData, LogFile logFile)
        {
            foreach (HttpData httpData in _httpDatas)
            {
                if (urlData.HttpDataId == httpData.Id)
                {
                    logFile.AddTextToLogFile("Http --> " + httpData.Name);
                    logFile.AddTextToLogFile("---");
                    _urlHttp = httpData.Name;
                }
            }
        }

        private void GetWebserviceOfUrl(UrlData urlData, LogFile logFile)
        {
            foreach (WebServiceData webServiceData in _webServiceDatas)
            {
                if (urlData.WebServiceDataId == webServiceData.Id)
                {
                    logFile.AddTextToLogFile("---");
                    logFile.AddTextToLogFile("Webservice --> " + webServiceData.Name);

                    _webserviceName = webServiceData.Name;
                    _isSoap = webServiceData.Soap;
                }
            }
        }

        private void GetKlantOfUrl(UrlData urlData, LogFile logFile)
        {
            foreach (KlantData klantData in _klantenDatas)
            {
                if (urlData.KlantDataId == klantData.Id)
                {
                    logFile.AddTextToLogFile("Klant --> " + klantData.Name);
                    logFile.AddTextToLogFile("---");
                }
            }
        }

        private void CheckResult(UrlData urlData, LogFile logFile)
        {
            if (_isSoap && urlData.Name.EndsWith(".svc"))
            {
                if (urlData.Name == "MessageServiceSoap31.svc")
                {
                    _result = JObject.Parse(@"{ ex: '" + "Deze service heeft een inlog nodig dus die moet je appart testen" + "'}");
                }
                else if (urlData.Name == "MessageServiceSoap.svc")
                {
                    _result = _webRequest.Get24SalesData(_urlHttp + _webserviceName);
                }
                else
                {
                    _result = JObject.Parse(_webRequest.GetWebRequestSoap(_urlHttp, _webserviceName, urlData.Name));
                }
            }
            else
            {
                _result = JObject.Parse(_webRequest.GetWebRequest(urlData.Id, _urlHttp, _webserviceName, urlData.Name, urlData.SecurityId));
            }

            foreach (JProperty item in _result)
            {
                if (item.Name != "id")
                {
                    logFile.AddTextToLogFile(item.Name + " = " + item.Value.ToString());
                }
                if (item.Name == "ex")
                {
                    _text += urlData.Name + " --> " + item.Value.ToString() + Environment.NewLine;
                }
            }
        }
    }
}
