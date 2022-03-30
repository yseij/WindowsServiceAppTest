using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private string urlHttp = "https://ws.kraan.com:444/";
        private int eventId = 1;
        private dynamic _result;

        WebRequest _webRequest;
        UrlTest _urltest;
        WebserviceTest _webserviceTest;
        KlantTest _klantTest;
        Timer _timer;

        public WebserviceService()
        {
            InitializeComponent();
            _timer = new Timer();
            _webserviceTest = new WebserviceTest();
            _klantTest = new KlantTest();
            _urltest = new UrlTest();
            _webRequest = new WebRequest();
        }

        protected override void OnStart(string[] args)
        {
            _timer.Interval = 60000; // 60 seconds
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
            string text = "";
            try
            {
                GetUrls();
                GetWebservices();
                GetKlanten();

                LogFile logFile = new LogFile();
                logFile.MakeLogFile("AllTestDoorService");
                foreach (UrlData urlData in _urlDatas)
                {
                    logFile.AddTitleToLogFile(urlData.Name);
                    foreach (WebServiceData webServiceData in _webServiceDatas)
                    {
                        if (urlData.WebServiceDataId == webServiceData.Id)
                        {
                            logFile.AddTextToLogFile("---");
                            logFile.AddTextToLogFile("Webservice --> " + webServiceData.Name);
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
                    _result = JObject.Parse(_webRequest.GetWebRequest(urlData.Id, urlHttp, urlData.Name, urlData.SecurityId));
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
                    MailClient.TestMail("TestAll", text, logFile.FilePath);
                }
            }
            finally 
            {
                _timer.Start();
                _urlDatas.Clear();
                _webServiceDatas.Clear();
                _klantenDatas.Clear();
            }   
        }

        private void GetUrls()
        {
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
    }
}
