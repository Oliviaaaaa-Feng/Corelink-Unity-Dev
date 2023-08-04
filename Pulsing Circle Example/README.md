# Corelink Unity Dev



## Unity Pulsing Circle Example

This is a simple example of sending real-time data using node.js sender and receiving these data using Unity and applying it on a GameObject.

## Author: Olivia Feng

## Goals:
- Use Javascript sender to send real-time data
- Use C# receiver to receive the data in Unity
- Apply data on a sphere GameObject to make it pulse

## Steps
- Download the Unity package and open in Unity
- Run simple_udp_sender_pulsing.js
- Click on play in Unity

## simple_udp_sender_pulsing
Installation

1) run `npm install` inside the folder
2) make sure you set the config, username and password inside the example script
3) run `node simple_udp_sender_pulsing.js`

You can use username: `Testuser`, password: `Testpassword`, and Control IP: `corelink.hpc.nyu.edu`. After running this .js file, you should receive real-time data from 1 to 10 than back to 1.

## Receiver.cs

This is a script attached to the sphere GameObject that inherites from ReceiverStream.cs. It will receive the real-time data from simple_udp_sender_pulsing.js and convert the data type using `OnMessage()`. Then, apply it on the sphere using `Update()`.

After click on play, the console log printout should look like this:
```
Initializing WebSocket Connection
WebSocket Message Receiver looping.
WebSocket Message Sender looping.
Connecting to wss://corelink.hpc.nyu.edu:20012/
Waiting to connect...
Connect status: Open
Request     {"function":"auth","username":"Testuser","Testpassword":"Testpassword","ID":0}
Response: {"statusCode":0,"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","IP":"68.161.197.183","ID":0}
Request:	{"function":"receiver","workspace":"Holodeck","proto":"udp","type":["distance"],"alert":true,"echo":true,"meta":{},"IP":"68.161.197.183","port":0,"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","ID":1}
Response: {"statusCode":0,"port":20011,"proto":"udp","streamID":1433,"streamList":[{"streamID":27789,"type":"distance","meta":{"name":"Random Data"},"user":"Testuser","apps":[]}],"MTU":20000,"ID":1}
Attempting to open a UDP port at 20011
Opening UDP Connection216.165.12.41:20011
Sent empty receiver packet
Request:	{"function":"subscribe","receiverID":1433,"streamID":[27789],"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","ID":2}
Response: {"statusCode":0,"streamList":[{"streamID":27789,"type":"distance","meta":{"name":"Random Data"},"user":"Testuser","apps":[]}],"ID":2}
1
2
3
4
.
.
.
10
9
8
.
.
.
1
```

When stop palying, the console log printout should look like this:
```
Request:	{"function":"disconnect","workspaces":[],"types":[],"streamIDs":[1433,1433],"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","ID":3}
Response: {"statusCode":0,"streamList":[1433,1433],"ID":3}
Waiting to disconnect...
Connect status: CloseReceived
Closing UDP Connection
```
