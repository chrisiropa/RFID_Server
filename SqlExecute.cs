using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace RFID_Server
{
   public class SqlExecute
   {
      private string connectionString;
      private string stmt;
      private int affectedRows = -1;
      public string Exception = "";

      
      public int AffectedRows
      {
         get
         {
            return affectedRows;
         }
      }

      public SqlExecute(string connectionString, string formatString, params object[] paramObjects)
      {
         this.connectionString = connectionString;
         this.stmt = string.Format(formatString, paramObjects);

         Execute();
      }

      private void Execute()
      {
         SqlConnection sqlConnection = TheRFID_Server.GetSingleton().DCB.ForceDatabaseConnection();

         if (sqlConnection == null)
         {
            Exception = string.Format("SqlExecute -> Konnte keine Datenbankverbindung herstellen ! Statement: #{0}# ", stmt);
			}

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = 10;
            dataCommand.CommandText = stmt;
            affectedRows = dataCommand.ExecuteNonQuery();
            
         }
         catch (Exception e)
         {
            Exception = string.Format("SqlExecute -> Exception bei der Ausführung des Statements: #{0}# -> {1}", stmt, e.Message);  
			}
         finally
         {
            sqlConnection.Close();
         }
      }
   }
}
