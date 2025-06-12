using OSDiagTool.Utils;

namespace OSDiagTool.Platform.ConfigFiles
{
    public class ConfigFileProperty
    {
        private string _name;
        private string _value;
        private bool _isEncrypted;

        public ConfigFileProperty(string name, string value, bool isEncrypted)
        {
            _name = name;
            _value = value;
            _isEncrypted = isEncrypted;
        }

        public string Name
        {
            get {return _name;}
        }

        public string Value
        {
            get { return _value; }
        }

        public bool IsEncrypted
        {
            get { return _isEncrypted; }
        }

        public string GetDecryptedValue(string privateKey)
        {
            return IsEncrypted ? CryptoUtils.Decrypt(privateKey, Value) : Value;
        }
    }
}
