/// <summary>
/// UtilityStructs - Small structs/public classes used in the CoreLink code
/// </summary>

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// The Config class is filled once at the beginning of the unity session
  /// and contains important server information
  /// </summary>
  /// <param name="ControlIP">The server IP address to connect to. This is
  /// corelink.hpc.nyu.edu when running on the server or 127.0.0.1 if running
  /// Corelink locally.</param>
  /// <param name="ControlPort">Port on the server to authenticate and submit
  /// function requests. For WebSockets, this is 20012</param>
  public class Config
  {
    public string dataIP;
    public string controlDomain;
    public int controlPort;
    public Config(string DataIP)
    {
      this.controlDomain = DataIP;
      IPAddress ipAddress;
      if (!IPAddress.TryParse(DataIP, out ipAddress))
        ipAddress = Dns.GetHostEntry(DataIP).AddressList[0];
      this.dataIP = ipAddress.ToString();
    }
    public Config(string DataIP, int ControlPort)
    {
      this.dataIP = DataIP;
      this.controlPort = ControlPort;
    }
  }
  /// <summary>
  /// Contains user information to authenticate with Corelink.
  /// The username and password are sent to the server and authenticated, and then
  /// the server returns a token which must be used throughout the app's life to authenticate
  /// every function request
  /// </summary>
  public class Credentials
  {
    private string username, password, token;
    public Credentials(string username, string password)
    {
      this.username = username;
      this.password = password;
    }
    public string Token { get; set; }
    public JSONNode ToJSON()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "auth";
      jsonRequest["username"] = username;
      jsonRequest["password"] = password;
      return jsonRequest;
    }
  }
  /// <summary>
  /// When creating a receiver stream, the request must query what workspace and type of stream
  /// to receive from. This class is a wrapper to create a filter to create receiver streams.
  /// At least 1 workspace and 1 type are required. Streamids can be left blank
  /// </summary>
  public class StreamFilter
  {
    public List<string> workspaces;
    public List<string> types;
    public List<uint> streamIDs;

    public StreamFilter()
    {
      workspaces = new List<string>();
      types = new List<string>();
      streamIDs = new List<uint>();
    }
  }

  /// <summary>
  /// Header for a UDP packet to Corelink. Contains the streamID and timestamp in
  /// milliseconds since the Unix Epoch
  /// </summary>
  public class UDPHeader
  {
    public string ID;
    public long time;

  }

  /// <summary>
  /// Utility class for asynchronous data receiving
  /// </summary>
  class UdpState : System.Object
  {
    public UdpState(IPEndPoint e, UdpClient c) { this.e = e; this.c = c; }
    public UdpState(UdpClient c) { this.c = c; }
    public IPEndPoint e;
    public UdpClient c;
  }
}
