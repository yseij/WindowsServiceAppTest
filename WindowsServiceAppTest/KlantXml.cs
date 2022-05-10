using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using WindowsFormsAppTest;

namespace WindowsServiceAppTest
{
    class KlantXml
    {
        public List<Klant> GetAll(string path)
        {
            XDocument doc = XDocument.Load(path);
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
