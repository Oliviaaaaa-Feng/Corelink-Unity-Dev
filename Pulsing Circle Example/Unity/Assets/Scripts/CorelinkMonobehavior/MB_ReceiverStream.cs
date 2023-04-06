using System.Collections.Generic;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Parent class to custom receiver scripts that will be attached to GameObjects. 
  /// Several of these can be instantiated, and each of them will have their own
  /// sender stream. OnMessage(streamID, data) is called whenever there is data
  /// See the README for caveats on multiple receiver streams
  /// </summary>
  
  public class ReceiverStream : MonoBehaviour
  {
    public string workspace = "Holodeck";
    public List<string> type;
    protected List<uint> streamIDs = new List<uint>();
    protected Control control;
    protected uint streamID;
    virtual public void Initialize(ref Control control, List<uint> streamIDs = null)
    {
      this.control = control;
      this.type.Add("udp");
      this.streamIDs = control.createReceiver(new ReceiverStreamParams("Holodeck", this.type, this, true));
      this.streamIDs = control.subscribe(this.streamIDs);
    }

    virtual public void OnMessage(uint streamID, byte[] header, byte[] message)
    {
      Debug.Log("'You abandoned me!' - " + message);
    }
  }
}
