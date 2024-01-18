using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OSDiagTool.Platform.ConfigFiles
{
    public class XmlReader
    {
        public static Dictionary<string, string> ReadAppSettingsConnectiongStrings(string filePath, string[] connectiongStringNames) // <connection name>, <connection value>
        {
            Dictionary<string, string> ConnectionStrings = new Dictionary<string, string>();

            try
            {
                XElement xmlElement = XElement.Load(filePath);

                foreach (string connection in connectiongStringNames)
                {
                    string value = (from element in xmlElement.Elements("add")
                                    where (string)element.Attribute("key") == connection
                                    select (string)element.Attribute("value")).FirstOrDefault();

                    ConnectionStrings[connection] = value ?? "Connection not found";
                }            

            } catch (Exception e)
            {
                throw e;
            }

            return ConnectionStrings;

        }
    }
}
