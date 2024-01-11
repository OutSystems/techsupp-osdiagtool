using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDiagTool.Database
{
    public enum DatabaseType
    {
        Oracle,
        SqlServer
    }
    class DatabaseTypeClass
    {
        private DatabaseType dbEngine;

        public DatabaseType DbEngine
        {
            get { return dbEngine; }
            set
            {
                dbEngine = value;
            }
        }
    }
}
