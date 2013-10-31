using System;

namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// Caches the <see cref="Delegate"/>s that perform constructor calls for pipeline generated <see cref="Type"/>s to enable efficient object creation.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public interface IConstructorCallCache
  {
    Delegate GetOrCreateConstructorCall (AssembledTypeID typeID, Type delegateType, bool allowNonPublic);
  }
}