using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Public;

namespace Forwarder
{
    public class Config
    {
        public string Exe { get; set; }
        public bool Wait { get; set; } = false;
        public string Params { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Init();

            var config = Public.Config.LoadConfig<Config>()??new Config();
            if (config is null) throw new Exception(nameof(config));

            var fileCfgSample = Public.Constant.ExeFile + ".cfg.sample";
            if (!File.Exists(fileCfgSample)) File.WriteAllText(fileCfgSample, JsonSerializer.Serialize(config));

            if (!File.Exists(config.Exe)) 
                throw new FileNotFoundException(config.Exe);

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
            startInfo.Arguments = config.Params + " " + string.Join(" ", args);

            var proc= Process.Start(startInfo);
            if (proc == null) throw new Exception(nameof(Process));

            if (config.Wait) proc.WaitForExit();
        }
    }
}