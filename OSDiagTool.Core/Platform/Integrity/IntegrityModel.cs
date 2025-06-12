using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform.Integrity
{
    public class IntegrityModel
    {
        public Dictionary<string, IntegrityDetails> CheckDetails { get; set; }

        // Default constructor
        public IntegrityModel()
        {
            CheckDetails = new Dictionary<string, IntegrityDetails>();
        }

        //Copy constructor
        public IntegrityModel(IntegrityModel other)
        {
            CheckDetails = new Dictionary<string, IntegrityDetails>(other.CheckDetails);
        }
    }

    public class IntegrityDetails
    {
        public string SqlText { get; set; }
        public string ErrorMessage { get; set; }
        public bool returnsRecords { get; set; }
        public bool? checkOk { get; set; }
        
    }
}
