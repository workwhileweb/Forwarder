using System;
using System.Diagnostics;
using Public;

namespace Forwarder
{
    public class Config
    {
        public string Exe { get; set; }
        public bool Wait { get; set; } = false;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Init();
            var config = Public.Config.LoadConfig<Config>()??new Config();

            //if (!File.Exists(config.Exe)) 
            //    throw new FileNotFoundException(config.Exe);

            //Environment.CommandLine : exe sẽ có bọc quote
            //Constant.ExeFile : không bọc quote
            //Environment.CommandLine.StartsWith(Constant.ExeFile) : remove quote thì ok

            var startInfo = new ProcessStartInfo();
            // all environment variables of the created process
            // are inherited from the current process
            startInfo.EnvironmentVariables["CALLER"] = Constant.ExeFile;
            // required for EnvironmentVariables to be set
            startInfo.UseShellExecute = false;
            startInfo.FileName = config.Exe;
            startInfo.Arguments = string.Join(" ", args);
            var proc= Process.Start(startInfo);

            if (proc == null) throw new Exception(nameof(Process));

            if (config.Wait) proc.WaitForExit();
        }
    }
}