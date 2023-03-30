/**
 * @file StreamParams - Parent class of stream parameters with
 * member variables shared between sender streams and receiver streams
 * @author Cindy Bui
 * @version V1.0.0.0
 */

using System.Collections.Generic;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Parent class of Receiver/SenderStreamParams with shared
  /// information
  /// </summary>
  public class StreamParams
  {

    public string workspace;
    public string proto;
    public int port;
    public string type;
    public bool alert;
    public JSONNode metadata;

    // This is only slightly different from senderID/receiverid.
    // Senderid/receiverid are sent as a JSON response to the server.
    // The server then replies with the actual stream id, which gets placed here
    public uint streamID;
    public StreamParams(string workspace, string type) : this(workspace, type, "udp", JSON.Parse("{}"), false) { }
    public StreamParams(string workspace, string type, string proto) : this(workspace, type, proto, JSON.Parse("{}"), false) { }

    public StreamParams(string workspace, string type, string proto, JSONNode metadata, bool alert)
    {
      this.workspace = workspace;
      this.type = type;
      this.proto = proto;
      this.metadata = metadata;
      this.alert = alert;
    }

    // This function should never be called since it's overriden, but it's here just in case
    virtual public JSONNode toJSON()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "";
      jsonRequest["workspace"] = this.workspace;
      jsonRequest["proto"] = this.proto;
      jsonRequest["meta"] = this.metadata;
      jsonRequest["alert"] = this.alert;
      jsonRequest["type"] = this.type;
      return jsonRequest;
    }

  }
}
