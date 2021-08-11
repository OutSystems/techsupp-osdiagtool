using System.Collections.Generic;

namespace OSDiagTool.Platform
{
    public class ConnectionList
    {
        public string Name { get; set; }
        public string Hostname { get; set; }
        public List<int> Ports { get; set; }
    }
}
