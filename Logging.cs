using System;
using System.Collections.Generic;
using System.Text;

namespace RFID_Server 
{
   public class Logging
   {
      private LogZentrale logZentrale; 
      private LogFile logFile;
      private LogConsole logConsole;

      private Boolean inf;
      private Boolean err;
      private Boolean war;
      private Boolean dev;
      
      public Logging()
      {
      }
      
      public void Init()
      {
         string info = "1";
         string error = "1";
         string warning = "1";
         string developer = "0";

         inf = info == "1";
         err = error == "1";
         war = warning == "1";
         dev = developer == "1";  
      }
      
      public void Run()
      {  
         Init();
         
         logZentrale = new LogZentrale(inf, war, err, dev);
         logZentrale.Start();
         
         logFile = new LogFile(ConfigManager.LogfilePath); 
         
         logZentrale.Register(logFile.Log);         
         
         logConsole = new LogConsole();
         logZentrale.Register(logConsole.Log);                
      }

      public void Stop()
      {  
         logZentrale.Stop();
      }

      public void Log(LogEintrag logEintrag)
      {
         logZentrale.Log(logEintrag);
      }
   }
}
