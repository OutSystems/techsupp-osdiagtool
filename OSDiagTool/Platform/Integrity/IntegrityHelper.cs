using OSDiagTool.Platform.ConfigFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace OSDiagTool.Platform.Integrity
{
    class IntegrityHelper
    {
        public static Dictionary<string, Dictionary<string, DateTime>> GetLastModulesRunningPublished(string platformRunningPath)
        {
            Dictionary<string, Dictionary<string, DateTime>> allRunningFolders = new Dictionary<string, Dictionary<string, DateTime>>(); // <path folder>, [<module name>, <creation date>] - check for duplicates and use latest created only

            // Add all modules running paths and respective creation date time to a dict
            foreach (string moduleRunning in Directory.GetDirectories(platformRunningPath))
            {
                allRunningFolders.Add(moduleRunning, new Dictionary<string, DateTime> { { moduleRunning.Substring(moduleRunning.LastIndexOf('\\') + 1, moduleRunning.LastIndexOf('.') - (moduleRunning.LastIndexOf('\\') + 1)), Directory.GetCreationTime(moduleRunning) } });
            }

            // Check for duplicates and output a filtered dict
            List<string> duplicateKeystoRemove = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string, DateTime>> element in allRunningFolders)
            {
                string elementNameTrimmed = element.Key.Substring(element.Key.LastIndexOf('\\') + 1, element.Key.LastIndexOf('.') - (element.Key.LastIndexOf('\\') + 1));
                foreach (KeyValuePair<string, Dictionary<string, DateTime>> elementToCheck in allRunningFolders)
                {
                    if (!String.Equals(element.Key.ToString(), elementToCheck.Key.ToString())) // skip check for same key - if the paths are exactly the same, it's the same module folder
                    {
                        string elementToCheckNameTrimmed = elementToCheck.Key.Substring(elementToCheck.Key.LastIndexOf('\\') + 1, elementToCheck.Key.LastIndexOf('.') - (elementToCheck.Key.LastIndexOf('\\') + 1));
                        if (String.Equals(elementNameTrimmed, elementToCheckNameTrimmed)) // check if the trimmed name is the same - if true, it's a duplicate
                        {

                            allRunningFolders.TryGetValue(element.Key, out var elementInnerDictionary);
                            allRunningFolders.TryGetValue(elementToCheck.Key, out var elementToCheckInnerDictionary);

                            elementInnerDictionary.TryGetValue(elementNameTrimmed, out DateTime elementDateTime);
                            elementToCheckInnerDictionary.TryGetValue(elementNameTrimmed, out DateTime elementToCheckDateTime);

                            if (elementDateTime > elementToCheckDateTime)
                            {
                                if (!duplicateKeystoRemove.Contains(elementToCheck.Key))
                                {
                                    //FileLogger.TraceLog("Found duplicate module in the running folder. Removing connection string check for the oldest module and checking only " + elementNameTrimmed + " with creation date time : " + elementDateTime);
                                    duplicateKeystoRemove.Add(elementToCheck.Key);
                                }
                            }
                        }
                    }
                }
            }

            //Remove duplicates from the dictionary
            if (!duplicateKeystoRemove.Count.Equals(0))
            {
                foreach (string key in duplicateKeystoRemove)
                {
                    allRunningFolders.Remove(key);
                }
            }

            return allRunningFolders;
        }

        public static Dictionary<string, Dictionary<string,string>> GetModuleConnectionStrings(Dictionary<string, Dictionary<string, DateTime>> allRunningFolders, string appSettingsConfig, string[] connectionStringList, string pKey) //<module name>, [< connection string name >, < connection string value >]
        {
            Dictionary<string, Dictionary<string, string>> modulesConnectionStrings = new Dictionary<string, Dictionary<string, string>>();

            foreach (KeyValuePair<string, Dictionary<string, DateTime>> moduleRunningPath in allRunningFolders)
            {
                string moduleName = moduleRunningPath.Key.Substring(moduleRunningPath.Key.LastIndexOf('\\') + 1, moduleRunningPath.Key.LastIndexOf('.') - (moduleRunningPath.Key.LastIndexOf('\\') + 1));

                Dictionary<string, string> connectionStrings = Platform.ConfigFiles.XmlReader.ReadAppSettingsConnectiongStrings(Path.Combine(moduleRunningPath.Key, appSettingsConfig), connectionStringList); //<connection name>, <connection value encrypted>

                foreach (KeyValuePair<string, string> connection in connectionStrings)
                {
                    string decryptedConnectionString = Utils.CryptoUtils.Decrypt(pKey, connection.Value);

                    if (modulesConnectionStrings.ContainsKey(moduleName))
                    {
                        modulesConnectionStrings[moduleName].Add(connection.Key.ToString(), decryptedConnectionString);
                    }
                    else
                    {
                        Dictionary<string, string> innerDict = new Dictionary<string, string>
                    {
                        { connection.Key.ToString(), decryptedConnectionString }
                    };
                        modulesConnectionStrings.Add(moduleName, innerDict);
                    }
                }
            }
            return modulesConnectionStrings;
        }

        public static Dictionary<string, string> GetPlatformConnectionStrings(string pKey)
        {
            Dictionary<string, string> platformConnectionProperties = new Dictionary<string, string>();
            Dictionary<string, string> runtimeMappingHsConfToConnection = new Dictionary<string, string>();
            Dictionary<string, string> sessionMappingHsConfToConnection = new Dictionary<string, string>();

            ConfigFileReader confFileParser = new ConfigFileReader(Program._platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileInfo platformDBInfo = confFileParser.DBPlatformInfo;
            ConfigFileInfo loggingDBInfo = confFileParser.DBLoggingInfo;
            ConfigFileInfo sessionDBInfo = confFileParser.DBSessionInfo;

            Dictionary<string, string> runtimeConnectionProperties = new Dictionary<string, string>();
            Dictionary<string, string> loggingConnectionProperties = new Dictionary<string, string>();
            Dictionary<string, string> sessionConnectionProperties = new Dictionary<string, string>();

            if (Program.dbEngine.Equals(Database.DatabaseType.SqlServer))
            {
                runtimeMappingHsConfToConnection = new Dictionary<string, string> 
                {
                    { "Server", "data source" },
                    { "Catalog", "initial catalog" },
                    { "RuntimeUser", "user id" },
                    { "RuntimePassword", "password" },
                    { "RuntimeAdvancedSettings", "runtime advanced settings" }
                };

                sessionMappingHsConfToConnection = new Dictionary<string, string>
                {
                    { "Server", "data source" },
                    { "Catalog", "initial catalog" },
                    { "SessionUser", "user id" },
                    { "SessionPassword", "password" },
                    { "SessionAdvancedSettings", "session advanced settings" }
                };
                
            }
            else if (Program.dbEngine.Equals(Database.DatabaseType.Oracle))
            {
                runtimeMappingHsConfToConnection = new Dictionary<string, string>
                {
                    { "Host", "data source" },
                    { "Port", "port" },
                    { "ServiceName", "service name" },
                    { "RuntimeUser", "user id" },
                    { "RuntimePassword", "password" },
                    { "RuntimeAdvancedSettings", "runtime advanced settings" }
                };

                sessionMappingHsConfToConnection = new Dictionary<string, string>
                {
                    { "Host", "data source" },
                    { "Port", "port" },
                    { "ServiceName", "service name" },
                    { "SessionUser", "user id" },
                    { "SessionPassword", "password" },
                    { "SessionAdvancedSettings", "session advanced settings" }
                };
            }

            foreach (KeyValuePair<string,string> property in runtimeMappingHsConfToConnection)
            {
                if (!property.Value.Equals("password"))
                {
                    runtimeConnectionProperties.Add(property.Value, platformDBInfo.GetProperty(property.Key).Value);
                    loggingConnectionProperties.Add(property.Value, loggingDBInfo.GetProperty(property.Key).Value);
                }
                else
                {
                    runtimeConnectionProperties.Add(property.Value, Utils.CryptoUtils.Decrypt(pKey, platformDBInfo.GetProperty(property.Key).Value));
                    loggingConnectionProperties.Add(property.Value, Utils.CryptoUtils.Decrypt(pKey, loggingDBInfo.GetProperty(property.Key).Value));
                }
                
            }

            foreach (KeyValuePair<string, string> property in sessionMappingHsConfToConnection)
            {
                if (!property.Value.Equals("password"))
                {
                    sessionConnectionProperties.Add(property.Value, sessionDBInfo.GetProperty(property.Key).Value);
                } else
                {
                    sessionConnectionProperties.Add(property.Value, Utils.CryptoUtils.Decrypt(pKey, sessionDBInfo.GetProperty(property.Key).Value));
                }
            }

            
            platformConnectionProperties.Add("RuntimeConnection", ConnectionBuilder(runtimeConnectionProperties));
            platformConnectionProperties.Add("LoggingConnection", ConnectionBuilder(loggingConnectionProperties));
            platformConnectionProperties.Add("SessionConnection", ConnectionBuilder(sessionConnectionProperties));
            return platformConnectionProperties;

        }

        public static Dictionary<string,string> ConnectionStringParser(string connectionString)
        {
            Dictionary<string, string> connectionDict = new Dictionary<string, string>();
            string pattern = @"[^;]+";

            MatchCollection properties = Regex.Matches(connectionString, pattern);

            foreach (Match match in properties)
            {
                string[] separatedString = match.Value.Trim().Split('=');
                connectionDict.Add(separatedString[0], separatedString[1]);
            }

            return connectionDict;
        }

        private static string ConnectionBuilder (Dictionary<string,string> connectionProperties)
        {
            string connectionString = null;

            foreach (KeyValuePair<string, string> property in connectionProperties)
            {
                if (!property.Key.Contains("advanced settings"))
                {
                    connectionString = connectionString + property.Key + "=" + property.Value + ";";
                }
                else
                {
                    connectionString = connectionString + property.Value;
                }
            }

            return connectionString;
        }
    }
}
