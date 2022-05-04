using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WindowsServiceAppTest
{
    class KlantWebserviceXml
    {
        private string _path = @"D://db.xml";

        public List<KlantWebservice> GetAll()
        {
            List<KlantWebservice> klantWebservices = new List<KlantWebservice>();

            XDocument doc = XDocument.Load(_path);
            IEnumerable<XElement> elements = doc.Descendants("KlantWebservice");
            foreach (XElement element in elements)
            {
                KlantWebservice newKlantWebservice = new KlantWebservice();
                newKlantWebservice.Id = Guid.Parse(element.Attribute("Id").Value);
                newKlantWebservice.Klant = Guid.Parse(element.Attribute("Klant").Value);
                newKlantWebservice.Webservice = Guid.Parse(element.Attribute("Webservice").Value);
                newKlantWebservice.BasisUrl1 = bool.Parse(element.Attribute("BasisUrl1").Value);
                newKlantWebservice.BasisUrl2 = bool.Parse(element.Attribute("BasisUrl2").Value);
                klantWebservices.Add(newKlantWebservice);
            }
            return klantWebservices;
        }
    }
}
