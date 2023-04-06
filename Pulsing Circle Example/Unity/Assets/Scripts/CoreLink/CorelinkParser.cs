using System;
using UnityEditor;
using UnityEngine;
namespace CoreLink
{
  /// <summary> This class is a purely static utility class that contains 
  /// helper functions for CoreLink data parsing. </summary>
  public class CorelinkParser
  {
    /// <summary>
    /// Converts a corelink message byte array into a Vector3
    /// </summary>
    /// <param name="message">Vector3 read from the first 3 floats of the message</param>
    /// <returns></returns>
    public static Vector3 bytesToVector3(byte[] message)
    {
      return new Vector3
      {
        x = BitConverter.ToSingle(message, 0),
        y = BitConverter.ToSingle(message, sizeof(float)),
        z = BitConverter.ToSingle(message, sizeof(float) * 2)
      };
    }
    /// <summary>
    /// Converts a Vector3 into bytes to send via CoreLink
    /// </summary>
    /// <param name="vector3">Vector3 to convert into a byte[12]</param>
    /// <returns></returns>
    public static byte[] vector3ToBytes(Vector3 vector3)
    {
      byte[] msg = new byte[12];

      byte[] valsx = BitConverter.GetBytes(vector3.x);
      byte[] valsy = BitConverter.GetBytes(vector3.y);
      byte[] valsz = BitConverter.GetBytes(vector3.z);
      for (int j = 0; j < 4; j++)
      {
        msg[j] = valsx[j];
        msg[4 + j] = valsy[j];
        msg[8 + j] = valsz[j];
      }
      return msg;
    }
    /// <summary>
    /// Converts a corelink message byte array into a Quaternion
    /// </summary>
    /// <param name="message">Vector3 read from the first 3 floats of the message</param>
    /// <returns></returns>
    public static Quaternion bytesToQuaternion(byte[] message)
    {
      return new Quaternion
      {
        x = BitConverter.ToSingle(message, 0),
        y = BitConverter.ToSingle(message, sizeof(float)),
        z = BitConverter.ToSingle(message, sizeof(float) * 2),
        w = BitConverter.ToSingle(message, sizeof(float) * 3)
      };
    }
    /// <summary>
    /// Converts a Vector3 into bytes to send via CoreLink
    /// </summary>
    /// <param name="vector3">Vector3 to convert into a byte[12]</param>
    /// <returns></returns>
    public static byte[] quaternionToBytes(Quaternion vector3)
    {
      byte[] msg = new byte[16];

      byte[] valsx = BitConverter.GetBytes(vector3.x);
      byte[] valsy = BitConverter.GetBytes(vector3.y);
      byte[] valsz = BitConverter.GetBytes(vector3.z);
      byte[] valsw = BitConverter.GetBytes(vector3.w);
      for (int j = 0; j < 4; j++)
      {
        msg[j] = valsx[j];
        msg[4 + j] = valsy[j];
        msg[8 + j] = valsz[j];
        msg[12 + j] = valsw[j];
      }
      return msg;
    }
  }
}
