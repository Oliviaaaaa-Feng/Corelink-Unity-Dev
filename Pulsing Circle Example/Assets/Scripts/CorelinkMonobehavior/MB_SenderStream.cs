using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Parent class to custom sender scripts that will be attached to GameObjects. 
  /// Several of these can be instantiated. Use Send(streamID, data) to send data
  /// </summary>
  public class SenderStream : MonoBehaviour
  {
    public string workspace = "Holodeck";
    public string type = "unity";
    protected uint streamID;
    protected Control control;
    virtual public void Initialize(ref Control control)
    {
      this.control = control;
      if (string.IsNullOrEmpty(this.workspace)) this.workspace = "Holodeck";
      if (string.IsNullOrEmpty(this.type)) this.type = "unity";
      streamID = control.createSender(new SenderStreamParams("Holodeck", "unity"));
    }
  }
}
