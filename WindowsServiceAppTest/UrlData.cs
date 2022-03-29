﻿namespace WindowsFormsAppTest
{
    public class UrlData
    {
        public UrlData()
        {

        }

        public UrlData(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public UrlData(int id, string name, string securityId, int webServiceDataId, int klantDataId)
            : this(id, name)
        {
            SecurityId = securityId;
            WebServiceDataId = webServiceDataId;
            KlantDataId = klantDataId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string SecurityId { get; set; }
        public int WebServiceDataId { get; set; }
        public int KlantDataId { get; set; }
    }
}