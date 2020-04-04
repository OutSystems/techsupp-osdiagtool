using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Database.DatabaseQueries {
    class DatabaseTroubleshoot {

        public static void DatabaseTroubleshooting(string dbEngineType) {

            // work in progress
            if (dbEngineType.ToLower().Equals("sqlserver")) {

                var sqlDBQueries = new SQLServerQueries();

                string statCachedPlan = string.Format(sqlDBQueries.statCachedPlans, "10");






            }
            else if (dbEngineType.ToLower().Equals("oracle")) {

                var oracleDBQueries = new OracleQueries();

                string sessionByReadIO = string.Format(oracleDBQueries.sessionByIOType, "DISK_READS");
                string sessionByWriteIO = string.Format(oracleDBQueries.sessionByIOType, "DIRECT_WRITES");


            }





        }





    }
}
