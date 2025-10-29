
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data.SqlClient;

namespace RFID_Server
{
   public class DatabaseConnectionProvider
   {
      private Boolean databaseConnectionOk;
      
      public DatabaseConnectionProvider()
      {
         databaseConnectionOk = false;
      }
   
      public SqlConnection GetOpenDBConnection()
      {
         if(databaseConnectionOk)
         {
            SqlConnection sqlConnection = null;
         
            if(DatabaseOk(out sqlConnection))
            {
               databaseConnectionOk = true;
               return sqlConnection;         
            }

            databaseConnectionOk = false;
         }

         TheRFID_Server.TheST.ZLog(ELF.ERROR, "GetOpenDBConnection -> Datenbankverbindung als gestört markiert !");
         
         return null;
      }

      public SqlConnection ForceDatabaseConnection()
      {
         SqlConnection sqlConnection = null;

         TheRFID_Server.TheST.ZLog(ELF.INFO, "DatabaseConnectionProvider -> Versuche DB-Verbindung zu erzwingen...");

         if (DatabaseOk(out sqlConnection))
         {
            databaseConnectionOk = true;
            TheRFID_Server.TheST.ZLog(ELF.INFO, "DatabaseConnectionProvider -> ... SUCCESS");
            return sqlConnection;
         }

         databaseConnectionOk = false;
         TheRFID_Server.TheST.ZLog(ELF.INFO, "DatabaseConnectionProvider -> ... FAILED");
         return null;
      }

      private Boolean DatabaseOk(out SqlConnection sqlConnection)
      {
         Boolean dbOk = false;         
         sqlConnection = null;
         SqlDataReader dataReader = null;
         SqlCommand dataCommand = new SqlCommand();
         
         try
         {
            sqlConnection = new SqlConnection(ConfigManager.GetSingleton().ConnectionString);
            
            sqlConnection.Open();            
            
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = 10;
            dataCommand.CommandText = "select top 0 ID from ORG_Rechte";
         }
         catch
         {
            if (sqlConnection != null)
            {
               sqlConnection.Close();
            }

            TheRFID_Server.TheST.ZLog(ELF.ERROR, "DatabaseConnectionProvider.DatabaseOk(Open FAILED) -> Datenbankverbindung gestört");
            return dbOk;
         }

         try
         {
            dataReader = dataCommand.ExecuteReader();
            dbOk = true;
         }
         catch
         {
            TheRFID_Server.TheST.ZLog(ELF.ERROR, "DatabaseConnectionProvider.DatabaseOk(Select FAILED) -> Datenbankverbindung gestört");
            sqlConnection = null;
            return dbOk;
         }
         finally
         {
            if(dataReader != null)
            {
               dataReader.Close();
            }
         }
         
         return dbOk;
      }
   }
}
