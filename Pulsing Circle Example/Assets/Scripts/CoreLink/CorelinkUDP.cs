using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Client to send and receive data to/from CoreLink via UDP
  /// </summary>
  public class CorelinkUDP
  {
    private UdpClient udpClient;
    private int udpPort;
    private Thread udpThread;
    private bool active;
    private string ControlIP;
    private List<uint> allowedStreams;
    private List<ReceiverStreamParams> receiverStreams;
    /// <summary>
    /// Constructor that begins the UDP Connection
    /// </summary>
    /// <param name="ControlIP">IP Address of the server that the data should be sent to</param>
    /// <param name="udpPort">UDP Port on the server that the data should be sent to</param>
    /// <param name="allowedStreams">list of strings of streamIDs that this program is receiving from</param>
    /// <param name="receiverStreams">List of ReceiverStreamParams that will contain the receiver
    /// stream callback functions when data gets called</param>
    public CorelinkUDP(string ControlIP, int udpPort,
      ref List<uint> allowedStreams, ref List<ReceiverStreamParams> receiverStreams)
    {
      this.allowedStreams = allowedStreams;
      this.receiverStreams = receiverStreams;
      this.active = true;
      this.ControlIP = ControlIP;
      this.udpPort = udpPort;
      udpClient = new UdpClient(ControlIP, udpPort);
      startUDPListenThread();
    }


    ////////////////////////////////////////////////////////////////////////////////////////////
    /// RECEIVER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////
    #region [Receiver]

    /// <summary>
    /// Instantiate another thread to start constantly listening to UDP data
    /// </summary>
    public void startUDPListenThread()
    {
      udpThread = new Thread(listenForUDPData);
      udpThread.Start();
    }

    /// <summary>
    /// Function called by a separate thread to begin the async callback function
    ///  to continually receive data until active = false
    /// Active = false is set at the end of the program at StopConnection()
    /// </summary>
    void listenForUDPData()
    {
      IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ControlIP), udpPort);
      Debug.Log("Opening UDP Connection" + ep);

      UdpState state = new UdpState(ep, udpClient);

      udpClient.BeginReceive(new AsyncCallback(UDPReceiveCallback), state);
      // idle time to check if the application quits
      while (active)
      {
        Thread.Sleep(100);
      }

      Debug.Log("Closing UDP Connection");
      udpClient.Close();
    }
    /// <summary>
    /// UDP Receiver callback function
    /// stream id to a port behind a firewall
    /// <param name="ar">Receiver stream ID to register with CoreLink.</param>
    /// </summary>
    void UDPReceiveCallback(IAsyncResult ar)
    {
      if (!active)
      {
        return;
      }
      UdpClient c = ((UdpState)(ar.AsyncState)).c;
      IPEndPoint e = ((UdpState)(ar.AsyncState)).e;
      byte[] buf = udpClient.EndReceive(ar, ref e);

      parseUDPMessage(buf);
      UdpState state = new UdpState(e, c);
      udpClient.BeginReceive(new AsyncCallback(UDPReceiveCallback), state);
    }

    /// <summary>
    /// Parses our UDP Packet into a header and data, and then
    /// calls the callback function associated with a receiver stream.
    /// Called by the UDP listener thread and 
    /// </summary>
    /// <param name="udpBuf">Entire UDP Packet in bytes</param>
    void parseUDPMessage(byte[] udpBuf)
    {
      int headerSize = BitConverter.ToUInt16(udpBuf, 0);
      int dataSize = (int)BitConverter.ToUInt16(udpBuf, 2);
      uint streamID = (uint)BitConverter.ToUInt16(udpBuf, 4);
      int federationID = (int)BitConverter.ToUInt16(udpBuf, 6); // Unused for now
      if (dataSize == 0)
        return;

      byte[] header = new byte[headerSize];
      if (headerSize != 0) 
      {
        Buffer.BlockCopy(udpBuf, 8, header, 0, headerSize);
      }
      byte[] msg = new byte[dataSize];
      Buffer.BlockCopy(udpBuf, 8 + headerSize, msg, 0, dataSize);
      // SimpleJSON.JSONNode root = SimpleJSON.JSON.Parse(Encoding.ASCII.GetString(header));

      if (allowedStreams.Contains(streamID))
      {
        for (int i = 0; i < receiverStreams.Count; i++)
        {
          if (receiverStreams[i].streamIDs.Contains(streamID))
          {
            receiverStreams[i].ReceiverStream.OnMessage(streamID, header, msg);
          }
        }
      }
    }

    /// <summary>
    /// Send an empty packet to the CoreLink server to assign this receiver
    /// stream id to a port behind a firewall
    /// <param name="streamID">Receiver stream ID to register with CoreLink.</param>
    /// </summary>
    public void receiverPing(uint streamID)
    {
      int num = 0;
      byte[] numByte = BitConverter.GetBytes(num);
      byte[] UDPInitMsg = constructUDPMsg(streamID, new byte[0], numByte);
      udpClient.Send(UDPInitMsg, UDPInitMsg.Length);

      Debug.Log("Sent empty receiver packet");
    }
    #endregion

    ////////////////////////////////////////////////////////////////////////////////////////////
    /// SENDER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////
    #region [Sender]
    /// <summary>
    /// Send a message to CoreLink via UDP
    /// </summary>
    /// <param name="streamID">streamID of this message</param>
    /// <param name="header">Optional custom JSON of the data
    /// <param name="data">Data to send, organized into a byte array</param>
    public void send(uint streamID, byte[] header, byte[] data)
    {
      if (udpClient == null) throw new Exception("udpClient undefined");

      try
      {
        byte[] UDPInitMsg = constructUDPMsg(streamID, header, data);
        udpClient.Send(UDPInitMsg, UDPInitMsg.Length);
      }
      catch (SocketException socketException)
      {
        Debug.LogException(socketException);
        throw new Exception("Socket exception: " + socketException);
      }
    }

    /// <summary>
    /// Package a UDP packet with the proper header and data
    /// </summary>
    /// <param name="streamID">streamID that the data is coming from</param>
    /// <param name="msgByte">message, packed into bytes</param>
    /// <returns>UDPInitMessage - packaged array of bytes with a proper header
    /// to send to CoreLink</returns>
    byte[] constructUDPMsg(uint streamID, byte[] header, byte[] data)
    {
      byte[] dataPacket = new byte[8 + header.Length + data.Length];

      byte[] headerLengthBytes = BitConverter.GetBytes((ushort)header.Length);
      // TODO: Check to make sure this will return 2 bytes if size is 0
      dataPacket[0] = headerLengthBytes[0];
      dataPacket[1] = headerLengthBytes[1];

      byte[] dataLengthBytes = BitConverter.GetBytes((ushort)data.Length);
      dataPacket[2] = dataLengthBytes[0];
      dataPacket[3] = dataLengthBytes[1];

      // TODO: Not entirely sure why we cast to ushort
      byte[] streamIDBytes = BitConverter.GetBytes((ushort)streamID);
      dataPacket[4] = streamIDBytes[0];
      dataPacket[5] = streamIDBytes[1];

      // TODO: Hardcoding federation ID 0 for now
      dataPacket[6] = 0;
      dataPacket[7] = 0;

      if (header.Length > 0)
      {
        string headerStr = JsonUtility.ToJson(header);
        byte[] headerByte = Encoding.ASCII.GetBytes(headerStr);
        Buffer.BlockCopy(headerByte, 0, dataPacket, 8, headerByte.Length);
      }

      Buffer.BlockCopy(data, 0, dataPacket, 8 + header.Length, data.Length);

      return dataPacket;
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////////////////
    /// CLEANUP FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////
    public void stopConnection()
    {
      active = false;
    }
  }
}
