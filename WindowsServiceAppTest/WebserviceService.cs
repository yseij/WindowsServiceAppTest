using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        private int eventId = 1;
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
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
            eventLog1.WriteEntry("inWebservice", EventLogEntryType.Information, eventId++);

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
            eventLog1.WriteEntry("OnStart", EventLogEntryType.Information, eventId++);
            _krXmlData = _krXml.GetDataOfXmlFile();
            _timer.Interval = _krXmlData.TijdService;
            _timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            _timer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            eventLog1.WriteEntry("OnTimer", EventLogEntryType.Information, eventId++);
            _timer.Stop();
            _krXmlData = _krXml.GetDataOfXmlFile();
            _timer.Interval = _krXmlData.TijdService;
            string text = string.Empty;
            string webserviceName = string.Empty;
            string urlHttp = string.Empty;
            bool isSoap = false;
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
                        foreach (HttpData httpData in _httpDatas)
                        {
                            if (urlData.HttpDataId == httpData.Id)
                            {
                                logFile.AddTextToLogFile("Http --> " + httpData.Name);
                                logFile.AddTextToLogFile("---");
                                urlHttp = httpData.Name;
                            }
                        }
                        foreach (WebServiceData webServiceData in _webServiceDatas)
                        {
                            if (urlData.WebServiceDataId == webServiceData.Id)
                            {
                                logFile.AddTextToLogFile("---");
                                logFile.AddTextToLogFile("Webservice --> " + webServiceData.Name);

                                webserviceName = webServiceData.Name;
                                isSoap = webServiceData.Soap;
                            }
                        }
                        foreach (KlantData klantData in _klantenDatas)
                        {
                            if (urlData.KlantDataId == klantData.Id)
                            {
                                logFile.AddTextToLogFile("Klant --> " + klantData.Name);
                                logFile.AddTextToLogFile("---");
                            }
                        }
                        if (isSoap && urlData.Name.EndsWith(".svc"))
                        {
                            //if (urlData.Name == "MessageServiceSoap31.svc")
                            //{
                            //    var m = new Sales31CredentialsForm();
                            //    m.TopMost = true;
                            //    m.ShowDialog();
                            //    MaterialMaskedTextBox userName = m._usernameTxtBx;
                            //    MaterialMaskedTextBox password = m._passwordTxtBx;
                            //    _result = _webRequest.Get31SalesData(_httpName + _webserviceName, userName, password, ResponseTextBox);
                            //}
                            else if (urlData.Name == "MessageServiceSoap.svc")
                            {
                                _result = _webRequest.Get24SalesData(_httpName + _webserviceName, ResponseTextBox);

                            }
                            else
                            {
                                string data = _webRequest.GetWebRequestSoap(_httpName, _webserviceName, urlData.Name);
                                _result = JObject.Parse(data);
                            }
                        }
                        _result = JObject.Parse(_webRequest.GetWebRequest(urlData.Id, urlHttp, webserviceName, urlData.Name, urlData.SecurityId));
                        foreach (JProperty item in _result)
                        {
                            if (item.Name != "id")
                            {
                                logFile.AddTextToLogFile(item.Name + " = " + item.Value.ToString());
                            }
                            if (item.Name == "ex")
                            {
                                text += urlData.Name + " --> " + item.Value.ToString() + Environment.NewLine;
                            }
                        }
                    }
                    if (text != "")
                    {
                        MailClient.TestMail("TestAll", text, logFile.FilePath, _krXmlData.Email);
                    }
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
            eventLog1.WriteEntry("GetUrls", EventLogEntryType.Information, eventId++);
            _urlDatas = _urltest.GetUrls(eventLog1);
        }

        private void GetWebservices()
        {
            _webServiceDatas = _webserviceTest.GetWebServiceDatas(true);
        }

        private void GetKlanten()
        {
            _klantenDatas = _klantTest.GetKlantData();
        }

        private void GetHttp()
        {
            _httpDatas = _httptest.GetHttpData();
        }
    }
}
