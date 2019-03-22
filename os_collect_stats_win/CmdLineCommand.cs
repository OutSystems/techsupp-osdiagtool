using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace os_collect_stats_win
{
    public class CmdLineCommand
    {
        private string _cmd;
        private string _outputFile;

        public CmdLineCommand(string cmd, string outputFile="")
        {
            _cmd = cmd;
            _outputFile = outputFile;
        }

        public void Execute()
        {
            CmdHelper.RunCommand(FullCmd);
        }

        public string FullCmd
        {
            get { return _cmd + (_outputFile != "" ? string.Format(" > \"{0}\"", _outputFile) : ""); }
        }

        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }
    }
}
