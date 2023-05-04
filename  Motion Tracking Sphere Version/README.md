# Corelink Unity Dev



## Unity Pulsing Circle Example

This is an example of receiving real-time motion tracking data from Motive and applying it to 21 spheres in Unity. Each sphere represents a body part.

## Author: Olivia Feng

## Goals:
- Use sampleClient.cpp to transfer and send real-time motion tracking data
- Use C# receiver to receive the data in Unity
- Split the data and apply on separate gameObject

## Receiver.cs

This is a script attached to the GameObject that inherites from ReceiverStream.cs. It will generate 21 sphere at the begining. Then, It will receive the real-time data from sampleClient.cpp and convert the data type using `OnMessage()`. Then, apply it on the sphere using `Update()`.

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

Received data: -0.431733 0.874897 -1.115012 -0.436535 0.944178 -1.098866 -0.448284 1.056828 -1.087505 -0.435808 1.205686 -1.143918 -0.508206 1.305639 -1.193446 -0.427491 1.169148 -1.094281 -0.389178 1.261202 -0.971903 -0.358317 1.271564 -0.724329 -0.307381 1.381147 -0.573119 -0.435945 1.145347 -1.158953 -0.485365 1.118221 -1.306399 -0.588452 1.007333 -1.504967 -0.672316 0.941816 -1.669193 -0.440694 0.854679 -1.030921 -0.462632 0.413419 -1.076108 -0.429504 0.108554 -1.138782 -0.422773 0.895115 -1.199103 -0.378092 0.577240 -1.506011 -0.336835 0.368288 -1.735369 -0.523976 0.023756 -1.074834 -0.402302 0.341926 -1.858755 

```
Each three data points represent the position of a sphere. There are 21 sphere in total.

When stop palying, the console log printout should look like this:
```
Request:	{"function":"disconnect","workspaces":[],"types":[],"streamIDs":[1433,1433],"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","ID":3}
Response: {"statusCode":0,"streamList":[1433,1433],"ID":3}
Waiting to disconnect...
Connect status: CloseReceived
Closing UDP Connection
```
