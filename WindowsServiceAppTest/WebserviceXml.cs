using System;
using System.Collections.Generic;
using System.Xml.Linq;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    class WebserviceXml
    {
        public List<Webservice> GetAll(string path)
        {
            XDocument doc = XDocument.Load(path);
            List<Webservice> webservices = new List<Webservice>();

            foreach (XElement element in doc.Descendants("Webservice"))
            {
                Webservice newWebservice = new Webservice();
                newWebservice.Id = Guid.Parse(element.Attribute("Id").Value);
                newWebservice.Name = element.Attribute("Name").Value;
                newWebservice.Soap = bool.Parse(element.Attribute("Soap").Value);
                newWebservice.SecurityId = element.Attribute("SecurityId").Value;
                webservices.Add(newWebservice);
            }
            return webservices;
        }
    }
}
