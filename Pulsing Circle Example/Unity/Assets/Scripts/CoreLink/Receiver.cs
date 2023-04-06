using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using CoreLink;
using SimpleJSON;

public class Receiver : ReceiverStream
{
    private Vector3 position;

    /// <summary>
    /// This function is specifically overriden to turn echo on so we can receive our own streams.
    /// This function can be excluded if you want to use all default parameters
    /// </summary>
    /// <param name="control">singleton control object</param>
    override public void Initialize(ref Control control, List<uint> streamIDs)
    {
        Debug.Log("test");
        this.control = control;
        this.type.Add("distance");
        this.streamIDs = control.createReceiver(new ReceiverStreamParams("Holodeck", this.type, this, true, true));
        this.streamIDs = control.subscribe(this.streamIDs);
    }
    private int valueInt;
    
    // Update is called once per frame
    void Update()
    {   
        Vector3 newScale = Vector3.one * valueInt;
        transform.localScale = newScale;

    }
    
    override public void OnMessage(uint streamID, byte[] header, byte[] message)
    {
        if (streamIDs.Contains(streamID))
        {
            string str = System.Text.Encoding.UTF8.GetString(message);
            valueInt = int.Parse(str);
            Debug.Log(valueInt);
        }
    }
}
