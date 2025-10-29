

using Microsoft.Win32;
using RFID_Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace RFID_Server
{

   public class ConfigManager
   {
      private string connectionString;
      private string executionDirectory = "";
      string instanzName = "";
      
      public static string IropaRegistryPath = "SOFTWARE\\IROPA\\RFID_Server";
      private static string logfileName = "RFID_Server.LOG";
      private string appName = "";
            
      private static ConfigManager config = null;

      private Dictionary<string, string> parameters = new Dictionary<string, string>();

      public static string KeyServer = "Server";
      public static string KeyDatabase = "Database";
      public static string KeyUserID = "UserID";
      public static string KeyPassword = "Password";
      public static string KeyLanguage = "Language";
      public static string KeyConnectTimeout = "ConnectTimeout";
      public static string KeyLogfilePath = "LogfilePath";
      
      private static string defaultDatabase = "LagerV";
      private static string defaultServer = "DEV11\\DEV2022";
      private static string defaultUserID = "sa";
      private static string defaultPassword = "IR_apori72";
      private static string defaultLanguage = "German";
      private static string defaultConnectTimeout = "10";
      
      public static string Server;
      public static string Database;
      public static string UserID;
      public static string Password;
      public static string Language;
      public static string ConnectTimeout; 
      public static string LogfilePath;
      
      
      public string ExecutionDirectory
      {
         get { return executionDirectory; }
      }  
               
      public string ConnectionString
      {
         get { return connectionString; }
      }

		private void BeginNewLogfile(string path)
      {
			try
         {
            if (File.Exists(path))
            {
               string historyFile = path.Insert(path.Length - 4, DateTime.Now.ToString(" ddMMyyyyHHmmssfff"));

               System.IO.FileInfo fi = new FileInfo(path);
               fi.MoveTo(historyFile);
            }
         }
         catch (Exception e4)
         {
            Console.WriteLine("Exception in ConfigManager.BeginNewLogfile -> {0}", e4.Message);              
         }
      }
      
      private ConfigManager()
      {         
         try
         {
				ConfigManager.Server = GetSetValue("INS", appName);
            
            ConfigManager.Server = GetSetValue(KeyServer, defaultServer);
            ConfigManager.Database = GetSetValue(KeyDatabase, defaultDatabase);
            ConfigManager.UserID = GetSetValue(KeyUserID, defaultUserID);
            ConfigManager.Password = GetSetValue(KeyPassword, defaultPassword);
            ConfigManager.Language = GetSetValue(KeyLanguage, defaultLanguage);
            ConfigManager.ConnectTimeout = GetSetValue(KeyConnectTimeout, defaultConnectTimeout);

            ConfigManager.LogfilePath = string.Format("{0}\\{1}", GetSetValue(KeyLogfilePath, executionDirectory), logfileName); 
            
            BeginNewLogfile(ConfigManager.LogfilePath);
            
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
         
            csb.DataSource = Server;
            csb.InitialCatalog = Database;
            try
            {
               csb.ConnectTimeout = Convert.ToInt32(ConnectTimeout);
            }
            catch
            {
               csb.ConnectTimeout = 30;               
            }
            csb.CurrentLanguage = Language;
            csb.PersistSecurityInfo = true;
            csb.UserID = UserID;
            csb.Password = Password;
            csb.ApplicationName = "RFID_Server";
                        
            connectionString = csb.ConnectionString;
            
            
            Console.WriteLine("ConnectionString = {0}", connectionString);

         }
         catch (Exception e)
         {
            Console.WriteLine("ConfigManger.ConfigManager{0}", e.Message);
         }         
      }
      
      private static string GetSetValue(string key, string defaultValue)
      {
         try
         {      
            RegistryKey iropaDatenSystemKey = Registry.LocalMachine.OpenSubKey(IropaRegistryPath, true);

            if (iropaDatenSystemKey == null)
            {
               iropaDatenSystemKey = Registry.LocalMachine.CreateSubKey(IropaRegistryPath);
            }
            if (iropaDatenSystemKey == null)
            {
               return null;
            }

            
            string value = (string)iropaDatenSystemKey.GetValue(key);
            if (value == null)
            {
               value = defaultValue;
               iropaDatenSystemKey.SetValue(key, value);
            }

            iropaDatenSystemKey.Close();
            
            return value;
         }
         catch(Exception e)
         {
            Console.WriteLine("Kein Zugriff auf die Registry");
            Console.WriteLine(e.Message);
            throw e;
         }
      }
      
      public static ConfigManager GetSingleton()
      {
         return config;
      }

      public static void Init()
      {
         if (config == null)
         {
            config = new ConfigManager();
         }
      }
   }
}
