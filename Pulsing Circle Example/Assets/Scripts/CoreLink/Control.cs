using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SimpleJSON;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Singleton class to communicate with Corelink.
  /// 
  /// This file emulates clients/javascript/corelink.lib.js and is
  /// designed to have the same functionality as the NodeJS library.
  /// Everything in this C# client library will be in the CoreLink namespace
  /// </summary>

  public class Control
  {
    /// <summary>
    /// debug turns on or off verbose logging. Control.Print() statements will only print if debug mode is on
    /// </summary>
    public static bool debug;
    /// <summary>
    /// In some callbacks, the code is waiting on a response from the server. This specifies the number of 100 ms iterations to wait before throwing an error
    /// </summary>
    public static int timeoutIterations = 100;

    private Credentials credentials;
    private Config config;

    private CorelinkTCP tcp;
    private ControlCallbacks ws;
    private CorelinkUDP udp;

    private IPAddress myIP;

    private uint receiverID;
    private List<SenderStreamParams> senderStreams;
    private List<ReceiverStreamParams> receiverStreams;
    private List<uint> allowedStreams;

    int id = 0;
    public Control() : this(false) { }
    public Control(bool debug)
    {
      senderStreams = new List<SenderStreamParams>();
      receiverStreams = new List<ReceiverStreamParams>();
      allowedStreams = new List<uint>();
      Control.debug = debug;
      receiverID = 0;
    }
    #region [Basic Usage]

    /// <summary>
    /// Connects with client at the specified Port and IP number.
    /// Throws an error if something goes wrong
    /// </summary>
    /// <param name="config">Refers to connection configuration like ControlPort and ControlIP</param>
    public void connect(Config config)
    {
      this.config = config;

      //tcp = new CorelinkTCP(config);

      Print("Initializing WebSocket Connection");
      ws = new ControlCallbacks(config.controlDomain);
      ConnectToServer();
      int iterations = 0;
      while (!ws.isConnectionOpen() && iterations < timeoutIterations)
      {
        Thread.Sleep(100);
        iterations++;
      }
      if (iterations == timeoutIterations)
      {
        throw new Exception("Timeout error");
      }
    }
    public async void ConnectToServer()
    {
      await ws.connect();
    }

    /// <summary>
    /// Logs into the server after connecting after verifying login credentials
    /// like username,password and token defined in the 'login' function.
    /// </summary>
    /// <param name="credentials">Contains username and password, writes to token</param>
    /// <returns>true if successful, false if not</returns>
    public bool login(Credentials credentials)
    {
      this.credentials = credentials;
      JSONNode jsonRequest = credentials.ToJSON();
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      this.credentials.Token = jsonResponse["token"].Value;
      myIP = IPAddress.Parse(jsonResponse["IP"].Value).MapToIPv4(); // client's public facing IP address (usually their router)
      int statusCode = jsonResponse["statusCode"].AsInt;
      string message = jsonResponse["message"].Value; // unused for now

      if (statusCode != 0) throw new CorelinkException(statusCode, message);

      return statusCode == 0 ? true : false;
    }

    /// <summary>
    /// Asks Corelink to create a sender stream. At minimum, this requires a workspace name and a type.
    /// The stream parameters are: workspace, metadata, type, senderID
    /// The stream also requires networking information to know where to send the data: 
    /// protocol type (UDP, TCP, WS), source IP (Set to your computer/router's IP address)
    /// source port (Your computer's port number. Set to 0 if you don't know this or have a firewall)
    ///
    /// Sender streams can only have one workspace and one type.
    /// Current workspaces are Holodeck, Chalktalk, Infinit 
    /// They must be created ahead of time
    /// Example types are:
    /// unity, 3d, iclone, audio, messagebus, ... etc
    /// These are user defined and do not need to be created ahead of time. They can be any string as long as the receiver
    /// is matching the same string. If an existing sender streamID already exists, it can be updated by passing that
    /// streamID into the 'senderID' parameter.
    /// 
    /// The request also requires a token, created on login
    /// An optional id is included in the request which the server will relay back in the response
    /// to make sure that the requests and responses are paired properly
    /// </summary>
    /// <param name="streamParams">SenderStreamParams with all pertinent information</param>
    /// <returns>sender streamID</returns>

    public uint createSender(SenderStreamParams streamParams)
    {
      JSONNode jsonRequest = streamParams.toJSON();
      jsonRequest["function"] = "sender";
      jsonRequest["IP"] = myIP.ToString();
      jsonRequest["port"] = 0;
      jsonRequest["token"] = this.credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      int statusCode = jsonResponse["statusCode"].AsInt;
      uint.TryParse(jsonResponse["streamID"].Value, out streamParams.streamID);
      int udpPort = jsonResponse["port"].AsInt;
      string MTU = jsonResponse["MTU"].Value; // unused for now
      string message = jsonResponse["message"].Value; // unused for now

      if (statusCode != 0) throw new CorelinkException(statusCode, message);

      if (streamParams.proto.Equals("udp"))
      {
        if (udp == null)
        {
          Print("Attempting to open a UDP port at " + jsonResponse["port"].Value);
          udp = new CorelinkUDP(config.dataIP, udpPort, ref allowedStreams, ref receiverStreams);
        }

      }
      streamParams.port = udpPort;
      senderStreams.Add(streamParams);
      return streamParams.streamID;
    }

    /// <summary>
    /// Asks Corelink to create a receiver stream. At minimum, this requires a workspace name and a list of types.
    /// The parameters are the same as sender streams except that types is a list. It also has an echo parameter
    /// which is by default false. It has a 'receiverid' parameter in case there is pre-existing receiver
    /// stream that needs to be updated.
    /// The request also requires a token, created on login
    /// An optional id is included in the request which the server will relay back in the response
    /// to make sure that the requests and responses are paired properly
    /// </summary>
    /// <param name="streamParams"></param>
    /// <returns>list of streamIDs to subscribe to</returns>
    public List<uint> createReceiver(ReceiverStreamParams streamParams)
    {
      if (receiverID != null)
      {
        // Update our current receiverID, not generate a new one
        streamParams.receiverid = receiverID;
      }
      JSONNode jsonRequest = streamParams.toJSON();
      jsonRequest["IP"] = myIP.ToString();
      jsonRequest["port"] = 0;
      jsonRequest["token"] = this.credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      uint.TryParse(jsonResponse["streamID"].Value, out streamParams.streamID);
      if (receiverID == 0)
      {
        receiverID = streamParams.streamID;
      }
      foreach (JSONNode node in jsonResponse["streamList"])
      {
        uint streamID = 0;
        uint.TryParse(node["streamID"].Value, out streamID);
        if (streamID == 0) throw new CorelinkException("Error parsing streamID");
        streamParams.streamIDs.Add(streamID);
        if (!allowedStreams.Contains(streamID))
        {
          allowedStreams.Add(streamID);
        }
      }

      streamParams.port = jsonResponse["port"].AsInt;
      string proto = jsonResponse["proto"].Value;
      string MTU = jsonResponse["MTU"].Value; // unused for now

      if (proto.Equals("udp"))
      {
        if (udp == null)
        {
          Print("Attempting to open a UDP port at " + jsonResponse["port"].Value);
          udp = new CorelinkUDP(config.dataIP, streamParams.port, ref allowedStreams, ref receiverStreams);
        }
        udp.receiverPing(streamParams.streamID);
      }

      receiverStreams.Add(streamParams);
      return streamParams.streamIDs;
    }

    /// <summary>
    /// Tells Corelink to subscribe a certain receiver stream to one or more sender streams
    /// </summary>
    /// <param name="streamIDs">Sender streams to subscribe to</param>
    /// <returns>List of sender streamIDs that have been subscribed to</returns>
    public List<uint> subscribe(List<uint> streamIDs)
    {
      if (receiverID == null)
      {
        throw new CorelinkException("ERROR: Need to create a receiver stream before subscribing");
      }
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "subscribe";
      jsonRequest["receiverID"] = receiverID;
      for (int i = 0; i < streamIDs.Count; i++)
      {
        jsonRequest["streamID"][i] = streamIDs[i];
      }
      jsonRequest["token"] = this.credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);

      List<uint> returnedStreamids = new List<uint>();
      for (int i = 0; i < jsonResponse["streamList"].Count; i++)
      {
        uint streamID = 0;
        uint.TryParse(jsonResponse["streamList"][i]["streamID"].Value, out streamID);
        if (streamID == 0) throw new CorelinkException("Error parsing streamID");
        returnedStreamids.Add(streamID);
        allowedStreams.Add(streamID);
      }

      return returnedStreamids;
    }

    /// <summary>
    /// Unsubscribes the program from various sender streams
    /// </summary>
    /// <param name="filter">Sender streams ids to unsubscribe from</param>
    /// <returns>List of stream ids successfully unsubscribed from</returns>
    public List<uint> unsubscribe(List<string> streamIDs)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "unsubscribe";
      jsonRequest["receiverID"] = receiverStreams[0].streamID;
      jsonRequest["streamID"] = new JSONArray();
      foreach (string w in streamIDs)
      {
        jsonRequest["workspaces"].Add(w);
      }
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<uint> retval = new List<uint>();
      foreach (JSONNode f in jsonResponse["streamList"].AsArray)
      {
        uint ID = 0;
        uint.TryParse(f.Value, out ID);
        if (ID == 0) throw new CorelinkException("Error parsing streamID");
        retval.Add(ID);
      }
      return retval;
    }

    /// <summary>
    /// Disconnects streams of a specific stream filter type, or all current streams if
    /// no parameters are given.
    /// </summary>
    /// <returns>Array of disconnected streamIDs</returns>
    public List<string> disconnect()
    {
      StreamFilter filter = new StreamFilter();
      if (receiverStreams.Count > 0)
      {
        filter.streamIDs.Add(receiverStreams[0].streamID);
      }
      foreach (SenderStreamParams param in senderStreams)
      {
        filter.streamIDs.Add(param.streamID);
      }
      return disconnect(filter);
    }
    public List<string> disconnect(StreamFilter filter)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "disconnect";
      jsonRequest["workspaces"] = new JSONArray();
      foreach (string w in filter.workspaces)
      {
        jsonRequest["workspaces"].Add(w);
      }
      jsonRequest["types"] = new JSONArray();
      foreach (string t in filter.types)
      {
        jsonRequest["types"].Add(t);
      }
      jsonRequest["streamIDs"] = new JSONArray();
      foreach (uint s in filter.streamIDs)
      {
        jsonRequest["streamIDs"].Add(s);
      }
      if (receiverID != null)
      {
        jsonRequest["streamIDs"].Add(receiverID);
      }
      jsonRequest["token"] = this.credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<string> retval = new List<string>();
      foreach (JSONNode f in jsonResponse["streamList"].AsArray)
      {
        retval.Add(f.Value);
      }
      return retval;
    }

    /// <summary>
    /// Disconnects all available streamIDs and stops the WebSocket/UDP Sockets
    /// </summary>
    public void exit()
    {
      // TODO: Check if all streamIDs are fetched successfully
      disconnect();
      ws.stopConnection();
      if (udp != null) udp.stopConnection();
    }

    /// <summary>
    /// Sends a UDP message to Corelink for a specific sender stream id
    /// </summary>
    /// <param name="streamID">Sender stream ID this data belongs to</param>
    /// <param name="header">User custom header information</param>
    /// <param name="data">Data to send to Corelink</param>
    public void send(uint streamID, byte[] header, byte[] data)
    {
      if (udp == null)
      {
        throw new Exception("udp connection not created");
      }
      udp.send(streamID, header, data);
    }

    #endregion

    #region [Workspace Functions]
    /// <summary>
    /// Asks the server to create a new workspace
    /// </summary>
    /// <param name="workspace">Name of new workspace</param>
    /// <returns>true if successful, false if not</returns>
    public bool addWorkspace(string workspace)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "addWorkspace";
      jsonRequest["workspace"] = workspace;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      int statusCode = jsonResponse["statusCode"].AsInt;
      return statusCode == 0 ? true : false;
    }

    /// <summary>
    /// Asks the server to remove a workspace
    /// </summary>
    /// <param name="workspace">Name of workspace to be deleted</param>
    /// <returns>True if successful, false if not</returns>
    public bool rmWorkspace(string workspace)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "rmWorkspace";
      jsonRequest["workspace"] = workspace;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      int statusCode = jsonResponse["statusCode"].AsInt;
      return statusCode == 0 ? true : false;
    }

    /// <summary>
    /// Requests Corelink to set the default workspace
    /// </summary>
    /// <param name="workspace">workspace name</param>
    /// <returns>true if successful, false if not</returns>
    public bool setDefaultWorkspace(string workspace)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "setDefaultWorkspace";
      jsonRequest["workspace"] = workspace;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      int statusCode = jsonResponse["statusCode"].AsInt;
      return statusCode == 0 ? true : false;
    }

    /// <summary>
    /// Asks Corelink for the current default workspace
    /// </summary>
    /// <returns>workspace name</returns>
    public string getDefaultWorkspace()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "getDefaultWorkspace";
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      return jsonResponse["workspace"].Value;
    }

    /// <summary>
    /// Asks corelink for all available workspaces
    /// </summary>
    /// <returns>List<string> of all available workspaces</returns>
    public List<string> listWorkspaces()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "listWorkspaces";
      jsonRequest["token"] = this.credentials.Token;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<string> retval = new List<string>();
      foreach (JSONNode f in jsonResponse["workspaceList"].AsArray)
      {
        retval.Add(f.Value);
      }
      return retval;
    }

    #endregion

    #region [Function Helpers]

    /// <summary>
    /// Asks the server for all available client functions
    /// </summary>
    /// <returns>Array of available functions</returns>
    public List<string> listFunctions()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "listFunctions";
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<string> retval = new List<string>();
      foreach (JSONNode f in jsonResponse["functionList"].AsArray)
      {
        retval.Add(f.Value);
      }
      return retval;
    }

    /// <summary>
    /// Asks the server for all server callback functions.
    /// Currently there is Update, Subscriber, OnStale, and Dropped
    /// </summary>
    /// <returns>Array of server callback functions</returns>
    public List<string> listServerFunctions()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "listServerFunctions";
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<string> retval = new List<string>();
      foreach (JSONNode f in jsonResponse["functionList"].AsArray)
      {
        retval.Add(f.Value);
      }
      return retval;
    }

    /// <summary>
    /// Asks the server for a specific client side function's description
    /// </summary>
    /// <param name="function">function name for query</param>
    /// <returns>description</returns>
    public string describeFunction(string function)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "describeFunction";
      jsonRequest["functionName"] = function;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      return jsonResponse["description"].Value;

    }

    /// <summary>
    /// Asks the server for a specific server callback function's description
    /// </summary>
    /// <param name="function">function name for query</param>
    /// <returns>description</returns>
    public string describeServerFunction(string function)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "describeServerFunction";
      jsonRequest["functionName"] = function;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      return jsonResponse["description"].Value;
    }

    #endregion

    #region [Stream Info]

    /// <summary>
    /// Asks the server for a list all streams
    /// </summary>
    /// <param name="filter">Optional filter by workspace name and types</param>
    /// <returns>List of available streams</returns>
    public JSONArray listStreams(StreamFilter filter)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "listStreams";
      jsonRequest["workspaces"] = new JSONArray();
      foreach (string w in filter.workspaces)
      {
        jsonRequest["workspaces"].Add(w);
      }
      jsonRequest["types"] = new JSONArray();
      foreach (string t in filter.types)
      {
        jsonRequest["types"].Add(t);
      }
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      return jsonResponse["senderList"].AsArray;
    }
    /// <summary>
    /// Asks the server for information about a sender stream
    /// </summary>
    /// <param name="streamID">sender's streamID</param>
    /// <returns>JSONNode of all the information</returns>
    public JSONNode streamInfo(uint streamID)
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "streamInfo";
      jsonRequest["workspaces"] = new JSONArray();
      jsonRequest["streamID"] = streamID;
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      return jsonResponse;

    }

    #endregion

    #region [User Functions]
    public List<string> listUsers()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "streamInfo";
      jsonRequest["token"] = credentials.Token;
      jsonRequest["ID"] = id;
      string request = jsonRequest.ToString();
      JSONNode jsonResponse = SendAndErrorCheck(request, id++);
      List<string> retval = new List<string>();
      foreach (JSONNode f in jsonResponse["userList"].AsArray)
      {
        retval.Add(f.Value);
      }
      return retval;
    }

    #endregion
    // TODO: keepAlive, password, rmUser, listUsers, addGroup, addUserGroup, rmUserGroup, changeOwner, rmGroup, listGroups, setConfig, expire, 


    #region [Utility]
    ///////////////////////////////////////////////////////////////////////////////
    /// UTILITY FUNCTIONS
    ///////////////////////////////////////////////////////////////////////////////
    JSONNode SendAndErrorCheck(string request, int id)
    {
      Print("Request:\t" + request);
      ws.sendMessage(request);
      string response = ws.getResponse();

      Print("Response: " + response);
      JSONNode jsonResponse = JSON.Parse(response);
      int statusCode = jsonResponse["statusCode"].AsInt;
      int receivedID = jsonResponse["ID"].AsInt;
      string message = jsonResponse["message"].Value;
      if (receivedID != id)
      {
        throw new CorelinkException("Wrong ID");
      }
      if (statusCode != 0) throw new CorelinkException(statusCode, message);
      return jsonResponse;
    }
    public static void Print(string message)
    {
      if (debug) Debug.Log(message);
    }
    #endregion
  }
}
