using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Corelink
{
  /// <summary>
  /// This class is a utility class to serialize the unity Transform class
  /// in order to pass/retrieve to/from corelink 
  /// </summary>
  public class TransformPasser
  {
    public Vector3 position =  new Vector3(0, 0, 0);
    public Vector3 localPosition =  new Vector3(0, 0, 0);
    public Vector3 eulerAngles = new Vector3(0, 0, 0);
    public Vector3 localEulerAngles = new Vector3(0, 0, 0);
    public Vector3 localScale = new Vector3(1, 1, 1);
    public Quaternion rotation;
    public Quaternion localRotation;

    /// <summary>
    /// Default Empty Constructor, at origin with scale of 1, 1, 1 
    /// </summary>
    public TransformPasser(){}
    /// <summary>
    /// Constructor to create TransformPasser that holds position, rotation, 
    /// and scale of transform argument.
    /// </summary>
    /// <param name="transform">Transform of the gameObject to pass</param>
    /// <returns>TransformPasser object set to values of Transform</returns>
    public TransformPasser(Transform transform)
    {
      this.position = transform.position;
      this.localPosition = transform.localPosition;
      this.eulerAngles = transform.eulerAngles;
      this.localEulerAngles = transform.localEulerAngles;
      this.localScale = transform.localScale;
      this.rotation = transform.rotation;
      this.localRotation = transform.localRotation;
    }

    /// <summary>
    /// Load function to extract values from TransformPasser into an object's transform
    /// Call within <c>Update()</c>
    /// </summary>
    /// <param name="destination">Transform to load data into</param>
    /// <param name="saved">TransformPasser to pull data from</param>
    /// <example>saved.LoadTransform(destination, saved);</example>
    public void LoadTransform(Transform destination, TransformPasser saved)
    {
      Debug.Log("Dest: " + destination.position + "Saved: " + saved.position);
      destination.position = saved.position;
      destination.localPosition = saved.localPosition;
      destination.eulerAngles = saved.eulerAngles;
      destination.localEulerAngles = saved.localEulerAngles;
      destination.localScale = saved.localScale;
      destination.rotation = saved.rotation;
      destination.localRotation = saved.localRotation;
    }
  }       
}
