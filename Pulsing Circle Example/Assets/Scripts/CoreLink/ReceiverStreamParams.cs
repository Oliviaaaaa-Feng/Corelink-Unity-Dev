using System.Collections.Generic;
using SimpleJSON;

namespace CoreLink
{
  public class ReceiverStreamParams : StreamParams
  {
    /// <summary>
    /// ReceiverStreamParams class - Child class that inherits StreamParams.
    /// </summary>
    public List<uint> streamIDs;
    public bool echo;
    public uint receiverid;
    public new List<string> type;
    ReceiverStream receiverStream;

    //workspace, type, proto, metadata, alert, echo, receiverid, streamIDs, receiverStream
    public ReceiverStreamParams(string workspace, List<string> type, ReceiverStream receiverStream) :
      this(workspace, type, "udp", JSON.Parse("{}"), false, false, 0, new List<uint>(), receiverStream) { }
    public ReceiverStreamParams(string workspace, List<string> type, ReceiverStream receiverStream, bool alert) :
     this(workspace, type, "udp", JSON.Parse("{}"), alert, false, 0, new List<uint>(), receiverStream)
    { }
    public ReceiverStreamParams(string workspace, List<string> type, ReceiverStream receiverStream, bool alert, bool echo) :
      this(workspace, type, "udp", JSON.Parse("{}"), alert, echo, 0, new List<uint>(), receiverStream) { }
    public ReceiverStreamParams(string workspace, List<string> type, string proto, ReceiverStream receiverStream) :
      this(workspace, type, proto, JSON.Parse("{}"), false, false, 0, new List<uint>(), receiverStream) { }

    public ReceiverStreamParams(string workspace, List<string> type, string proto,
      JSONNode metadata, bool alert, bool echo, uint receiverid,
      List<uint> streamIDs, ReceiverStream receiverStream) : base(workspace, type[0], proto, metadata, alert)
    {
      this.type = type;
      this.echo = echo;
      this.receiverid = receiverid;
      this.streamIDs = streamIDs;
      this.receiverStream = receiverStream;
    }

    public ReceiverStream ReceiverStream { set => receiverStream = value;  get => receiverStream; }

    /// <summary>
    /// Parse to server function. IP, port, and token gets filled in by CoreLink.Control
    /// </summary>
    /// <returns>jsonRequest will all of the existing data filled in</returns>
    override public JSONNode toJSON()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "receiver";
      jsonRequest["workspace"] = workspace;
      if (receiverid != 0) jsonRequest["receiverid"] = receiverid;
      for (int i = 0; i < streamIDs.Count; i++)
      {
        jsonRequest["streamIDs"][i] = streamIDs[i];
      }
      jsonRequest["proto"] = proto;
      for (int i = 0; i < type.Count; i++)
      {
        jsonRequest["type"][i] = type[i];
      }
      jsonRequest["alert"] = alert;
      jsonRequest["echo"] = echo;

      jsonRequest["meta"] = metadata;
      return jsonRequest;
    }
  }
}
