using System.Reflection;
namespace Remotion.Reflection
{
  /// <summary>
  /// Provides an interface for filtering the assemblies found by the <see cref="AssemblyFinder"/>.
  /// </summary>
  /// <remarks>The filtering interface provides a two-step model: first, the <see cref="AssemblyFinder"/> checks whether the assembly name fits the 
  /// criteria of the filter implementation, then, it checks whether the assembly itself fits the criteria. If the <see cref="AssemblyFinder"/> locates
  /// an assembly on disk, it will only load it if the assembly's name passes the first step. After loading, the second step can still reject the
  /// assembly based on more detailed investigation.</remarks>
  public interface IAssemblyFinderFilter
  {
    /// <summary>
    /// Determines whether the assembly of the given name should be considered for inclusion by the <see cref="AssemblyFinder"/>.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to be checked.</param>
    /// <returns>True if the <see cref="AssemblyFinder"/> should consider this assembly; otherwise false.</returns>
    /// <remarks>This is the first step of a two-step filtering protocol. Assemblies rejected by this method will not be explicitly loaded by the
    /// <see cref="AssemblyFinder"/>.</remarks>
    bool ShouldConsiderAssembly (AssemblyName assemblyName);
    
    /// <summary>
    /// Determines whether the given assembly should be included in the list of assemblies returned by the <see cref="AssemblyFinder"/>.
    /// </summary>
    /// <param name="assembly">The assembly to be checked.</param>
    /// <returns>True if the <see cref="AssemblyFinder"/> should return this assembly; otherwise false.</returns>
    /// <remarks>This is the second step of a two-step filtering protocol. Only assemblies not rejected by <see cref="ShouldConsiderAssembly"/> are
    /// passed on to this step.</remarks>
    bool ShouldIncludeAssembly (Assembly assembly);
  }
}