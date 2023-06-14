using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Python
{
    public static class PythonHelper
    {
        public static string ExecutePython(string script, List<double> args, string modelpath, string environment)
        {
            string totalpath, exe;
            switch (environment)
            {
                case "Production":
                    script = script.Replace('\\', '/');
                    modelpath = modelpath.Replace('\\', '/');
                    totalpath = "Modeling" + @$"/{script} " + string.Join(" ", args) + " " + "Modeling" + @$"/{modelpath}";
                    exe = "python3";
                    break;
                case "Development":
                    totalpath = @"..\..\..\..\Modeling" + @$"\{script} " + string.Join(" ", args) + " " + @"..\..\..\..\Modeling" + @$"\{modelpath}";
                    exe = "python";
                    break;
                default:
                    totalpath = @"..\..\..\..\Modeling" + @$"\{script} " + string.Join(" ", args) + " " + @"..\..\..\..\Modeling" + @$"\{modelpath}";
                    exe = "python";
                    break;
            }

            var psi = new ProcessStartInfo(exe, totalpath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            var results = StartProcess(psi);

            return results;
        }

        public static string ExecutePythonV2(List<double> args, string environment)
        {
            string totalpath, exe, script = "playerpredictions.py";

            var start = new ProcessStartInfo();
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            
            switch (environment)
            {
                case "Production":
                    start.FileName = "python3";
                    start.Arguments = String.Join(" ", $"Modeling/{script}", environment, String.Join(" ", args));
                    break;
                case "Development":
                    start.FileName = "python";
                    start.Arguments = String.Join(" ", $"../../../../Modeling/{script}", environment, String.Join(" ", args));
                    break;
                default:
                    break;
            }

            var results = StartProcess(start);

            return results;
        }

        private static string StartProcess(ProcessStartInfo psi)
        {
            using (var process = Process.Start(psi))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }
    }
}
