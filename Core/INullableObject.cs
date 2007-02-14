using System;

namespace Rubicon
{
  /// <summary>
  /// Represents a nullable object according to the "Null Object Pattern".
  /// </summary>
  public interface INullableObject
  {
    /// <summary>
    /// Gets a value indicating whether the object is a "Null Object".
    /// </summary>
    bool IsNull {get;}
  }
}