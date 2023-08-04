# Corelink Unity Dev



## Unity Pulsing Circle Example

This is an example of receiving real-time motion tracking data from Motive and applying it to a skeleton in Unity.
## Author: Olivia Feng

## Goals:
- Use sampleClient.cpp to transfer and send real-time motion tracking data
- Use C# receiver to receive the data in Unity
- Split the data and apply on separate bones

## Steps
- Download the SphereVersion.unitypackage and open in Unity
- Run Motive (a motion capture software) on a machine
- Run sampleClient.cpp on that machine to decode and transfer data
- Click on play in Unity

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
Each three data points represent the position of a bone. There are 21 bones in total.
The order of the bones:
```
RigidBody (Bone) Count : 21
  RigidBody Name : Skeleton 02_Hip
  RigidBody ID : 1
  RigidBody Parent ID : 0
  Parent Offset : -0.00,0.80,0.00
  RigidBody Name : Skeleton 02_Ab
  RigidBody ID : 2
  RigidBody Parent ID : 1
  Parent Offset : -0.00,0.07,0.00
  RigidBody Name : Skeleton 02_Chest
  RigidBody ID : 3
  RigidBody Parent ID : 2
  Parent Offset : -0.00,0.14,0.00
  RigidBody Name : Skeleton 02_Neck
  RigidBody ID : 4
  RigidBody Parent ID : 3
  Parent Offset : -0.00,0.16,0.00
  RigidBody Name : Skeleton 02_Head
  RigidBody ID : 5
  RigidBody Parent ID : 4
  Parent Offset : -0.00,0.13,0.02
  RigidBody Name : Skeleton 02_LShoulder
  RigidBody ID : 6
  RigidBody Parent ID : 3
  Parent Offset : 0.03,0.11,-0.01
  RigidBody Name : Skeleton 02_LUArm
  RigidBody ID : 7
  RigidBody Parent ID : 6
  Parent Offset : 0.16,0.00,0.00
  RigidBody Name : Skeleton 02_LFArm
  RigidBody ID : 8
  RigidBody Parent ID : 7
  Parent Offset : 0.25,0.00,0.00
  RigidBody Name : Skeleton 02_LHand
  RigidBody ID : 9
  RigidBody Parent ID : 8
  Parent Offset : 0.19,0.00,0.00
  RigidBody Name : Skeleton 02_RShoulder
  RigidBody ID : 10
  RigidBody Parent ID : 3
  Parent Offset : -0.03,0.11,-0.01
  RigidBody Name : Skeleton 02_RUArm
  RigidBody ID : 11
  RigidBody Parent ID : 10
  Parent Offset : -0.16,0.00,0.00
  RigidBody Name : Skeleton 02_RFArm
  RigidBody ID : 12
  RigidBody Parent ID : 11
  Parent Offset : -0.25,0.00,0.00
  RigidBody Name : Skeleton 02_RHand
  RigidBody ID : 13
  RigidBody Parent ID : 12
  Parent Offset : -0.19,0.00,0.00
  RigidBody Name : Skeleton 02_LThigh
  RigidBody ID : 14
  RigidBody Parent ID : 1
  Parent Offset : 0.09,0.00,0.00
  RigidBody Name : Skeleton 02_LShin
  RigidBody ID : 15
  RigidBody Parent ID : 14
  Parent Offset : -0.00,-0.44,0.00
  RigidBody Name : Skeleton 02_LFoot
  RigidBody ID : 16
  RigidBody Parent ID : 15
  Parent Offset : -0.00,-0.31,0.00
  RigidBody Name : Skeleton 02_RThigh
  RigidBody ID : 18
  RigidBody Parent ID : 1
  Parent Offset : -0.09,0.00,0.00
  RigidBody Name : Skeleton 02_RShin
  RigidBody ID : 19
  RigidBody Parent ID : 18
  Parent Offset : -0.00,-0.44,0.00
  RigidBody Name : Skeleton 02_RFoot
  RigidBody ID : 20
  RigidBody Parent ID : 19
  Parent Offset : -0.00,-0.31,0.00
  RigidBody Name : Skeleton 02_LToe
  RigidBody ID : 17
  RigidBody Parent ID : 16
  Parent Offset : -0.00,-0.06,0.13
  RigidBody Name : Skeleton 02_RToe
  RigidBody ID : 21
  RigidBody Parent ID : 20
  Parent Offset : -0.00,-0.06,0.13
```


When stop palying, the console log printout should look like this:
```
Request:	{"function":"disconnect","workspaces":[],"types":[],"streamIDs":[1433,1433],"token":"4bb552a666f54466dae4e0dbf629d6986523707fc2221ea531225bcf815c5b29","ID":3}
Response: {"statusCode":0,"streamList":[1433,1433],"ID":3}
Waiting to disconnect...
Connect status: CloseReceived
Closing UDP Connection
```
