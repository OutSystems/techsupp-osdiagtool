
namespace OSDiagTool.DBConnector
{
    public class SQLConnStringModel
    {
        public string dataSource { get; set; }

        public string initialCatalog { get; set; }

        public string userId { get; set; }

        public string pwd { get; set; }

        public string advancedSettings { get; set; }

    }

    public class OracleConnStringModel
    {
        public string host { get; set; }

        public string port { get; set; }

        public string serviceName { get; set; }

        public string userId { get; set; }

        public string pwd { get; set; }

        public string advancedSettings { get; set; }
    }
}
