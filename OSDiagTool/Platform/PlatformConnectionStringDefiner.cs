using OSDiagTool.Platform.ConfigFiles;
using OSDiagTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Platform {
    class PlatformConnectionStringDefiner {

        public DBConnector.SQLConnStringModel SQLConnString { get; set; }
        public DBConnector.OracleConnStringModel OracleConnString { get; set; }


        public PlatformConnectionStringDefiner GetConnectionString(string dbEngine, bool isLogDatabase, bool isSaCredentials, PlatformConnectionStringDefiner ConnStringDefiner, string saUser = null, string saPwd = null) {

            if (dbEngine.Equals("sqlserver")) {

                ConnStringDefiner.SQLConnString = SetPlatformSQLConnString(isLogDatabase, isSaCredentials, saUser, saPwd);
                return ConnStringDefiner;

            } else if (dbEngine.Equals("oracle")) {

                ConnStringDefiner.OracleConnString = SetPlatformOracleConnString(isLogDatabase, isSaCredentials, saUser, saPwd);
                return ConnStringDefiner;

            }

            return null;

        }

        public DBConnector.SQLConnStringModel SetPlatformSQLConnString(bool isLogDatabase, bool isSaCredentials, string saUser = null, string saPwd = null) {

            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            var sqlConnString = new DBConnector.SQLConnStringModel();

            if (!isLogDatabase && !isSaCredentials) { // Uses Runtime user and Platform Main Catalog 

                string platformDBRuntimeUser = platformDBInfo.GetProperty("RuntimeUser").Value;
                string platformDBRuntimeUserPwd = platformDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath));

                sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;
                sqlConnString.userId = platformDBRuntimeUser;
                sqlConnString.pwd = platformDBRuntimeUserPwd;

            }
            else if (isLogDatabase) { // Uses Runtime Log user and Log Catalog

                ConfigFileDBInfo loggingDBInfo = confFileParser.DBLoggingInfo;

                sqlConnString.dataSource = loggingDBInfo.GetProperty("Server").Value;
                sqlConnString.userId = loggingDBInfo.GetProperty("RuntimeUser").Value; // needs to use oslog configurations
                sqlConnString.pwd = loggingDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath));
                sqlConnString.initialCatalog = loggingDBInfo.GetProperty("Catalog").Value;

            }
            else if (isSaCredentials) { // Uses SA Credentials inputted on the Form

                sqlConnString.dataSource = platformDBInfo.GetProperty("Server").Value;
                sqlConnString.initialCatalog = platformDBInfo.GetProperty("Catalog").Value;
                sqlConnString.userId = saUser;
                sqlConnString.pwd = saPwd;

            }

            return sqlConnString;

        }

        public DBConnector.OracleConnStringModel SetPlatformOracleConnString(bool isLogDatabase, bool isSaCredentials, string saUser = null, string saPwd = null) {

            ConfigFileReader confFileParser = new ConfigFileReader(Program.platformConfigurationFilepath, Program.osPlatformVersion);
            ConfigFileDBInfo platformDBInfo = confFileParser.DBPlatformInfo;

            var orclConnString = new DBConnector.OracleConnStringModel();

            if (!isLogDatabase && !isSaCredentials) { // Uses Runtime user and Platform Main Catalog 

                string platformDBRuntimeUser = platformDBInfo.GetProperty("RuntimeUser").Value;
                string platformDBRuntimeUserPwd = platformDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath));
                string platformDBAdminUser = platformDBInfo.GetProperty("AdminUser").Value;

                orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                orclConnString.userId = platformDBRuntimeUser;
                orclConnString.pwd = platformDBRuntimeUserPwd;

            } else if (isLogDatabase) { // Uses Runtime Log user and Log Catalog

                ConfigFileDBInfo loggingDBInfo = confFileParser.DBLoggingInfo;

                orclConnString.host = loggingDBInfo.GetProperty("Host").Value;
                orclConnString.port = loggingDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = loggingDBInfo.GetProperty("ServiceName").Value;
                orclConnString.userId = loggingDBInfo.GetProperty("RuntimeUser").Value; // needs to use oslog configurations
                orclConnString.pwd = loggingDBInfo.GetProperty("RuntimePassword").GetDecryptedValue(CryptoUtils.GetPrivateKeyFromFile(Program.privateKeyFilepath));


            } else if (isSaCredentials) { // Uses SA Credentials inputted on the Form

                orclConnString.host = platformDBInfo.GetProperty("Host").Value;
                orclConnString.port = platformDBInfo.GetProperty("Port").Value;
                orclConnString.serviceName = platformDBInfo.GetProperty("ServiceName").Value;
                orclConnString.userId = saUser;
                orclConnString.pwd = saPwd;

            }

            return orclConnString;
        }

        



    }
}
