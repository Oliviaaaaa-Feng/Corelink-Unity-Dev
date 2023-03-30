using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;

namespace CoreLink
{
  // Code from https://www.patrykgalach.com/2019/11/11/implementing-websocket-in-unity/
  public class CorelinkWebSocket : ControlStream
  {
    /// <summary>
    /// CorelinkWebSocket class.
    /// Responsible for handling control communication between server and client.
    /// </summary>

    // WebSocket
    private ClientWebSocket ws;
    private UTF8Encoding encoder; // For websocket text message encoding.
    private const UInt64 MAXREADSIZE = 1 * 1024 * 1024;
    // Server address
    private Uri serverUri;

    // Queues
    public ConcurrentQueue<String> receiveQueue { get; }
    public BlockingCollection<ArraySegment<byte>> sendQueue { get; }

    // Threads
    private Thread receiveThread { get; set; }
    private Thread sendThread { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:WsClient"/> class.
    /// </summary>
    /// <param name="serverURL">Server URL.</param>
    public CorelinkWebSocket(string serverURL)
    {
      encoder = new UTF8Encoding();
      ws = new ClientWebSocket();
      //X509CertificateCollection certs = ws.Options.ClientCertificates;
      //certs.Add(new X509Certificate("D:\\School\\College\\GradSchool\\HolodeckWork\\CoreLink\\networktest\\server\\config\\ca-crt.pem"));
      //certs.Add(new X509Certificate("D:\\School\\College\\GradSchool\\HolodeckWork\\CoreLink\\networktest\\server\\config\\server-crt.pem"));
      //ws.Options.ClientCertificates = certs;

      serverUri = new Uri("wss://" + serverURL + ":20012");

      receiveQueue = new ConcurrentQueue<string>();
      receiveThread = new Thread(runReceive);
      receiveThread.Start();

      sendQueue = new BlockingCollection<ArraySegment<byte>>();
      sendThread = new Thread(runSend);
      sendThread.Start();
    }

    /// <summary>
    /// Method which connects client to the server.
    /// </summary>
    /// <returns>The connect.</returns>
    public async Task connect()
    {

      //ws.Options.RemoteCertificateValidationCallback = ValidateServerCertificate;
      //ws.RemoteCertificateValidationCallback
      //certs.Add()
      Control.Print("Connecting to: " + serverUri);
      // Removed the await keyword to end this gracefully if it can't connect instead of crashing unity
      ws.ConnectAsync(serverUri, CancellationToken.None);
      while (isConnecting())
      {
        Control.Print("Waiting to connect...");
        Task.Delay(100).Wait();
      }
      Control.Print("Connect status: " + ws.State);
    }
    //// The following method is invoked by the RemoteCertificateValidationDelegate.
    //public static bool ValidateServerCertificate(
    //  object sender,
    //  X509Certificate certificate,
    //  X509Chain chain,
    //  SslPolicyErrors sslPolicyErrors)
    //{
    //  if (sslPolicyErrors == SslPolicyErrors.None)
    //    return true;

    //  Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

    //  // Do not allow this client to communicate with unauthenticated servers.
    //  return false;
    //}
    override public void stopConnection()
    {
      ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
      while (!(ws.State == WebSocketState.CloseReceived))
      {
        Control.Print("Waiting to disconnect...");
        Task.Delay(50).Wait();
      }
      Control.Print("Connect status: " + ws.State);
    }
    #region [Status]

    /// <summary>
    /// Return if is connecting to the server.
    /// </summary>
    /// <returns><c>true</c>, if is connecting to the server, <c>false</c> otherwise.</returns>
    public bool isConnecting()
    {
      return ws.State == WebSocketState.Connecting;
    }

    /// <summary>
    /// Return if connection with server is open.
    /// </summary>
    /// <returns><c>true</c>, if connection with server is open, <c>false</c> otherwise.</returns>
    public bool isConnectionOpen()
    {
      return ws.State == WebSocketState.Open;
    }

    #endregion

    #region [Send]

    /// <summary>
    /// Method used to send a message to the server.
    /// </summary>
    /// <param name="message">Message.</param>
    override public void sendMessage(string message)
    {
      byte[] buffer = encoder.GetBytes(message);
      //Control.Print("Message to queue for send: " + buffer.Length + ", message: " + message);
      var sendBuf = new ArraySegment<byte>(buffer);

      sendQueue.Add(sendBuf);
    }
    public string getResponse()
    {
      // Check if server send new messages
      var cqueue = receiveQueue;
      string msg;
      int iterations = 0;
      while (!cqueue.TryPeek(out msg) && iterations < Control.timeoutIterations)
      {
        Thread.Sleep(100);
        iterations++;

      }
      if (iterations == Control.timeoutIterations)
      {
        throw new Exception("Timeout error");
      }
      cqueue.TryDequeue(out msg);

      return msg;
    }
    /// <summary>
    /// Method for other thread, which sends messages to the server.
    /// </summary>
    private async void runSend()
    {
      Control.Print("WebSocket Message Sender looping.");
      ArraySegment<byte> msg;
      while (true)
      {
        while (!sendQueue.IsCompleted)
        {
          msg = sendQueue.Take();
          //Control.Print("Dequeued this message to send: " + msg);
          await ws.SendAsync(msg, WebSocketMessageType.Text, true /* is last part of message */ , CancellationToken.None);
        }
      }
    }

    #endregion

    #region [Receive]

    /// <summary>
    /// Reads the message from the server.
    /// </summary>
    /// <returns>The message.</returns>
    /// <param name="maxSize">Max size.</param>
    private async Task<string> receive(UInt64 maxSize = MAXREADSIZE)
    {
      // A read buffer, and a memory stream to stuff unknown number of chunks into:
      byte[] buf = new byte[4 * 1024];
      var ms = new MemoryStream();
      ArraySegment<byte> arrayBuf = new ArraySegment<byte>(buf);
      WebSocketReceiveResult chunkResult = null;

      if (isConnectionOpen())
      {
        do
        {
          chunkResult = await ws.ReceiveAsync(arrayBuf, CancellationToken.None);
          ms.Write(arrayBuf.Array, arrayBuf.Offset, chunkResult.Count);
          //Control.Print("Size of Chunk message: " + chunkResult.Count);
          if ((UInt64)(chunkResult.Count) > MAXREADSIZE)
          {
            Console.Error.WriteLine("Warning: Message is bigger than expected!");
          }
        } while (!chunkResult.EndOfMessage);
        ms.Seek(0, SeekOrigin.Begin);

        // Looking for UTF-8 JSON type messages.
        if (chunkResult.MessageType == WebSocketMessageType.Text)
        {
          return streamToString(ms, Encoding.UTF8);
        }
      }
      return "";
    }

    /// <summary>
    /// Method for other thread, which receives messages from the server.
    /// </summary>
    private async void runReceive()
    {
      Control.Print("WebSocket Message Receiver looping.");
      string result;
      while (true)
      {
        //Control.Print("Awaiting Receive...");
        result = await receive();
        if (result != null && result.Length > 0)
        {
          JSONNode response = JSON.Parse(result);
          if (response["function"])
          {
            string function = response["function"].Value;
            if (function.Equals("subscriber"))
            {
              onSubscriber(result);
            }
            else if (function.Equals("update"))
            {
              onUpdate(result);
            }
            else if (function.Equals("stale"))
            {
              onStale(result);
            }
            else if (function.Equals("dropped"))
            {
              onDropped(result);
            }
            else
            {
              Control.Print("Unknown callback. Maybe this library is outdated?");
            }
          }
          else
          {
            receiveQueue.Enqueue(result);
          }

        }
        else
        {
          Task.Delay(50).Wait();
        }
      }
    }

    #endregion

    #region [Utility]
    /// <summary>
    /// Converts memory stream into string.
    /// </summary>
    /// <returns>The string.</returns>
    /// <param name="ms">Memory Stream.</param>
    /// <param name="encoding">Encoding.</param>
    public static string streamToString(MemoryStream ms, Encoding encoding)
    {
      string readString = "";
      if (encoding == Encoding.UTF8)
      {
        using (var reader = new StreamReader(ms, encoding))
        {
          readString = reader.ReadToEnd();
        }
      }
      return readString;
    }
    #endregion
  }
}
