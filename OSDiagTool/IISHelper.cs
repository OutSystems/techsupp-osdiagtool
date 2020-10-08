using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace OSDiagTool
{
    class IISHelper
    {

        public static void GetIISAccessLogs(string iisapplicationHostPath, string tempFolderPath, FileSystemHelper fsHelper, int daysToFetch) {

            //Retrieving IIS access logs
            try {
                FileLogger.TraceLog("Retrieving IIS Access logs... ");

                // Loading Xml text from the file. Note: 32 bit processes will redirect \System32 to \SysWOW64: http://www.samlogic.net/articles/sysnative-folder-64-bit-windows.htm
                if (Environment.Is64BitOperatingSystem == false) {
                    iisapplicationHostPath = iisapplicationHostPath.Replace("system32", "Sysnative");
                }
                var xmlString = XDocument.Load(iisapplicationHostPath);

                // Querying the data and finding the Access logs path
                var query = from p in xmlString.Descendants("siteDefaults")
                            select new {
                                LogsFilePath = p.Element("logFile").Attribute("directory").Value,
                            };

                string iisAccessLogsPath = query.First().LogsFilePath.ToLower();

                if (iisAccessLogsPath.Contains("%systemdrive%")) {
                    iisAccessLogsPath = iisAccessLogsPath.Replace("%systemdrive%\\", Path.GetPathRoot(Environment.SystemDirectory));
                    if ((Environment.Is64BitOperatingSystem == false) && iisAccessLogsPath.Contains("system32")) {
                        iisAccessLogsPath = iisAccessLogsPath.Replace("system32", "Sysnative");
                    }
                }

                //Copies all the contents from the path iisAcessLogsPath, including contents in subfolder
                fsHelper.DirectoryCopy(iisAccessLogsPath, Path.Combine(tempFolderPath, "IISLogs"), true, true, daysToFetch);

            } catch (Exception e) {
                FileLogger.LogError("Attempted to retrieve IIS Access logs but failed...", e.Message + e.StackTrace);
            }

        }
    }
}
