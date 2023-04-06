using UnityEditor;
using UnityEngine;

namespace CoreLink
{
  /// <summary>
  /// Abstract class for a control stream that contains the function prototypes for the
  /// server callback functions. Inherited by CorelinkTCP and CorelinkWebSocket.
  /// CorelinkWebSocket must have a child class called ControlCallBacks that can
  /// override these server callback functions
  /// </summary>
  abstract public class ControlStream
  {
    virtual public void onUpdate(string msg)
    {
      Debug.Log("OnUpdate called " + msg);
    }
    virtual public void onSubscriber(string msg)
    {
      Debug.Log("OnSubscriber called " + msg);
    }
    virtual public void onStale(string msg)
    {
      Debug.Log("OnStale called " + msg);
    }
    virtual public void onDropped(string msg)
    {
      Debug.Log("OnDropped called " + msg);
    }
    abstract public void sendMessage(string request);
    abstract public void stopConnection();
  }
}
