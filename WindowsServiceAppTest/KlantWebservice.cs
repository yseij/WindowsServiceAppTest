using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceAppTest
{
    class KlantWebservice
    {
        public KlantWebservice()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid Klant { get; set; }

        public Guid Webservice { get; set; }

        public bool BasisUrl1 { get; set; }

        public bool BasisUrl2 { get; set; }
    }
}
