using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    class UrlXml
    {
        public List<Url> GetAll(string path)
        {
            List<Url> urls = new List<Url>();

            XDocument doc = XDocument.Load(path);
            IEnumerable<XElement> elements = doc.Descendants("Url");
            foreach (XElement element in elements)
            {
                Url newUrl = new Url();
                newUrl.Id = Guid.Parse(element.Attribute("Id").Value);
                newUrl.Name = element.Attribute("Name").Value;
                newUrl.KlantId = Guid.Parse(element.Attribute("KlantId").Value);
                newUrl.WebserviceId = Guid.Parse(element.Attribute("WebserviceId").Value);
                urls.Add(newUrl);
            }
            return urls;
        }
    }
}
