//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.CadPlus.Xport.EDrawingsHost;
using Xarial.CadPlus.Xport.SwEDrawingsHost;

namespace Xarial.CadPlus.Xport.StandAloneExporter
{
    public class Program
    {
        public const string LOG_MESSAGE_TAG = "XARIAL:::XPORT:::";

        [STAThread]
        private static void Main(string[] args)
        {
            async void OnIdle(object sender, EventArgs e)
            {
                Application.Idle -= OnIdle;

                try
                {
                    await ProcessAsync(args[0], args[1], args[2]);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    //TODO: extract message exception only
                    WriteLine(ex.Message);
                    Environment.Exit(1);
                }
            };

            Application.Idle += OnIdle;
            Application.Run();
        }
        
        private static async Task ProcessAsync(string srcFile, string outFile, string versNmb) 
        {
            var vers = EDrawingsVersion_e.Default;

            foreach (EDrawingsVersion_e curVer in Enum.GetValues(typeof(EDrawingsVersion_e)))
            {
                if (string.Equals(curVer.ToString(), $"v{versNmb}"))
                {
                    vers = curVer;
                    break;
                }
            }

            WriteLine($"eDrawings version: {vers}");

            using (var publisher = new EDrawingsPublisher(vers))
            {
                WriteLine($"Opening '{srcFile}'...");
                await publisher.OpenDocument(srcFile);

                WriteLine($"Saving '{srcFile}' to '{outFile}'...");
                await publisher.SaveDocument(outFile);

                WriteLine($"Closing '{srcFile}'...");
                await publisher.CloseDocument();
            }
        }

        private static void WriteLine(string line)
        {
            Console.WriteLine(LOG_MESSAGE_TAG + line);
        }
    }
}