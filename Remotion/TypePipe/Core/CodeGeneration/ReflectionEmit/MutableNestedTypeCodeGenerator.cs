using System;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Behaves like <see cref="MutableTypeCodeGenerator"/> but for nested types.
  /// </summary>
  public class MutableNestedTypeCodeGenerator : MutableTypeCodeGenerator
  {
    private readonly ITypeBuilder _enclosingTypeBuilder;

    [CLSCompliant (false)]
    public MutableNestedTypeCodeGenerator (
        MutableType mutableType,
        ITypeBuilder enclosingTypeBuilder,
        IMutableNestedTypeCodeGeneratorFactory nestedTypeCodeGeneratorFactory,
        IReflectionEmitCodeGenerator codeGenerator,
        IEmittableOperandProvider emittableOperandProvider,
        IMemberEmitter memberEmitter,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
        : base (
            mutableType,
            nestedTypeCodeGeneratorFactory,
            codeGenerator,
            emittableOperandProvider,
            memberEmitter,
            initializationBuilder,
            proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("enclosingTypeBuilder", enclosingTypeBuilder);

      _enclosingTypeBuilder = enclosingTypeBuilder;
    }

    [CLSCompliant (false)]
    protected override ITypeBuilder DefineType (IReflectionEmitCodeGenerator codeGenerator, IEmittableOperandProvider emittableOperandProvider)
    {
      return _enclosingTypeBuilder.DefineNestedType (MutableType.Name, MutableType.Attributes, typeof (object));
    }
  }
}