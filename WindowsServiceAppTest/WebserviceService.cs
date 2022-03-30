using Newtonsoft.Json.Linq;
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

        private string urlHttp = "https://wsdev.kraan.com/";
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

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            _timer.Interval = 10000; // 60 seconds
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
            _timer.Stop();
            try
            {
                eventLog1.WriteEntry("OnTimer", EventLogEntryType.Information, eventId++);
                GetUrls();
                GetWebservices();
                GetKlanten();

                LogFile logFile = new LogFile();
                foreach (UrlData urlData in _urlDatas)
                {
                    logFile.AddTextToLogFile(urlData.Name + "\n");
                    _result = JObject.Parse(_webRequest.GetWebRequest(urlData.Id, urlHttp, urlData.Name, urlData.SecurityId));
                    foreach (JProperty item in _result)
                    {
                        if (item.Name == "WebServiceDataId")
                        {
                            foreach (WebServiceData webServiceData in _webServiceDatas)
                            {
                                if ((int)item.Value == webServiceData.Id)
                                {
                                    logFile.AddTextToLogFile("Webservice --> " + webServiceData.Name);
                                }
                            }
                        }
                        else if (item.Name == "KlantDataId")
                        {
                            foreach (KlantData klantData in _klantenDatas)
                            {
                                if ((int)item.Value == klantData.Id)
                                {
                                    logFile.AddTextToLogFile("Klant --> " + klantData.Name);
                                }
                            }
                        }
                        logFile.AddTextToLogFile("\n");
                        if (item.Name != "id")
                        {
                            logFile.AddTextToLogFile(item.Name + " = " + item.Value.ToString() + "\n");
                        }
                    }
                }
            }
            finally
            {
                _timer.Start();
            }
            
        }

        private void GetUrls()
        {
            eventLog1.WriteEntry("GetUrls", EventLogEntryType.Information, eventId++);
            _urlDatas = _urltest.GetUrls(eventLog1);
            eventLog1.WriteEntry(_urlDatas + "", EventLogEntryType.Information, eventId++);
        }

        private void GetWebservices()
        {
            _webServiceDatas = _webserviceTest.GetWebServiceDatas(true);
            eventLog1.WriteEntry(_webServiceDatas.ToString(), EventLogEntryType.Information, eventId++);
        }

        private void GetKlanten()
        {
            _klantenDatas = _klantTest.GetKlantData();
            eventLog1.WriteEntry(_klantenDatas.ToString(), EventLogEntryType.Information, eventId++);
        }
    }
}
