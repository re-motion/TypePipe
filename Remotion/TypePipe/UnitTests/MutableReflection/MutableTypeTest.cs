using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableTypeTest
  {
    private ITypeInfo _originalTypeInfoStub;
    private IEqualityComparer<MemberInfo> _memberInfoEqualityComparerStub;
    private IBindingFlagsEvaluator _bindingFlagsEvaluatorMock;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _originalTypeInfoStub = MockRepository.GenerateStub<ITypeInfo>();
      _memberInfoEqualityComparerStub = MockRepository.GenerateStub<IEqualityComparer<MemberInfo>>();
      _bindingFlagsEvaluatorMock = MockRepository.GenerateStrictMock<IBindingFlagsEvaluator>();
      _mutableType = MutableTypeObjectMother.Create (_originalTypeInfoStub, _memberInfoEqualityComparerStub, _bindingFlagsEvaluatorMock);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_mutableType.AddedInterfaces, Is.Empty);
      Assert.That (_mutableType.AddedFields, Is.Empty);
    }

    [Test]
    public void AddInterface ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces ()).Return (Type.EmptyTypes);

      _mutableType.AddInterface (typeof (IDisposable));
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.AddedInterfaces, Is.EqualTo (new[] { typeof(IDisposable), typeof(IComparable) }));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Type must be an interface.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfNotAnInterface ()
    {
      _mutableType.AddInterface (typeof (string));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Interface 'System.IDisposable' is already implemented.\r\nParameter name: interfaceType")]
    public void AddInterface_ThrowsIfAlreadyImplemented ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces ()).Return (new[] { typeof (IDisposable) });

      _mutableType.AddInterface (typeof (IDisposable));
    }

    [Test]
    public void GetInterfaces ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetInterfaces()).Return (new[] { typeof (IDisposable) });
      _mutableType.AddInterface (typeof (IComparable));

      Assert.That (_mutableType.GetInterfaces(), Is.EqualTo (new[] { typeof (IDisposable), typeof (IComparable) }));
    }

    [Test]
    public void AddField ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
          .Return (new FieldInfo[0]);

      var newField = _mutableType.AddField ("_newField", typeof (string), FieldAttributes.Private);

      // Correct field info instance
      Assert.That (newField.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (newField.Name, Is.EqualTo ("_newField"));
      Assert.That (newField.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (newField.Attributes, Is.EqualTo (FieldAttributes.Private));
      // Field info is stored
      Assert.That (_mutableType.AddedFields, Is.EqualTo (new[] { newField }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Field with equal signature already exists.\r\nParameter name: name, type")]
    public void AddField_ThrowsIfAlreadyExist ()
    {
      var field = FutureFieldInfoObjectMother.Create (name: "_bla", fieldType: typeof (string));
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field });
      _memberInfoEqualityComparerStub
          .Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (true);

      _mutableType.AddField ("_bla", typeof (string), FieldAttributes.Private);
    }

    [Test]
    public void AddField_ReliesOnFieldSignature ()
    {
      var field = FutureFieldInfoObjectMother.Create (name: "_foo", fieldType: typeof (object));
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field });
      var attributes = FieldAttributes.Private;
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _memberInfoEqualityComparerStub.Stub (stub => stub.Equals (Arg<FieldInfo>.Is.Anything, Arg<FieldInfo>.Is.Anything))
          .Return (false);
      _bindingFlagsEvaluatorMock.Stub (stub => stub.HasRightAttributes (attributes, bindingFlags)).Return (true);

      _mutableType.AddField ("_foo", typeof (string), attributes);
      var fields = _mutableType.GetFields (bindingFlags);

      Assert.That (fields, Has.Length.EqualTo (2));
    }

    [Test]
    public void GetFields ()
    {
      var field1 = FutureFieldInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      _originalTypeInfoStub.Stub (stub => stub.GetFields (bindingFlags)).Return (new[] { field1 });
      var attributes = FieldAttributes.Private;
      _bindingFlagsEvaluatorMock.Stub (stub => stub.HasRightAttributes (attributes, bindingFlags)).Return (true);

      var field2 = _mutableType.AddField ("field2", ReflectionObjectMother.GetSomeType(), attributes);
      var fields = _mutableType.GetFields (bindingFlags);

      Assert.That (fields, Is.EqualTo (new[] { field1, field2 }));
    }

    [Test]
    public void GetFields_FilterAddedWithUtility ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new FieldInfo[0]);
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (FieldAttributes.Public, bindingFlags)).Return (false);

      _mutableType.AddField ("_newField", typeof (int), FieldAttributes.Public);
      var fields = _mutableType.GetFields (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations ();
      Assert.That (fields, Is.Empty);
    }

    [Test]
    public void GetField ()
    {
      var field1 = FutureFieldInfoObjectMother.Create (name: "field1", fieldType: typeof (string));
      var field2 = FutureFieldInfoObjectMother.Create (name: "field2", fieldType: typeof (int));
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field1, field2 });
      _bindingFlagsEvaluatorMock
        .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
        .Return (true);

      var resultField = _mutableType.GetField ("field2", BindingFlags.NonPublic | BindingFlags.Instance);

      Assert.That (resultField, Is.SameAs (field2));
    }

    [Test]
    public void GetField_NoMatch ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new FieldInfo[0]);

      Assert.That (_mutableType.GetField ("field"), Is.Null);
    }

    [Test]
    [ExpectedException (typeof (AmbiguousMatchException), ExpectedMessage = "Ambiguous field name 'foo'.")]
    public void GetField_Ambigious ()
    {
      var field1 = FutureFieldInfoObjectMother.Create (name: "foo", fieldType: typeof (string));
      var field2 = FutureFieldInfoObjectMother.Create (name: "foo", fieldType: typeof (int));
      _originalTypeInfoStub.Stub (stub => stub.GetFields (Arg<BindingFlags>.Is.Anything)).Return (new[] { field1, field2 });
      _bindingFlagsEvaluatorMock
        .Stub (stub => stub.HasRightAttributes (Arg<FieldAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
        .Return (true);

      _mutableType.GetField ("foo", 0);
    }

    [Test]
    public void AddConstructor ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        .Return (new ConstructorInfo[0]);
      var attributes = MethodAttributes.Public;
      var parameterDeclarations = new[] { ParameterDeclarationObjectMother.Create (), ParameterDeclarationObjectMother.Create () };
      var ctorInfo = _mutableType.AddConstructor (attributes, parameterDeclarations);

      // Correct constroctur info instance
      Assert.That (ctorInfo.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (ctorInfo.Attributes, Is.EqualTo (attributes));
      var expectedParameterInfos = new[] { new { ParameterType = parameterDeclarations[0].Type }, new { ParameterType = parameterDeclarations[1].Type } };
      var actualParameterInfos = ctorInfo.GetParameters().Select (pi => new { pi.ParameterType });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      // Constructor info is stored
      Assert.That (_mutableType.AddedConstructors, Is.EqualTo (new[] { ctorInfo }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Static constructors are not (yet) supported.\r\nParameter name: attributes")]
    public void AddConstructor_ThrowsForStatic ()
    {
      _mutableType.AddConstructor (MethodAttributes.Static);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
      "Constructor with equal signature already exists.\r\nParameter name: parameterDeclarations")]
    public void AddConstructor_ThrowsIfAlreadyExists ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new ConstructorInfo[1]);
      _memberInfoEqualityComparerStub.Stub (stub => stub.Equals (null, null)).IgnoreArguments().Return (true);

      _mutableType.AddConstructor (0);
    }

    [Test]
    public void GetConstructors ()
    {
      var constructor1 = MutableConstructorInfoObjectMother.Create();
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; // Don't return static constructors by default
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (bindingFlags)).Return (new[] { constructor1 });
      var attributes = MethodAttributes.Public;
      var parameterDeclarations = new ArgumentTestHelper(7).ParameterDeclarations; // Need different signature
      _bindingFlagsEvaluatorMock.Stub (mock => mock.HasRightAttributes (attributes, bindingFlags)).Return (true);

      var constructor2 = _mutableType.AddConstructor (attributes, parameterDeclarations);
      var constructors = _mutableType.GetConstructors (bindingFlags);

      Assert.That (constructors, Is.EqualTo (new[] { constructor1, constructor2 }));
    }

    [Test]
    public void GetConstructors_FilterAddedWithUtility () 
    {
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new ConstructorInfo[0]);
      var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      _bindingFlagsEvaluatorMock.Expect (mock => mock.HasRightAttributes (MethodAttributes.Public, bindingFlags)).Return (false);

      _mutableType.AddConstructor (MethodAttributes.Public);
      var constructors = _mutableType.GetConstructors (bindingFlags);

      _bindingFlagsEvaluatorMock.VerifyAllExpectations();
      Assert.That (constructors, Is.Empty);
    }

    [Test]
    public void GetConstructorImpl ()
    {
      var constructor1 = MutableConstructorInfoObjectMother.Create();
      var arguments = new ArgumentTestHelper (typeof (int));
      var constructor2 = MutableConstructorInfoObjectMother.Create (parameterDeclarations: arguments.ParameterDeclarations);
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new[] { constructor1, constructor2 });
      _bindingFlagsEvaluatorMock
        .Stub (stub => stub.HasRightAttributes (Arg<MethodAttributes>.Is.Anything, Arg<BindingFlags>.Is.Anything))
        .Return (true);

      var resultCtor = _mutableType.GetConstructor (arguments.Types);

      Assert.That (resultCtor, Is.SameAs (constructor2));
    }

    [Test]
    public void GetConstructorImpl_NoMatch ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetConstructors (Arg<BindingFlags>.Is.Anything)).Return (new ConstructorInfo[0]);

      Assert.That (_mutableType.GetConstructor (Type.EmptyTypes), Is.Null);
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_mutableType.HasElementType, Is.False);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_mutableType.Assembly, Is.Null);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_mutableType.IsByRef, Is.False);
    }

    [Test]
    public void UnderlyingSystemType ()
    {
      var type = typeof (string);
      _originalTypeInfoStub.Stub (stub => stub.GetUnderlyingSystemType ()).Return (Maybe.ForValue (type));

      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (type));
    }

    [Test]
    public void UnderlyingSystemType_ForNull ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetUnderlyingSystemType ()).Return (Maybe<Type>.Nothing);

      Assert.That (_mutableType.UnderlyingSystemType, Is.SameAs (_mutableType));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      _originalTypeInfoStub.Stub (stub => stub.GetAttributeFlags()).Return (TypeAttributes.Sealed);

      Assert.That (_mutableType.Attributes, Is.EqualTo (TypeAttributes.Sealed));
    }

    [Test]
    public void BaseType ()
    {
      var baseType = typeof (IDisposable);
      _originalTypeInfoStub.Stub (stub => stub.GetBaseType()).Return(baseType);

      Assert.That (_mutableType.BaseType, Is.SameAs(baseType));
    }
  }
}