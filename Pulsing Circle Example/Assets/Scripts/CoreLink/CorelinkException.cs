using System;
using UnityEditor;
using UnityEngine;

namespace CoreLink
{
  public class CorelinkException : Exception
  {
    public CorelinkException()
    { }

    public CorelinkException(string message) : base(message)
    { }

    public CorelinkException(string message, Exception inner) : base(message, inner)
    { }

    public CorelinkException(int statusCode, string message) :
      base("statusCode: " + statusCode + " - " + message)
    {

    }
  }
}
