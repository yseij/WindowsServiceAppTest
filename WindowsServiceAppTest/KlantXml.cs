using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    class KlantXml
    {
        private string _path = ConfigurationManager.AppSettings["PlaceDb"];
        public List<Klant> GetAll()
        {
            XDocument doc = XDocument.Load(_path);
            List<Klant> klanten = new List<Klant>();

            foreach (XElement element in doc.Descendants("Klant"))
            {
                Klant newKlant = new Klant();
                newKlant.Id = Guid.Parse(element.Attribute("Id").Value);
                newKlant.Name = element.Attribute("Name").Value;
                newKlant.BasisUrl1 = element.Attribute("BasisUrl1").Value;
                newKlant.BasisUrl2 = element.Attribute("BasisUrl2").Value;
                klanten.Add(newKlant);
            }
            return klanten;
        }
    }
}
