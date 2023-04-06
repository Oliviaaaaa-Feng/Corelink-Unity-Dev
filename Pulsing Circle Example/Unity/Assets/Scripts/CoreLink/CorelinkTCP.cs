using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

namespace CoreLink
{  /// <summary>
   /// This class was used as the control connection which will be deprecated soon in favor of WebSockets
   /// </summary>
  public class CorelinkTCP : ControlStream
  {
    private TcpClient tcpClient;
    private string response = null;
    private Thread clientReceiveThread;
    // Boolean flag to tell that thread to stop listening
    private bool shouldExit;
    private Byte[] bytes;
    private int MAX_BUF_SIZE = 65536; // 2^16
    private bool isTCPDone = false;
    // Get a stream object for reading 				
    private NetworkStream stream;
    public CorelinkTCP(Config config)
    {
      // Initialize TCP Connection
      Control.Print("Initializing TCP Connection");
      tcpClient = new TcpClient(config.dataIP, config.controlPort);
      stream = tcpClient.GetStream();
      // TODO: Figure out how to use TCP KeepAlives instead of a perpetual while loop
      //tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

      // Start a separate thread to continue listen to TCP to keep the connection alive
      StartTCPListenThread();
    }
    /**
     * Function to start a perpetually background thread to receive TCP data
     * and keep the control TCP port alive
     */
    void StartTCPListenThread()
    {
      Debug.Log("StartTCPListenThread");
      try
      {
        clientReceiveThread = new Thread(new ThreadStart(ListenForTCPData));
        //clientReceiveThread.IsBackground = true;
        clientReceiveThread.Start();
      }
      catch (Exception e)
      {
        Debug.Log("On client connect exception " + e);
      }
    }
    public string GetResponse()
    {
      int iterations = 0;
      while (string.IsNullOrEmpty(response) && iterations < Control.timeoutIterations)
      {
        Thread.Sleep(100);
        iterations++;
      }
      if (iterations == Control.timeoutIterations)
      {
        throw new Exception("Timeout error");
      }
      string retval = response;
      response = null;
      return retval;
      //string serverMessage = "";
      //bytes = new Byte[MAX_BUF_SIZE];
      //try
      //{
      //  // Get a stream object for reading 				
      //  using (NetworkStream stream = tcpClient.GetStream())
      //  {
      //    int length;
      //    // Read incoming stream into byte array. 
      //    while ((length = stream.Read(bytes, 0, bytes.Length)) == 0)
      //    {
      //      Debug.Log("Waiting for input");
      //      Thread.Sleep(100);
      //    }

      //    var incommingData = new byte[length];
      //    Array.Copy(bytes, 0, incommingData, 0, length);
      //    // Convert byte array to string message. 						
      //    serverMessage = Encoding.ASCII.GetString(incommingData);
      //    //response = serverMessage;

      //  }
      //}
      //catch (SocketException socketException)
      //{
      //  Debug.Log("Socket exception: " + socketException);
      //}
      //return serverMessage;
    }
    void ListenForTCPData()
    {
      //Debug.Log("ListenForTCPData");
      try
      {
        // connect to relay first
        //tcpClient.Connect(IPAddress.Parse(config.ControlIP), config.ControlPort);

        bytes = new Byte[MAX_BUF_SIZE];
        while (!shouldExit)
        {
          int length;
          try
          {
            // Read incoming stream into byte array. 
            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
              if (shouldExit)
                break;
              byte[] incomingData = new byte[length];
              Array.Copy(bytes, 0, incomingData, 0, length);
              // Convert byte array to string message. 						
              string serverMessage = Encoding.ASCII.GetString(incomingData);
              //Debug.Log("server message received as: " + serverMessage);
              response = serverMessage;
            }
          }
          catch (System.IO.IOException e)
          {
            Control.Print("Catching IO Error from ending the stream and (hopefully) continuing");
          }
        }
        isTCPDone = true;
        Control.Print("Closing TCP Connection");
      }
      catch (SocketException socketException)
      {
        Debug.LogException(socketException);
      }
    }
    override public void sendMessage(string request)
    {
      if (tcpClient == null)
      {
        return;
      }

      //Debug.Log("SendWS:" + request);
      try
      {
        // Get a stream object for writing. 			
        NetworkStream stream = tcpClient.GetStream();
        if (stream.CanWrite)
        {
          // Convert string message to byte array.                 
          byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(request);
          // Write byte array to socketConnection stream.                 
          stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
        }
      }
      catch (SocketException socketException)
      {
        Debug.LogException(socketException);
        throw new Exception("Socket exception: " + socketException);
      }
    }
    override public void stopConnection()
    {
      shouldExit = true;
      stream.Close();
      tcpClient.Close();
    }
  }

}
