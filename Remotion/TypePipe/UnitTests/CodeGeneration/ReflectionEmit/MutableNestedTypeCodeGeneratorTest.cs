using System;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MutableNestedTypeCodeGeneratorTest : MutableTypeCodeGeneratorTest
  {
    private ITypeBuilder _enclosingTypeBuilderMock;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp();

      _enclosingTypeBuilderMock = MockRepository.StrictMock<ITypeBuilder>();

      Generator = new MutableNestedTypeCodeGenerator (_enclosingTypeBuilderMock, 
          MutableType,
          NestedTypeCodeGeneratorFactoryMock,
          CodeGeneratorMock,
          EmittableOperandProviderMock,
          MemberEmitterMock,
          InitializationBuilderMock, ProxySerializationEnablerMock);
    }

    [Test]
    public override void DeclareType ()
    {
      var nestedType = MutableType.AddNestedType();

      using (MockRepository.Ordered())
      {
        _enclosingTypeBuilderMock
            .Expect (mock => mock.DefineNestedType (MutableType.Name, MutableType.Attributes))
            .Return (TypeBuilderMock);
        TypeBuilderMock.Expect (mock => mock.RegisterWith (EmittableOperandProviderMock, MutableType));
        CodeGeneratorMock.Expect (mock => mock.DebugInfoGenerator).Return (DebugInfoGeneratorMock);

        NestedTypeCodeGeneratorFactoryMock
            .Expect (mock => mock.Create (TypeBuilderMock, nestedType))
            .Return (NestedTypeCodeGeneratorMock);
        NestedTypeCodeGeneratorMock.Expect (mock => mock.DeclareType ());
      }
      MockRepository.ReplayAll ();

      Generator.DeclareType ();

      MockRepository.VerifyAll ();
      var context = (CodeGenerationContext) PrivateInvoke.GetNonPublicField (Generator, "_context");
      Assert.That (context, Is.Not.Null);
      Assert.That (context.MutableType, Is.SameAs (MutableType));
      Assert.That (context.TypeBuilder, Is.SameAs (TypeBuilderMock));
      Assert.That (context.DebugInfoGenerator, Is.SameAs (DebugInfoGeneratorMock));
      Assert.That (context.EmittableOperandProvider, Is.SameAs (EmittableOperandProviderMock));
    }
  }
}