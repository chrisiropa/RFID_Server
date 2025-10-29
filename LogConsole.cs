using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace RFID_Server
{
   class LogConsole
   {
      public static void InitConsole()
      {
         try
         {
            //Schon hier machen, sonst verschwinden wichtige Consoleneintr�ge
            Console.SetWindowSize(160, 75);
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Clear();
         }
         catch
         {
            //Wenn die APP als Dienst l�uft schmiert er hier ab !!!
         }
      }
   
      public LogConsole()
      {
         try
         {
            //Console.SetWindowSize(160, 75);
            //Console.BackgroundColor = ConsoleColor.DarkRed;
            //Console.Clear();
         }
         catch
         {
            //Wenn die APP als Dienst l�uft schmiert er hier ab !!!
         }

      }

      public void Log(LogEintrag logEintrag)
      {
         try
         {
            ConsoleColor color = Console.ForegroundColor;

            

            Console.ForegroundColor = logEintrag.GetColor(logEintrag.LogFlags);

            Console.WriteLine(string.Format("{2} {0} {3} {1}", logEintrag.ZeitStempel.ToString("dd.MM.yy HH:mm:ss.fff "), logEintrag.Text, logEintrag.LogFlagAsText, logEintrag.ThreadInfo));

            Console.ForegroundColor = color;
         }
         catch(Exception e)
         {
            Console.WriteLine("CONSOLE POS3: {0}", e.Message);
         }
      }
   }
}
