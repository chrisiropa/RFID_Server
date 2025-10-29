using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace RFID_Server
{
   public class ServiceMain
   {
      private Thread mainThread;
      private TcpListener listener;
      private volatile bool running;
      private IPAddress listenAddress = IPAddress.Parse("192.168.21.126");
      private int listenPort = 2168;
      public delegate void CardSeenHandler(string uid, string user);
      public event CardSeenHandler CardSeen;
      public void Start()
      {
         running = false;

         mainThread = new Thread(new ThreadStart(MainThread));
         mainThread.Start();
      }
      
      public void Init()
      {  
      }

      public void MainThread()
      {
         listener = new TcpListener(listenAddress, listenPort);

         running = true;
         listener.Start();
         Console.WriteLine("RFID Server lauscht auf " + listener.LocalEndpoint);

         while (running)
         {
            try
            {
               if (!listener.Pending())
               {
                  Thread.Sleep(100);
                  continue;
               }

               var client = listener.AcceptTcpClient();
               ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
            catch (SocketException) 
            { 
               /* Listener gestoppt */ 
            }
            catch (Exception ex)
            {
               Console.WriteLine("Accept error: " + ex.Message);
            }
         }
      }

      private void HandleClient(object clientObj)
      {
            var client = (TcpClient)clientObj;
            var remote = client.Client.RemoteEndPoint;
            Console.WriteLine($"Verbunden: {remote}");

            try
            {
               var stream = client.GetStream();
               var buffer = new byte[2048];
               int read = stream.Read(buffer, 0, buffer.Length);

               if (read > 0)
               {
                  Console.WriteLine($"Empfangen {read} Bytes von {remote}");

                  // Nur die tatsächlich empfangenen Bytes extrahieren
                  byte[] data = new byte[read];
                  Array.Copy(buffer, data, read);

                  // Nur wenn genau 42 Bytes empfangen wurden
                  if (data.Length == 42)
                  {
                     //Erstmal alles printen
                     byte[] bytes = ExtractPart(data, 0, 42);
                     string alles = BitConverter.ToString(bytes).Replace("-", "");
                        
                         
                        bytes = ExtractPart(data, 0, 4);
                        string reserved = BitConverter.ToString(bytes).Replace("-", "");
                        bytes = ExtractPart(data, 4, 1);
                        string dataType = BitConverter.ToString(bytes).Replace("-", "");
                        bytes = ExtractPart(data, 5, 1);
                        string dataSize = BitConverter.ToString(bytes).Replace("-", "");
                        bytes = ExtractPart(data, 6, 4);
                        string payLoad = BitConverter.ToString(bytes).Replace("-", "");
                        bytes = ExtractPart(data, 38, 4);
                        string ident = BitConverter.ToString(bytes).Replace("-", "");



                        Console.WriteLine("Alles    = {0}", alles);
                        Console.WriteLine("Reserved = {0}", reserved);
                        Console.WriteLine("DataType = {0}", dataType);
                        Console.WriteLine("DataSize = {0}", dataSize);
                        Console.WriteLine("PayLoad  = {0}", payLoad);
                        Console.WriteLine("Ident    = {0}", ident);

                  }
                  else
                  {
                        Console.WriteLine($"Ungültige Länge ({data.Length}). Erwartet: 42 Bytes.");
                  }
               }
               else
               {
                  Console.WriteLine($"Keine Daten empfangen von {remote}");
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Fehler bei Client {remote}: {ex.Message}");
            }
            finally
            {
               client.Close();
               Console.WriteLine($"Verbindung geschlossen: {remote}");
            }
      }


      public static byte[] ExtractPart(byte[] data, int start, int len)
      {
         if (data == null)
            throw new ArgumentNullException(nameof(data));

         if (data.Length != 42)
            throw new ArgumentException($"Expected 42 bytes, got {data.Length}.");

         // Beispiel: nimm alles ab Byte 4 bis 41 als "Payload"
         // (kannst du bei Bedarf anpassen)
         int payloadStart = start;
         int payloadLength = len;

         byte[] payload = new byte[payloadLength];
         Array.Copy(data, payloadStart, payload, 0, payloadLength);

         return payload;
      }

      public void Stop()
      {
         running = false;
         try 
         { 
            listener.Stop(); 
         } 
         catch 
         { 
         }
      }
   }
}
