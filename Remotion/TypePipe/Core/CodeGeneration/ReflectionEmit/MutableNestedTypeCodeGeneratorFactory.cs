using System;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Serves as a factory for instances of <see cref="IMutableTypeCodeGenerator"/> for nested types.
  /// </summary>
  public class MutableNestedTypeCodeGeneratorFactory : IMutableNestedTypeCodeGeneratorFactory
  {
    private readonly IReflectionEmitCodeGenerator _reflectionEmitCodeGenerator;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    [CLSCompliant (false)]
    public MutableNestedTypeCodeGeneratorFactory (
        IReflectionEmitCodeGenerator reflectionEmitCodeGenerator,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("reflectionEmitCodeGenerator", reflectionEmitCodeGenerator);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      _reflectionEmitCodeGenerator = reflectionEmitCodeGenerator;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;
    }

    [CLSCompliant (false)]
    public IMutableTypeCodeGenerator Create (MutableType nestedType, ITypeBuilder enclosingTypeBuilder, IMemberEmitter memberEmitter, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("nestedType", nestedType);
      ArgumentUtility.CheckNotNull ("enclosingTypeBuilder", enclosingTypeBuilder);
      ArgumentUtility.CheckNotNull ("memberEmitter", memberEmitter);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      return new MutableNestedTypeCodeGenerator (
          nestedType,
          enclosingTypeBuilder,
          this,
          _reflectionEmitCodeGenerator,
          emittableOperandProvider,
          memberEmitter,
          _initializationBuilder,
          _proxySerializationEnabler);
    }
  }
}