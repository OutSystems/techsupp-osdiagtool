using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;

namespace OSDiagTool.Platform.ConfigFiles
{
    class ConfigFileReader
    {
        public static string PlatformDatabaseConfigurationElement = "PlatformDatabaseConfiguration";
        public static string LoggingDatabaseConfigurationElement = "LoggingDatabaseConfiguration";
        public static string SessionDatabaseConfigurationElement = "SessionDatabaseConfiguration";
        public static string IsEncryptedAttributeName = "encrypted";
        public static string ProviderKeyAttributeName = "ProviderKey";
        

        private ConfigFileDBInfo _dbPlatformDetails;
        private ConfigFileDBInfo _dbLoggingDetails;
        private ConfigFileDBInfo _dbSessionDetails;

        private string _configFilePath;


        public ConfigFileReader(string filepath)
        {
            _configFilePath = filepath;
            ReadFile();
        }

        private void ReadFile()
        {
            using (FileStream fs = File.OpenRead(_configFilePath))
            {
                XElement root = XElement.Load(fs);
                _dbPlatformDetails = ReadDbPlatformInfo(root);
                _dbLoggingDetails = ReadDbLoggingInfo(root);
                _dbSessionDetails = ReadDbSessionInfo(root);
            }
        }

        private string ReadDBMSType(XElement root, string dbType)
        {
            return root.Element(dbType).Attribute("ProviderKey").Value;
        }

        private ConfigFileDBInfo ReadDbPlatformInfo(XElement root)
        {
            return ReadDBSection(PlatformDatabaseConfigurationElement, root);
        }

        private ConfigFileDBInfo ReadDbLoggingInfo(XElement root)
        {
            return ReadDBSection(LoggingDatabaseConfigurationElement, root);
        }

        private ConfigFileDBInfo ReadDbSessionInfo(XElement root)
        {
            return ReadDBSection(SessionDatabaseConfigurationElement, root);
        }

        private ConfigFileDBInfo ReadDBSection(string sectionName, XElement root)
        {
            ConfigFileDBInfo dbInfo = new ConfigFileDBInfo(sectionName, ReadDBMSType(root, sectionName));
            
            foreach (XElement el in root.Element(sectionName).Elements())
            {
                string elName = el.Name.LocalName;
                dbInfo.AddProperty(ReadDbProperty(root, sectionName, elName));
            }

            return dbInfo;
        }

        private ConfigFileProperty ReadDbProperty(XElement root, string dbType, string parameter)
        {
            string value = root.Element(dbType).Element(parameter).Value;
            bool isEncrypted = root.Element(dbType).Element(parameter).Attribute(IsEncryptedAttributeName).Value.Equals("true");

            return new ConfigFileProperty(parameter, value, isEncrypted);
        }

        public ConfigFileDBInfo DBPlatformInfo
        {
            get { return _dbPlatformDetails; }
        }

        public ConfigFileDBInfo DBLoggingInfo
        {
            get { return _dbLoggingDetails; }
        }

        public ConfigFileDBInfo DBSessionInfo
        {
            get { return _dbLoggingDetails; }
        }
    }
}
