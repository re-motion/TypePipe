using System;
using System.Reflection;

namespace Remotion.Reflection
{
  /// <summary>
  /// Provides in interface for classes that find assemblies.
  /// </summary>
  public interface IAssemblyFinder
  {
    /// <summary>
    /// Finds assemblies as defined by implementors of this interface.
    /// </summary>
    Assembly[] FindAssemblies ();
  }
}