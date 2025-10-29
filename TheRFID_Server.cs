using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Threading;
using System.IO;
using System.Reflection;

namespace RFID_Server
{
   public class TheRFID_Server
   {
      public static TheRFID_Server TheST = null;
      private Logging logging = new Logging();
      private ServiceMain serviceMain = new ServiceMain();
      private DatabaseConnectionProvider databaseConnectionProvider = new DatabaseConnectionProvider();
      
      
      public TheRFID_Server()
      {
         TheRFID_Server.TheST = this;
      }

      public DatabaseConnectionProvider DCB
      {
         get { return databaseConnectionProvider; }
      }
      
      private void InitThread()
      {
         logging.Run();
         
         
         serviceMain.Init();
         
         serviceMain.Start();
      }

      
   
      public void Start()
      {
         ConfigManager.Init();         
      
         new Thread(new ThreadStart(InitThread)).Start();
      
         Thread.Sleep(1000);
      }

      public void Stop()
      {
         TheRFID_Server.TheST.ZLog(ELF.INFO, "ServiceMain wird beendet...");      
         
         serviceMain.Stop();

         TheRFID_Server.TheST.ZLog(ELF.SUCCESS, "SUCCESS !");
         
         
         TheRFID_Server.TheST.ZLog(ELF.INFO, "Logging wird beendet...");      
         
         logging.Stop();

         TheRFID_Server.TheST.ZLog(ELF.SUCCESS, "SUCCESS !");
      }

      public void ZLog(ELF logFlags, string formatString, params object[] paramObjects)
      {
         string threadInfo = string.Format("{0}:{1}", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
         LogEintrag logEintrag = new LogEintrag(logFlags, string.Format(formatString, paramObjects), DateTime.Now, threadInfo);
         
         logging.Log(logEintrag);
      }

      public static TheRFID_Server GetSingleton()
      {
         return TheST;
      }
   }
}
