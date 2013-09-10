using System;
using Remotion.Reflection;
using Remotion.TypePipe.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Creates delegates for constructing instances of assembled types.
  /// </summary>
  public class ConstructorDelegateFactory : IConstructorDelegateFactory
  {
    private readonly IConstructorFinder _constructorFinder;
    private readonly IDelegateFactory _delegateFactory;

    public ConstructorDelegateFactory (IConstructorFinder constructorFinder, IDelegateFactory delegateFactory)
    {
      ArgumentUtility.CheckNotNull ("constructorFinder", constructorFinder);
      ArgumentUtility.CheckNotNull ("delegateFactory", delegateFactory);
      
      _constructorFinder = constructorFinder;
      _delegateFactory = delegateFactory;
    }

    public Delegate CreateConstructorCall (Type requestedType, Type assembledType, Type delegateType, bool allowNonPublic)
    {
      ArgumentUtility.CheckNotNull ("requestedType", requestedType);
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);
      ArgumentUtility.CheckNotNull ("delegateType", delegateType);

      var ctorSignature = _delegateFactory.GetSignature (delegateType);
      var constructor = _constructorFinder.GetConstructor (requestedType, ctorSignature.Item1, allowNonPublic, assembledType);

      return _delegateFactory.CreateConstructorCall (constructor, delegateType);
    }
  }
}