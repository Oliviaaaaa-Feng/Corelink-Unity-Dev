# Real-Time MoCap to Unity via Corelink

## Overview
This is a project that demonstrates the integration of real-time motion tracking data into Unity using the [Corelink server](https://corelink.hsrn.nyu.edu/). The project processes 3D motion capture (MoCap) data, encodes it in C++, and decodes it in Unity for visualization and interaction with 3D skeletal models. This system enables real-time applications such as virtual reality (VR), gaming, and live animation.

## Key Features
- **Real-time Data Transmission**: Seamless transfer of motion tracking data from a real person to Unity via the Corelink server.
- **MoCap Integration**: Utilizes OptiTrack's Motive to capture motion data, decode it using C++, and apply it to Unity skeletal models.
- **3D Skeletal Animation**: Supports 21-bone skeleton models with precise position and rotation tracking.
- **Custom Visualization**: Provides Unity packages for rendering and interacting with motion data.

---

## Workflow

### 1. Capturing Motion Data with Motive
- **Motion Tracking**: Use OptiTrack’s Motive application in the FRL Lab to capture real-time motion data from a person wearing the motion capture suit.

### 2. Decoding Motion Data
- **Client Application**: Run the C++ client application in the FRL Lab to receive motion tracking data from Motive.
- **Data Decoding**: Decode the MoCap data into a string format for further processing.

### 3. Transferring Data to Corelink Server
- **Data Transmission**: Connect the C++ client application to the Corelink server (running in the data center) and send the decoded motion tracking data to the server.

### 4. Receiving Data in Game Engine
- **Unity or Unreal Engine**: Write C# scripts in Unity or C++ in Unreal Engine to connect to the Corelink server and request the motion tracking data.

### 5. Mapping Data to Skeleton Model
- **Data Parsing**: Parse the received data and group every seven data points (3 position data and 4 rotation data) for each bone.
- **Skeleton Mapping**: Map the parsed data to the corresponding 21-bone skeleton model in Unity or Unreal Engine.

### 6. Real-time Motion Visualization
- **Skeleton Animation**: Render the real-time motion on the skeleton model in the game engine, successfully transferring human motion to the virtual skeleton.
---

## Installation and Setup

### Prerequisites
- [OptiTrack Motive](https://optitrack.com/products/motive/) for motion capture.
- Visual Studio or other IDE for compiling the `sampleClient.cpp` project.
- Unity 2021 or later for visualization.
- Corelink server setup.
- Chrome Remote Desktop (optional for remote access to MoCap PC).

### Steps
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Oliviaaaaa-Feng/Corelink-Unity-Dev.git
   cd Corelink-Unity-Dev
   ```

2. **Access MoCap Data**:
   - Detail in Wiki [Access MoCap Data](https://github.com/Oliviaaaaa-Feng/Corelink-Unity-Dev/wiki/Access-MoCap-Data)

3. **Run Unity Demo**:
   - Open the Unity project.
   - Import `SkeletonVersion.unitypackage`.
   - Play the scene to visualize motion data on the skeleton model.

---

## Visualization and Examples
Wiki [Corelink Unity Example](https://github.com/Oliviaaaaa-Feng/Corelink-Unity-Dev/wiki/Corelink-Unity-Example)
