using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    case "ServerNaam":
                        xmlData.ServerNaam = aNode.FirstChild.Value;
                        break;
                    default:
                        break;
                }
            }
            return xmlData;
        }
    }
}
