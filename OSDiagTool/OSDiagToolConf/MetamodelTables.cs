using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSDiagTool.OSDiagToolConf {
    class MetamodelTables {

        // Allowed prefixes
        private static string _ossysPrefix = "ossys";
        private static string _osltmPrefix = "osltm";

        List<string> allowedPrefixes = new List<string> {
            _ossysPrefix,
            _osltmPrefix,
        };

        public bool ValidateMetamodelTableName(string tableName) {

            if(!tableName.ToLower().Contains(" ")) {
                bool checkPrefixes = allowedPrefixes.Any(o => tableName.ToLower().StartsWith(o)); // check if tableName matches any of the allowed prefixes in the list
                if (checkPrefixes) {
                    return true;
                }
            }
            return false;
        }

        public string TableNameEscapeCharacters(string tableName) {

            Regex pattern = new Regex("[ -*/]|[\n]{2}/g");
            return pattern.Replace(tableName, "");

        }

    }
}
