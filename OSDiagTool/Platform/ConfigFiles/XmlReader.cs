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
        public static string ReadAppSettingsConnectiongString(string filePath, string connectiongStringName)
        {
            string connection = null;
            try
            {
                XDocument xmlDoc = XDocument.Parse(filePath);
                XElement element = xmlDoc.Descendants("add").FirstOrDefault(e => e.Attribute("key")?.Value == connectiongStringName);

                if (element != null)
                {
                    return connection = element.Attribute("value").Value;
                }

            } catch (Exception e)
            {
                throw e;
            }

            return connection;

        }
    }
}
