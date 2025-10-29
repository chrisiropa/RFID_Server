using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
         listener = new TcpListener(listenAddress, listenPort);
      }

      public void MainThread()
      {
         running = true;
         listener.Start();

         TheRFID_Server.TheST.ZLog(ELF.INFO, "RFID Server lauscht auf " + listener.LocalEndpoint);

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
               TheRFID_Server.TheST.ZLog(ELF.INFO, "MainThread Exception = {0}", ex.Message); 
            }
         }
      }

      private void HandleClient(object clientObj)
      {
          var client = (TcpClient)clientObj;
          var remote = client.Client.RemoteEndPoint;
          TheRFID_Server.TheST.ZLog(ELF.INFO, $"Verbunden: {remote}");

          try
          {
              var stream = client.GetStream();
              var buffer = new byte[4096]; // größerer Zwischenpuffer
              var receivedData = new List<byte>();

              while (client.Connected)
              {
                  if (!stream.DataAvailable)
                  {
                      Thread.Sleep(10); // CPU schonen
                      continue;
                  }

                  int bytesRead = stream.Read(buffer, 0, buffer.Length);
                  if (bytesRead <= 0)
                      break; // Verbindung geschlossen

                  // Gelesene Bytes anhängen
                  receivedData.AddRange(buffer.Take(bytesRead));
                  TheRFID_Server.TheST.ZLog(ELF.INFO, $"Empfangen {bytesRead} Bytes von {remote}");

                  // Solange wir mindestens 42 Bytes haben → verarbeiten
                  while (receivedData.Count >= 42)
                  {
                      byte[] frame = receivedData.Take(42).ToArray();
                      receivedData.RemoveRange(0, 42);

                      ProcessFrame(frame, remote);
                  }
              }
          }
          catch (Exception ex)
          {
              TheRFID_Server.TheST.ZLog(ELF.INFO, $"Fehler bei Client {remote}: {ex.Message}");
          }
          finally
          {
              client.Close();
              TheRFID_Server.TheST.ZLog(ELF.INFO, $"Verbindung geschlossen: {remote}");
          }
      }

      private void ProcessFrame(byte[] data, EndPoint remote)
      {
         if (data.Length != 42)
         {
            TheRFID_Server.TheST.ZLog(ELF.INFO, $"Ungültige Länge ({data.Length}). Erwartet: 42 Bytes.");
            return;
         }

         byte[] bytes;

         string alles = BitConverter.ToString(data).Replace("-", "");

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

         TheRFID_Server.TheST.ZLog(ELF.INFO, "Alles    = {0}", alles);
         TheRFID_Server.TheST.ZLog(ELF.INFO, "Reserved = {0}", reserved);
         TheRFID_Server.TheST.ZLog(ELF.INFO, "DataType = {0}", dataType);
         TheRFID_Server.TheST.ZLog(ELF.INFO, "DataSize = {0}", dataSize);
         TheRFID_Server.TheST.ZLog(ELF.INFO, "PayLoad  = {0}", payLoad);
         TheRFID_Server.TheST.ZLog(ELF.INFO, "Ident    = {0}", ident);
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
