using System;
using System.Configuration;
using System.Xml;

namespace WindowsServiceAppTest
{
    class KrXml
    {
        public KrXml()
        {

        }

        public KrXmlData GetDataOfXmlFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"D:\user.xml");
            XmlNodeList aNodes = doc.SelectNodes("//*");

            KrXmlData xmlData = new KrXmlData();
            foreach (XmlNode aNode in aNodes)
            {
                switch (aNode.Name)
                {
                    case "ServiceAanOfUit":
                        xmlData.ServiceAanOfUit = aNode.FirstChild.Value;
                        break;
                    case "TijdService":
                        xmlData.TijdService = Convert.ToDouble(aNode.FirstChild.Value);
                        break;
                    case "SaveLogFilePlace":
                        xmlData.SaveLogFilePlace = aNode.FirstChild.Value;
                        break;
                    case "Email":
                        xmlData.Email = aNode.FirstChild.Value;
                        break;
                    case "PlaceDb":
                        xmlData.PlaceDb = aNode.FirstChild.Value;
                        break;
                    case "MailServerNaam":
                        xmlData.MailServerNaam = aNode.FirstChild.Value;
                        break;
                    case "MailServerPoort":
                        xmlData.MailServerPoort = Int32.Parse(aNode.FirstChild.Value);
                        break;
                    case "MailVerzendenVanuitEmail":
                        xmlData.MailVerzendenVanuitEmail = aNode.FirstChild.Value;
                        break;
                    case "MailServerGebruikersnaam":
                        if (aNode.FirstChild.Value != string.Empty)
                        {
                            xmlData.MailServerGebruikersnaam = aNode.FirstChild.Value;
                        }            
                        break;
                    case "MailServerWachtwoord":
                        if (aNode.FirstChild.Value != string.Empty)
                        {
                            xmlData.MailServerWachtwoord = aNode.FirstChild.Value;
                        }
                        break;
                    default:
                        break;
                }
            }
            return xmlData;
        }
    }
}
