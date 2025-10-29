using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace RFID_Server
{
   public partial class ServiceWrapper : ServiceBase
   {
      TheRFID_Server theST = new TheRFID_Server();

      public ServiceWrapper()
      {
         InitializeComponent();
      }

      protected override void OnStart(string[] args)
      {
         string applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         theST.Start();            
      }

      protected override void OnStop()
      {
         theST.Stop();
         theST = null;
         Thread.Sleep(1000);
      }
   }    
}
