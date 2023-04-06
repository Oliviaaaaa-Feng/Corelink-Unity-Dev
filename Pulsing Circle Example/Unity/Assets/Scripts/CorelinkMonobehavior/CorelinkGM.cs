using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLink;

public class CorelinkGM : MonoBehaviour
{

  public string username = "Testuser", password = "Testpassword", serverName = "corelink.hpc.nyu.edu";
  public List<GameObject> senderObjects;
  public List<GameObject> receiverObjects;
  [SerializeField]
  private bool debug = true;


  public static Control control;

  
  // Start is called before the first frame update
  /// <summary>
  /// Log into CoreLink with the username/password/server address given in the Inspector tab
  /// </summary>
  void Start()
  {
    Credentials credentials = new Credentials(username, password);
    Config config = new Config(serverName);
    control = new Control(debug);
    control.connect(config);
    bool status = control.login(credentials);

    if (!status)
    {
      Debug.LogError("ERROR: Authentication failed");
    }

    foreach (GameObject g in senderObjects)
    {
      g.GetComponent<SenderStream>().Initialize(ref control);
    }

    foreach (GameObject g in receiverObjects)
    {
      g.GetComponent<ReceiverStream>().Initialize(ref control);
    }
  }

  public void startOffline(string username, string password, string serverName)
  {
    Credentials credentials = new Credentials(username, password);
    Config config = new Config(serverName);
    control = new Control(debug);
    control.connect(config);
    bool status = control.login(credentials);

    if (!status)
    {
      Debug.LogError("ERROR: Authentication failed");
    }

    foreach (GameObject g in senderObjects)
    {
      g.GetComponent<SenderStream>().Initialize(ref control);
    }

    foreach (GameObject g in receiverObjects)
    {
      g.GetComponent<ReceiverStream>().Initialize(ref control);
    }
  }
  // Update is called once per frame
  void Update()
  {

  }
  private void OnDestroy()
  {
    control.exit();
  }
}
