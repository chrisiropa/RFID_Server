using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.ServiceProcess;


namespace RFID_Server
{
   class Program
   {
      private static bool runAsService = true;
      private static bool installServiceMode = false;

      private static string applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".vshost", "");
      private static string applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      private static string applicationTitle = "IROPA RFID_Server";
      private static string applicationDescription = "IROPA RFID_Server";
      
      public static string ApplicationTitle
      {
         get 
         {
            return applicationTitle;
         }
      }
      
      public static Boolean IsService
      {
         get { return (runAsService == true); }
      }
            
      private static void CONSOLE()
      {
         TheRFID_Server theST = new TheRFID_Server();
         
         theST.Start();
         Console.ReadLine();
         theST.Stop();
         theST = null;
         Thread.Sleep(1000);
         
         Console.WriteLine("CONSOLE: Nochmal ENTER zum beenden !");
         Console.ReadLine();
      }
      
      
      public static void DevLog(string filename, string log)
      {
         try
         {
            StreamWriter file = new StreamWriter(filename, true);
            file.WriteLine(log);
            file.Close();
         }
         catch(Exception e)
         {
            Console.WriteLine("CONSOLE: DevLog-> {0}", e.Message);
         }
      }
      
      public static void Main(string[] args)
      {
         LogConsole.InitConsole();
         
         string parameter = "";
         foreach(string arg in args)
         {
            parameter += arg;
         }
         
         if(parameter.ToUpper().Contains("DEBUG") || parameter.ToUpper().Contains("CONSOLE"))
         {      
            CONSOLE();
            return;
         }
      
         if (AccountInfo.IsService())
         {
            //Als Dienst gestartet
            installServiceMode = false;
            runAsService = true;

            DevLog("c:\\Account.log", string.Format("{0} Als Dienst gestartet -> {1}", DateTime.Now.ToString("dd.MM.yy HH:mm:ss.fff "), AccountInfo.CurrentUser()));
         }
         else
         {
            //Von Console gestartet (Dienst installieren)
            installServiceMode = true;
            runAsService = false;
         }
         
         
         if (installServiceMode)
         {
            if (!ServiceExists())
            {
               if (!InstallService())
               {
                  Console.WriteLine("CONSOLE: Dienst NICHT installiert ! Ab WIN7 mu� die EXE dazu als Administrator ausgef�hrt werden !");
                  Console.ReadLine();
               }
               else
               {
                  Console.WriteLine(string.Format("CONSOLE: Dienst {0} installiert !", applicationName));
                  Thread.Sleep(1500);
               }
            }
            else
            {
               if(ServiceRunning())
               {
                  Console.WriteLine(string.Format("CONSOLE: Dienst {0} l�uft gerade ! !", applicationName));
                  Console.WriteLine(string.Format("CONSOLE: Erst beenden !"));
               }
               else
               {
                  UnInstallService();
                  Console.WriteLine(string.Format("CONSOLE: Dienst {0} deinstalliert !", applicationName));
                  Thread.Sleep(1500);
               }
            }
         }
         else
         {  
            if (runAsService)
            {
               if (ServiceExists())
               {
                  ServiceBase[] services = new ServiceBase[] { new ServiceWrapper() };
                  ServiceBase.Run(services);
               }
            }
         }          
      }

      static bool ServiceExists()
      {
         bool installed = false;

         ServiceController[] controllers = ServiceController.GetServices();
         foreach (ServiceController con in controllers)
         {
            if (con.ServiceName == applicationName)
            {
               installed = true;
               break;
            }
         }

         return installed;
      }

      static bool ServiceRunning()
      {
         bool running = false;

         ServiceController[] controllers = ServiceController.GetServices();
         foreach (ServiceController con in controllers)
         {
            if (con.ServiceName == applicationName)
            {
               if (con.Status == ServiceControllerStatus.Running)
               {
                  running = true;                  
               }
               break;
            }
         }

         return running;
      }

      static Boolean InstallService()
      {
         return ServiceInstaller.InstallService("\"" + applicationPath + "\\" + applicationName + ".exe\" -service", applicationName, applicationDescription, applicationTitle, true, false);
      }

      static void UnInstallService()
      {
         ServiceController controller = new ServiceController(applicationName);
         if (controller.Status == ServiceControllerStatus.Running)
         {
            controller.Stop();
         }
         if(!ServiceInstaller.UnInstallService(applicationName))
         {
            Console.WriteLine("CONSOLE: FEHLER beim Deinstallieren....");
         }
      }
   }
}
