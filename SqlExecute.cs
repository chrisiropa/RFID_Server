using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace RFID_Server
{
   public class SqlExecute
   {
      private string stmt;
      private int affectedRows = -1;
      
      public int AffectedRows
      {
         get
         {
            return affectedRows;
         }
      }

      public SqlExecute(string formatString, params object[] paramObjects)
      {
         this.stmt = string.Format(formatString, paramObjects);

         Execute();
      }

      private void Execute()
      {
         SqlConnection sqlConnection = TheRFID_Server.GetSingleton().DCB.ForceDatabaseConnection();

         if (sqlConnection == null)
         {
            throw new Exception("SqlExecute.Execute -> Störung der Datenbankverbindung !");
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
            throw new Exception(string.Format("SqlExecute -> Fehler beim Ausführen des Statements: #{0}# -> {1} ", stmt, e.Message));
         }
         finally
         {
            sqlConnection.Close();
         }
      }
   }
}
