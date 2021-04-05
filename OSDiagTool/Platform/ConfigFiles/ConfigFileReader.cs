using System.Xml.Linq;
using System.IO;

namespace OSDiagTool.Platform.ConfigFiles
{
    class ConfigFileReader
    {
        public static string PlatformDatabaseConfigurationElement = "PlatformDatabaseConfiguration";
        public static string LoggingDatabaseConfigurationElement = "LoggingDatabaseConfiguration";
        public static string SessionDatabaseConfigurationElement = "SessionDatabaseConfiguration";
        public static string ServiceConfigurationElement = "ServiceConfiguration"; // Read ServiceConfiguration section from server.hsconf
        public static string NetworkConfigurationElement = "NetworkConfiguration"; // Read NetworkConfiguration section from server.hsconf
        public static string IsEncryptedAttributeName = "encrypted";
        public static string ProviderKeyAttributeName = "ProviderKey";
        

        private ConfigFileInfo _dbPlatformDetails;
        private ConfigFileInfo _dbLoggingDetails;
        private ConfigFileInfo _dbSessionDetails;
        private ConfigFileInfo _srvConfigurationDetails;

        private string _configFilePath;

        public ConfigFileReader(string filepath, string osPlatformVersion)
        {
            _configFilePath = filepath;
            ReadFile(osPlatformVersion);
        }

        /*
         * Read the server.hsconf file and retrieve its sections
         */
        private void ReadFile(string osPlatformVersion)
        {
            using (FileStream fs = File.OpenRead(_configFilePath))
            {
                XElement root = XElement.Load(fs);
                _dbPlatformDetails = ReadDbPlatformInfo(root);
                if (!(osPlatformVersion.StartsWith("10."))) {
                    _dbLoggingDetails = ReadDbLoggingInfo(root);
                }
                _dbSessionDetails = ReadDbSessionInfo(root);
                _srvConfigurationDetails = ReadSrvConfigurationInfo(root);
            }
        }

        /*
         * Returns the ProviderKey attribute of an database configuration from server.hsconfig
         */
        private string ReadDBMSType(XElement root, string dbType)
        {
            return root.Element(dbType).Attribute("ProviderKey").Value;
        }

        private ConfigFileInfo ReadDbPlatformInfo(XElement root)
        {
            return ReadSection(PlatformDatabaseConfigurationElement, root);
        }

        private ConfigFileInfo ReadDbLoggingInfo(XElement root)
        {
            return ReadSection(LoggingDatabaseConfigurationElement, root);
        }

        private ConfigFileInfo ReadDbSessionInfo(XElement root)
        {
            return ReadSection(SessionDatabaseConfigurationElement, root);
        }

        private ConfigFileInfo ReadSrvConfigurationInfo(XElement root)
        {
            return ReadSection(ServiceConfigurationElement, root);
        }

        private ConfigFileInfo ReadSection(string sectionName, XElement root)
        {
            // If we are unable to retrieve the ProviderKey value, then the readType should be empty
            string readType;
            try {
                readType = ReadDBMSType(root, sectionName); 
            } catch { 
                readType = ""; 
            }

            ConfigFileInfo dbInfo = new ConfigFileInfo(sectionName, readType);

            foreach (XElement el in root.Element(sectionName).Elements())
            {
                string elName = el.Name.LocalName;
                dbInfo.AddProperty(ReadProperty(root, sectionName, elName));
            }

            return dbInfo;
        }

        // Reading property attributes
        private ConfigFileProperty ReadProperty(XElement root, string dbType, string parameter)
        {
            string value = root.Element(dbType).Element(parameter).Value;

            // If we are unable to retrieve the encrypted attribute's value, then isEncrypted should be set as false
            bool isEncrypted;
            try
            {
                isEncrypted = root.Element(dbType).Element(parameter).Attribute(IsEncryptedAttributeName).Value.Equals("true");
            } catch {
                isEncrypted = false;
            }
            return new ConfigFileProperty(parameter, value, isEncrypted);
        }

        // PlatformDatabaseConfiguration section of the server.hsconfig
        public ConfigFileInfo DBPlatformInfo
        {
            get { return _dbPlatformDetails; }
        }

        // LoggingDatabaseConfiguration section of the server.hsconfig
        public ConfigFileInfo DBLoggingInfo
        {
            get { return _dbLoggingDetails; }
        }

        // SessionDatabaseConfiguration section of the server.hsconfig
        public ConfigFileInfo DBSessionInfo
        {
            get { return _dbSessionDetails; }
        }

        // ServiceConfiguration section of the server.hsconfig
        public ConfigFileInfo SrvConfigurationInfo
        {
            get { return _srvConfigurationDetails; }
        }
    }
}
