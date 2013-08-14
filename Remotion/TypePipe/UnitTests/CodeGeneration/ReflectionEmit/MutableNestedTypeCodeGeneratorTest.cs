using System;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  // TODO 5550
  //[TestFixture]
  //public class MutableNestedTypeCodeGeneratorTest
  //{
  //  private ITypeBuilder _enclosingTypeBuilderMock;

  //  [SetUp]
  //  public override void SetUp ()
  //  {
  //    base.SetUp();

  //    _enclosingTypeBuilderMock = MockRepository.StrictMock<ITypeBuilder>();

  //    _generator = new MutableNestedTypeCodeGenerator (_enclosingTypeBuilderMock, 
  //        MutableType,
  //        NestedTypeCodeGeneratorFactoryMock,
  //        CodeGeneratorMock,
  //        EmittableOperandProviderMock,
  //        MemberEmitterMock,
  //        InitializationBuilderMock, ProxySerializationEnablerMock);
  //  }

  //  [Ignore("TODO 5550")]
  //  [Test]
  //  public override void DeclareType ()
  //  {
  //    var nestedType = MutableType.AddNestedType();

  //    using (MockRepository.Ordered())
  //    {
  //      _enclosingTypeBuilderMock
  //          .Expect (mock => mock.DefineNestedType (MutableType.Name, MutableType.Attributes))
  //          .Return (_typeBuilderMock);
  //      _typeBuilderMock.Expect (mock => mock.RegisterWith (EmittableOperandProviderMock, MutableType));
  //      CodeGeneratorMock.Expect (mock => mock.DebugInfoGenerator).Return (_debugInfoGeneratorMock);

  //      NestedTypeCodeGeneratorFactoryMock
  //          .Expect (mock => mock.Create (_typeBuilderMock, nestedType))
  //          .Return (_nestedTypeCodeGeneratorMock);
  //      _nestedTypeCodeGeneratorMock.Expect (mock => mock.DeclareType ());
  //    }
  //    MockRepository.ReplayAll ();

  //    _generator.DeclareType ();

  //    MockRepository.VerifyAll ();
  //    var context = (CodeGenerationContext) PrivateInvoke.GetNonPublicField (_generator, "_context");
  //    Assert.That (context, Is.Not.Null);
  //    Assert.That (context.MutableType, Is.SameAs (MutableType));
  //    Assert.That (context.TypeBuilder, Is.SameAs (_typeBuilderMock));
  //    Assert.That (context.DebugInfoGenerator, Is.SameAs (_debugInfoGeneratorMock));
  //    Assert.That (context.EmittableOperandProvider, Is.SameAs (EmittableOperandProviderMock));
  //  }
  //}
}