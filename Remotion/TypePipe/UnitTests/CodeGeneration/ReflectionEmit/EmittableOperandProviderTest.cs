using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class EmittableOperandProviderTest
  {
    private EmittableOperandProvider _map;

    private Type _someType;
    private FieldInfo _someFieldInfo;
    private ConstructorInfo _someConstructorInfo;
    private MethodInfo _someMethodInfo;

    [SetUp]
    public void SetUp ()
    {
      _map = new EmittableOperandProvider();

      _someType = ReflectionObjectMother.GetSomeType();
      _someFieldInfo = ReflectionObjectMother.GetSomeField ();
      _someConstructorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      _someMethodInfo = ReflectionObjectMother.GetSomeMethod ();
    }

    [Test]
    public void AddMapping ()
    {
      CheckAddMapping (_map.AddMapping, _map.GetEmittableOperand, _someType);
      CheckAddMapping (_map.AddMapping, _map.GetEmittableOperand, _someFieldInfo);
      CheckAddMapping (_map.AddMapping, _map.GetEmittableOperand, _someConstructorInfo);
      CheckAddMapping (_map.AddMapping, _map.GetEmittableOperand, _someMethodInfo);
    }

    [Test]
    public void AddMapping_Twice ()
    {
      CheckAddMappingTwiceThrows<Type, IEmittableOperand> (
          _map.AddMapping, _someType, "Type is already mapped.\r\nParameter name: mappedType");
      CheckAddMappingTwiceThrows<FieldInfo, IEmittableOperand> (
          _map.AddMapping, _someFieldInfo, "FieldInfo is already mapped.\r\nParameter name: mappedFieldInfo");
      CheckAddMappingTwiceThrows<ConstructorInfo, IEmittableOperand> (
          _map.AddMapping, _someConstructorInfo, "ConstructorInfo is already mapped.\r\nParameter name: mappedConstructorInfo");
      CheckAddMappingTwiceThrows<MethodInfo, IEmittableMethodOperand> (
          _map.AddMapping, _someMethodInfo, "MethodInfo is already mapped.\r\nParameter name: mappedMethodInfo");
    }
    
    [Test]
    public void GetBuilder_NoMapping ()
    {
      Assert.That (_map.GetEmittableOperand (_someType), Is.Null);
      Assert.That (_map.GetEmittableOperand (_someFieldInfo), Is.Null);
      Assert.That (_map.GetEmittableOperand (_someConstructorInfo), Is.Null);
      Assert.That (_map.GetEmittableOperand (_someMethodInfo), Is.Null);
    }

    private void CheckAddMapping<TMappedObject, TEmittableOperand> (
        Action<TMappedObject, TEmittableOperand> addMappingMethod,
        Func<TMappedObject, TEmittableOperand> getEmittableOperandMethod,
        TMappedObject mappedObject)
        where TEmittableOperand : class, IEmittableOperand
    {
      var fakeBuilder = MockRepository.GenerateStub<TEmittableOperand>();
     
      addMappingMethod (mappedObject, fakeBuilder);

      var result = getEmittableOperandMethod (mappedObject);
      Assert.That (result, Is.SameAs (fakeBuilder));
    }

    private void CheckAddMappingTwiceThrows<TMappedObject, TBuilder> (
        Action<TMappedObject, TBuilder> addMappingMethod, TMappedObject mappedObject, string expectedMessage)
        where TBuilder: class
    {
      addMappingMethod (mappedObject, MockRepository.GenerateStub<TBuilder>());

      Assert.That (
          () => addMappingMethod (mappedObject, MockRepository.GenerateStub<TBuilder>()),
          Throws.ArgumentException.With.Message.EqualTo (expectedMessage));
    }
  }
}