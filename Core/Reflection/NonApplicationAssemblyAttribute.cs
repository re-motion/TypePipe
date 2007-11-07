using System;

namespace Rubicon.Reflection
{
  /// <summary>
  /// Causes an assembly to be ignored by <see cref="ApplicationAssemblyFinderFilter"/> (which is used by the configuration loaders
  /// in Rubicon.Data.DomainObjects and Rubicon.Mixins).
  /// </summary>
  [AttributeUsage (AttributeTargets.Assembly)]
  public class NonApplicationAssemblyAttribute : Attribute
  {
  }
}