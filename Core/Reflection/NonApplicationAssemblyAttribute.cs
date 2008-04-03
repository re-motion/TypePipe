using System;

namespace Remotion.Reflection
{
  /// <summary>
  /// Causes an assembly to be ignored by <see cref="ApplicationAssemblyFinderFilter"/> (which is used by the configuration loaders
  /// in Remotion.Data.DomainObjects and Remotion.Mixins).
  /// </summary>
  [AttributeUsage (AttributeTargets.Assembly)]
  public class NonApplicationAssemblyAttribute : Attribute
  {
  }
}