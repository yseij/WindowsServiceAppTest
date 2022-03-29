namespace WindowsFormsAppTest
{
    class WebServiceData
    {
        public WebServiceData(int id, string name, bool soap)

        {
            Id = id;
            Name = name;
            Soap = soap;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public bool Soap { get; set; }
    }
}
