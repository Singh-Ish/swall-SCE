using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NewMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int monitorCount;
        public static double monitorHeight;
        public static double monitorWidth;
        public static string slave1IP;
        public static string slave2IP;
        public static string slave3IP;
        public static string slave4IP;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeApp();
        }
        private void InitializeApp()
        {
            //Get the number of monitors hooked up in the system according to the App.config
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("MonitorCount"), out monitorCount)
                || monitorCount < 0)
            {
                throw new ConfigurationErrorsException("Invalid monitor count value set in App config");
            }
            slave1IP = ConfigurationManager.AppSettings.Get("Slave1IP");
            slave2IP = ConfigurationManager.AppSettings.Get("Slave2IP");
            slave3IP = ConfigurationManager.AppSettings.Get("Slave3IP");
            slave4IP = ConfigurationManager.AppSettings.Get("Slave4IP");

            //if (!long.TryParse(ConfigurationManager.AppSettings.Get("MasterIP"), out masterIP))
            //{
            //    throw new ConfigurationErrorsException("Invalid master IP value set in App config");
            //}
            //Set the size of width and height for a single app space.
            //This is based on the size of the primary monitor size.
            //Therefore this assumes all monitors are the same size
            monitorWidth = SystemParameters.PrimaryScreenWidth;
            monitorHeight = SystemParameters.PrimaryScreenHeight;
        }
    }
}
