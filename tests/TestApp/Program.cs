using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;

namespace TestApp
{
    public class MyAppArgs 
    {
        public string[] Arguments { get; set; }
    }

    public class MyApp : IApplication
    {
        public Guid Id => Guid.Parse("A0A1C488-96E1-4A06-BF7E-2F3EB034F676");
    }

    public class MyInitiator : IInitiator
    {
        private IHost m_Host;
        private readonly AppLogger m_Logger;

        public MyInitiator() 
        {
            m_Logger = new AppLogger();
        }

        public void Init(IHost host)
        {
            m_Host = host;

            m_Host.Connect += OnHostConnect;
            m_Host.ConfigureServices += OnHostConfigureServices;
            m_Host.Initialized += OnHostInitialized;
            m_Host.Started += OnHostStartedStarted;
        }

        private void OnHostStartedStarted()
            => m_Logger.Log("7 - Host started");

        private void OnHostInitialized()
            => m_Logger.Log("4 - Host initiated");

        private void OnHostConnect()
            => m_Logger.Log("5 - Host connect");

        private void OnHostConfigureServices(IContainerBuilder obj)
            => m_Logger.Log("3 - Host configure services");
    }

    class Program
    {
        private static AppLogger m_Logger;
        
        [STAThread]
        static void Main(string[] args)
        {
            m_Logger = new AppLogger();
            var appLauncher = new ApplicationLauncher<MyAppArgs, MyWindow>(new MyApp(), new MyInitiator());
            appLauncher.ConfigureServices += OnConfigureServices;
            appLauncher.ParseArguments += OnParseArguments;
            appLauncher.RunConsole += OnRunConsole;
            appLauncher.WindowCreated += OnWindowCreated;
            appLauncher.Start(args);
        }

        private static void OnWindowCreated(MyWindow window, MyAppArgs args)
            => m_Logger.Log("6 - Window created");

        private static void OnRunConsole(MyAppArgs args)
        {
            m_Logger.Log("8 - Run Console");

            if (args.Arguments.Any(a => string.Equals(a, "err", StringComparison.CurrentCultureIgnoreCase))) 
            {
                throw new Exception("Some error");
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Console output {i + 1}");
                Thread.Sleep(500);
            }
        }

        private static bool OnParseArguments(string[] input, ref MyAppArgs args, ref bool createConsole)
        {
            args = new MyAppArgs()
            {
                Arguments = input
            };

            m_Logger.Log("1 - Parse Arguments");
            return true;
        }

        private static void OnConfigureServices(ContainerBuilder builder, MyAppArgs args)
            => m_Logger.Log("2 - Configure Services");
    }
}
