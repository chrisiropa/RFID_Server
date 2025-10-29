using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace RFID_Server
{
   public class SimpleSqlQuery
   {
      private string connectionString;
      private string query;
      private Exception exception = null;
      
      public Exception Exception
      {
         get { return exception; }
      }

      private List<Dictionary<string, object>> queryResult = new List<Dictionary<string, object>>();

      public List<Dictionary<string, object>> QueryResult
      {
         get { return queryResult; }
      }


      public SimpleSqlQuery(string connectionString, string formatString, params object[] paramObjects)
      {
         this.connectionString = connectionString;
         this.query = string.Format(formatString, paramObjects);

         Execute();
      }

      private void Execute()
      {
         SqlDataReader dataReader = null;
         SqlConnection sqlConnection = new SqlConnection(connectionString);

         try
         {
            sqlConnection.Open();
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandText = query;

            dataReader = dataCommand.ExecuteReader();
            while (dataReader.Read())
            {
               Dictionary<string, object> dict = new Dictionary<string, object>();

               for (int i = 0; i < dataReader.FieldCount; i++)
               {
                  string fieldName = dataReader.GetName(i);

                  object value = dataReader[fieldName];

                  dict[fieldName] = value;
               }

               queryResult.Add(dict);
            }

            dataReader.Close();
         }

         catch (Exception e)
         {
            queryResult = null;
            exception = e;
         }
         finally
         {
            if (dataReader != null)
            {
               dataReader.Close();
            }

            sqlConnection.Close();
         }
      }
   }
}
