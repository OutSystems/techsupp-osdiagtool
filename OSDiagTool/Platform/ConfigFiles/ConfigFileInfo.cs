using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform.ConfigFiles
{
    public class ConfigFileInfo
    {
        private string _platformDBType;
        private string _dbms;
        private IDictionary<string, ConfigFileProperty> _properties;

        public ConfigFileInfo(string platformDbType, string dbms)
        {
            _platformDBType = platformDbType;
            _dbms = dbms;

            _properties = new Dictionary<string, ConfigFileProperty>();
        }

        public string PlatformDBType
        {
            get { return _platformDBType; }
        }

        public string DBMS
        {
            get { return _dbms; }
        }

        public ConfigFileProperty GetProperty(string propertyName)
        {
            return _properties[propertyName];
        }

        public ICollection<string> GetPropertyNames()
        {
            return _properties.Keys;
        }

        public ICollection<ConfigFileProperty> GetProperties()
        {
            return _properties.Values;
        }

        public void AddProperty(ConfigFileProperty property)
        {
            _properties.Add(property.Name, property);
        }

        public void AddProperties(ICollection<ConfigFileProperty> properties)
        {
            foreach(ConfigFileProperty prop in properties)
            {
                AddProperty(prop);
            }
        }
    }
}
