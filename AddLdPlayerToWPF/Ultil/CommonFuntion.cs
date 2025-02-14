using Auto_LDPlayer;
using Emgu.CV.Flann;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AddLdPlayerToWPF.Ultil
{
    public static class CommonFuntion
    {
        public static List<string> GetName(int thread)
        {
            List<string> list = new List<string>();
            for (int i = 1; i < thread + 1; i++)
            {
                list.Add("LDPlayer-" + i.ToString());
            }
            return list;
        }

        public static bool CheckLDRunning(int index, string pathLD)
        {
            string name = $"index {index}";
            string arguments = string.Format("adb --{0} --command \"{1}\"", name, "shell input tap 1 1");
            int num = 0;
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = pathLD,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            for (; ; )
            {
                process.Start();
                process.WaitForExit(3000);
                string text = process.StandardOutput.ReadToEnd();
                string a = text;
                bool flag = a == "";
                if (flag)
                {
                    break;
                }
                Thread.Sleep(2000);
                num++;
                bool flag2 = num == 30;
                if (flag2)
                {
                    goto Block_2;
                }
            }
            return true;
        Block_2:
            return false;
        }
    }
}
