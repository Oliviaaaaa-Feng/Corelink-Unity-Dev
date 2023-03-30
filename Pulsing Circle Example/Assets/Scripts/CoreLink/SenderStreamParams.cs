using SimpleJSON;
namespace CoreLink
{
  /// <summary>
  /// Contains all the necessary parameters for sender streams except IP and port.
  /// Inherits from StreamParams
  /// </summary>
  public class SenderStreamParams : StreamParams
  {
    public string senderID;
    public string from; // TODO: What is this?
    SenderStream stream;

    public SenderStreamParams(string workspace, string type) : this(workspace, type, "udp", JSON.Parse("{}"), false, "", "") { }
    public SenderStreamParams(string workspace, string type, string proto) : this(workspace, type, proto, JSON.Parse("{}"), false, "", "") { }

    public SenderStreamParams(string workspace, string type, string proto, JSONNode metadata, bool alert, string senderID, string from) : base(workspace, type, proto, metadata, alert)
    {
      this.senderID = senderID;
      this.from = from;
    }
    /// <summary>
    /// Parse to server function. IP, port, and token gets filled in by CoreLink.Control
    /// </summary>
    /// <returns>jsonRequest will all of the existing data filled in</returns>
    override public JSONNode toJSON()
    {
      JSONNode jsonRequest = JSON.Parse("{}");
      jsonRequest["function"] = "sender";
      jsonRequest["workspace"] = this.workspace;
      jsonRequest["senderID"] = this.senderID;
      jsonRequest["from"] = this.from;
      jsonRequest["proto"] = this.proto;

      jsonRequest["type"] = this.type;
      jsonRequest["alert"] = this.alert;
      jsonRequest["meta"] = this.metadata;
      return jsonRequest;
    }
  }
}
