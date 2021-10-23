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
        public static string FixPath(string path)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return path.Replace("{home}", home).Replace("{exe}", Constant.ExeFile);
        }

        public static void SetupForward(string fromExe, string toExe, string param)
        {
            fromExe = FixPath(fromExe);
            toExe = FixPath(toExe);

            var folder = Path.GetDirectoryName(fromExe);
            if (string.IsNullOrWhiteSpace(folder)) throw new Exception(nameof(folder));

            Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            InOut.DirectoryCopy(Constant.ExeFolder, folder);
            var config = new Config {Exe = toExe, Params = param};

            File.WriteAllText(fromExe + ".cfg", JsonSerializer.Serialize(config));
            File.Copy(Constant.ExeFile,fromExe);
            File.Copy(Constant.ExeFile + ".config", fromExe + ".config");
        }

        private static void Main(string[] args)
        {
            Log.Init();

            if (args.Length == 4 && args[0].Equals("setup"))
            {
                SetupForward(args[1], args[2], args[3]);
                return;
            }

            var config = Public.Config.LoadConfig<Config>()??new Config();
            if (config is null) throw new Exception(nameof(config));

            //var fileCfgSample = Constant.ExeFile + ".cfg.sample";
            //if (!File.Exists(fileCfgSample)) File.WriteAllText(fileCfgSample, JsonSerializer.Serialize(config));

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