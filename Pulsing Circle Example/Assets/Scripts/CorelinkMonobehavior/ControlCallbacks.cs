using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

namespace CoreLink
{
    public class ControlCallbacks : CorelinkWebSocket
    {
        public ControlCallbacks(string serverURL) : base(serverURL) {}
        override public void onUpdate(string msg)
        {
            Debug.Log("This is an overriden OnUpdate Message " + msg);
        }
        override public void onSubscriber(string msg)
        {
            Debug.Log("This is an overriden OnSubscriber Message " + msg);
        }
        override public void onStale(string msg)
        {

        }
        override public void onDropped(string msg)
        {

        }
    }
}
