// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.IntegrationTests.TypeAssembly;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.Serialization
{
  public abstract class SerializationTestBase : ObjectFactoryIntegrationTestBase
  {
    private static IParticipant CreateFieldAddingParticipant ()
    {
      var attributeConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute());
      return CreateParticipant (
          mutableType =>
          {
            mutableType.AddField ("AddedIntField", typeof (int), FieldAttributes.Public);
            mutableType.AddField ("AddedSkippedIntField", typeof (int), FieldAttributes.Public)
                       .AddCustomAttribute (new CustomAttributeDeclaration (attributeConstructor, new object[0]));
          });
    }

    private static IParticipant CreateInitializationAddingParticipant ()
    {
      return CreateParticipant (
          mutableType =>
          {
            var stringField = mutableType.GetField ("String");

            mutableType.AddInstanceInitialization (
                ctx =>
                Expression.AddAssign (
                    Expression.Field (ctx.This, stringField),
                    Expression.Constant (" valueFromInstanceInitialization"),
                    ExpressionHelper.StringConcatMethod));
          });
    }

    private static IParticipant CreateCallbackImplementingParticipant ()
    {
      return CreateParticipant (
          mutableType =>
          {
            var stringField = mutableType.GetField ("String");
            var callback = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));

            mutableType.AddInterface (typeof (IDeserializationCallback));
            mutableType.AddExplicitOverride (
                callback,
                ctx => Expression.AddAssign (
                    Expression.Field (ctx.This, stringField), Expression.Constant (" addedCallback"), ExpressionHelper.StringConcatMethod));
          });
    }

    [Test]
    public void NoModifications ()
    {
      var factory = CreateObjectFactoryForSerialization();
      var instance1 = factory.CreateObject<SerializableType>();
      var instance2 = factory.CreateObject<CustomSerializableType>();
      instance1.String = "abc";
      instance2.String = "def";
      instance1.PropertyForPrivateField = "private field value";
      instance2.PropertyForPrivateField = "private field value";

      Assert.That (instance1.ConstructorCalled, Is.True);
      Assert.That (instance2.ConstructorCalled, Is.True);

      Action<SerializableType, SerializationTestContext<SerializableType>> assertions = (deserializedInstance, ctx) =>
      {
        Assert.That (deserializedInstance.ConstructorCalled, Is.False);
        Assert.That (deserializedInstance.String, Is.EqualTo (ctx.ExpectedStringFieldValue));
        Assert.That (deserializedInstance.PropertyForPrivateField, Is.EqualTo ("private field value"));
      };
      CheckInstanceIsSerializable (instance1, assertions, expectedStringFieldValue: "abc");
      CheckInstanceIsSerializable (instance2, assertions, expectedStringFieldValue: "def (custom deserialization ctor)");
    }
    
    [Test]
    public void Standard_AddedFields ()
    {
      var factory = CreateObjectFactoryForSerialization (CreateFieldAddingParticipant);
      var instance1 = factory.CreateObject<SerializableType>();
      var instance2 = factory.CreateObject<CustomSerializableType>();

      PrivateInvoke.SetPublicField (instance1, "AddedIntField", 7);
      PrivateInvoke.SetPublicField (instance1, "AddedSkippedIntField", 7);
      PrivateInvoke.SetPublicField (instance2, "AddedIntField", 7);
      PrivateInvoke.SetPublicField (instance2, "AddedSkippedIntField", 7);

      Action<SerializableType, SerializationTestContext<SerializableType>> assertions = (deserializedInstance, ctx) =>
      {
        Assert.That (PrivateInvoke.GetPublicField (deserializedInstance, "AddedIntField"), Is.EqualTo (7));
        Assert.That (PrivateInvoke.GetPublicField (deserializedInstance, "AddedSkippedIntField"), Is.EqualTo (0));
      };
      CheckInstanceIsSerializable (instance1, assertions);
      CheckInstanceIsSerializable (instance2, assertions);
    }

    [Test]
    public void InstanceInitialization ()
    {
      var factory = CreateObjectFactoryForSerialization (CreateInitializationAddingParticipant);
      var instance1 = factory.CreateObject<SerializableType> ();
      var instance2 = factory.CreateObject<CustomSerializableType> ();
      instance1.String = "abc";
      instance2.String = "def";

      Action<SerializableType, SerializationTestContext<SerializableType>> assertions =
          (deserializedInstance, ctx) => Assert.That (deserializedInstance.String, Is.EqualTo (ctx.ExpectedStringFieldValue));
      CheckInstanceIsSerializable (instance1, assertions, expectedStringFieldValue: "abc valueFromInstanceInitialization");
      CheckInstanceIsSerializable (instance2, assertions, expectedStringFieldValue: "def (custom deserialization ctor) valueFromInstanceInitialization");
    }

    [Test]
    public void ExistingCallback ()
    {
      var factory = CreateObjectFactoryForSerialization (CreateInitializationAddingParticipant);
      var instance1 = factory.CreateObject<DeserializationCallbackType> ();
      var instance2 = factory.CreateObject<CustomDeserializationCallbackType> ();
      instance1.String = "abc";
      instance2.String = "def";

      Action<SerializableType, SerializationTestContext<SerializableType>> assertions =
          (deserializedInstance, ctx) => Assert.That (deserializedInstance.String, Is.EqualTo (ctx.ExpectedStringFieldValue));
      CheckInstanceIsSerializable (instance1, assertions, expectedStringFieldValue: "abc existingCallback valueFromInstanceInitialization");
      CheckInstanceIsSerializable (
          instance2, assertions, expectedStringFieldValue: "def (custom deserialization ctor) existingCallback valueFromInstanceInitialization");
    }

    [Test]
    public void AddedCallback ()
    {
      var factory = CreateObjectFactoryForSerialization (CreateInitializationAddingParticipant, CreateCallbackImplementingParticipant);
      var instance1 = factory.CreateObject<SerializableType> ();
      var instance2 = factory.CreateObject<CustomSerializableType> ();
      instance1.String = "abc";
      instance2.String = "def";

      Action<SerializableType, SerializationTestContext<SerializableType>> assertions =
          (deserializedInstance, ctx) => Assert.That (deserializedInstance.String, Is.EqualTo (ctx.ExpectedStringFieldValue));
      CheckInstanceIsSerializable (instance1, assertions, expectedStringFieldValue: "abc addedCallback valueFromInstanceInitialization");
      CheckInstanceIsSerializable (
          instance2, assertions, expectedStringFieldValue: "def (custom deserialization ctor) addedCallback valueFromInstanceInitialization");
    }

    [Test]
    public void OnDeserializationMethodWithoutInterface ()
    {
      var factory = CreateObjectFactoryForSerialization (CreateInitializationAddingParticipant);
      var instance = factory.CreateObject<OnDeserializationMethodType> ();
      instance.String = "abc";

      CheckInstanceIsSerializable (
          instance,
          (deserializedInstance, ctx) => Assert.That (deserializedInstance.String, Is.EqualTo (ctx.ExpectedStringFieldValue)),
          expectedStringFieldValue: "abc valueFromInstanceInitialization");
    }

    [Test]
    public void CyclicObjectGraph ()
    {
      var factory = CreateObjectFactoryForSerialization();
      var instance1 = CreateCyclicInstance<ReferencingSerializableType> (factory);
      var instance2 = CreateCyclicInstance<CustomReferencingSerializableType> (factory);

      CheckInstanceIsSerializable (instance1, (deserializedInstance, ctx) => { });
      CheckInstanceIsSerializable (instance2, (deserializedInstance, ctx) => { });
    }

    [Test]
    public void ISerializable_CannotModifyOrOverrideGetObjectData ()
    {
      SkipSavingAndPeVerification();
      var factory = CreateObjectFactoryForSerialization (CreateFieldAddingParticipant);

      var message = "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
                    + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.";
      Assert.That (
          () => factory.GetAssembledType (typeof (ExplicitISerializableType)),
          Throws.TypeOf<NotSupportedException>().With.Message.Contains (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (DerivedExplicitISerializableType)),
          Throws.TypeOf<NotSupportedException>().With.Message.Contains (message));
    }

    [Test]
    public void IDeserializationCallback_CannotModifyOrOverrideOnDeserialization ()
    {
      SkipSavingAndPeVerification();
      var factory = CreateObjectFactoryForSerialization (CreateInitializationAddingParticipant);

      var message = "The underlying type implements IDeserializationCallback but OnDeserialization cannot be overridden. "
                    + "Make sure that OnDeserialization is implemented implicitly (not explicitly) and virtual.";
      Assert.That (
          () => factory.GetAssembledType (typeof (ExplicitIDeserializationCallbackType)),
          Throws.TypeOf<NotSupportedException>().With.Message.Contains (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (DerivedExplicitIDeserializationCallbackType)),
          Throws.TypeOf<NotSupportedException>().With.Message.Contains (message));
    }

    private T CreateCyclicInstance<T> (IObjectFactory factory) where T : ReferencingSerializableType
    {
      var instance = factory.CreateObject<T> ();
      var referenceObject = new ReferencedType ();

      instance.ReferencedObject = referenceObject;
      referenceObject.ReferencingObject = instance;

      return instance;
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected abstract IObjectFactory CreateObjectFactoryForSerialization (params Func<IParticipant>[] participantProviders);

    protected abstract Func<SerializationTestContext<T>, T> CreateDeserializationCallback<T> (SerializationTestContext<T> context);

    private AppDomain _appDomainForDeserialization;

    public override void TestFixtureSetUp ()
    {
      base.TestFixtureSetUp();

      var evidence = AppDomain.CurrentDomain.Evidence;
      var setup = AppDomain.CurrentDomain.SetupInformation;
      _appDomainForDeserialization = AppDomain.CreateDomain ("AppDomainForDeserialization", evidence, setup);
    }

    public override void TestFixtureTearDown ()
    {
      AppDomain.Unload (_appDomainForDeserialization);

      base.TestFixtureTearDown();
    }

    protected void CheckInstanceIsSerializable<T> (
        T instance, Action<T, SerializationTestContext<T>> assertions, string expectedStringFieldValue = null)
    {
      Assert.That (instance.GetType().IsSerializable, Is.True);

      var context =
          new SerializationTestContext<T>
          {
              SerializedData = Serializer.Serialize (instance),
              Assertions = assertions,
              ExpectedAssemblyQualifiedName = instance.GetType().AssemblyQualifiedName,
              SerializedTypeFullName = instance.GetType().FullName,
              ExpectedStringFieldValue = expectedStringFieldValue
          };
      context.DeserializationCallback = CreateDeserializationCallback (context);

      _appDomainForDeserialization.DoCallBack (context.AppDomainDelegate);
    }

    [Serializable]
    protected class SerializationTestContext<T>
    {
      public IEnumerable<Func<IParticipant>> ParticipantProviders { get; set; }
      public byte[] SerializedData { get; set; }
      public Action<T, SerializationTestContext<T>> Assertions { get; set; }

      public string ExpectedAssemblyQualifiedName { get; set; }
      public string SerializedTypeFullName { get; set; }
      public string ExpectedStringFieldValue { get; set; }

      public Func<SerializationTestContext<T>, T> DeserializationCallback { get; set; }

      public void AppDomainDelegate ()
      {
        var deserializedInstance = DeserializationCallback (this);
        Assertions (deserializedInstance, this);
      }
    }

    [Serializable]
    public class SerializableType
    {
      public string String;
      private string _privateField;

      [NonSerialized]
      public readonly bool ConstructorCalled;

      public SerializableType ()
      {
        ConstructorCalled = true;
      }

      public SerializableType (string arg1) { Dev.Null = arg1; }

      public string PropertyForPrivateField
      {
        get { return _privateField; }
        set { _privateField = value; }
      }
    }

    [Serializable]
    public class CustomSerializableType : SerializableType, ISerializable
    {
      public CustomSerializableType () { }

      public CustomSerializableType (SerializationInfo info, StreamingContext context) : base ("")
      {
        String = info.GetString ("key1") + " (custom deserialization ctor)";
        PropertyForPrivateField = info.GetString ("key2");
      }

      public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
      {
        info.AddValue ("key1", String);
        info.AddValue ("key2", PropertyForPrivateField);
      }
    }

    [Serializable]
    public class DeserializationCallbackType : SerializableType, IDeserializationCallback
    {
      public virtual void OnDeserialization (object sender)
      {
        String += " existingCallback";
      }
    }

    [Serializable]
    public class CustomDeserializationCallbackType : CustomSerializableType, IDeserializationCallback
    {
      public CustomDeserializationCallbackType () { }

      public CustomDeserializationCallbackType (SerializationInfo info, StreamingContext context) : base (info, context)
      {
        String = info.GetString ("key1") + " (custom deserialization ctor)";
      }

      public virtual void OnDeserialization (object sender)
      {
        String += " existingCallback";
      }
    }

    [Serializable]
    public class OnDeserializationMethodType : SerializableType
    {
      [UsedImplicitly]
      public virtual void OnDeserialization (object sender)
      {
        String += " existingCallback (but does not implement IDeserializationCallback)";
      }
    }

    [Serializable]
    public class ReferencingSerializableType : IDeserializationCallback
    {
      public ReferencedType ReferencedObject;

      public virtual void OnDeserialization (object sender)
      {
        Assert.That (ReferencedObject, Is.Not.Null);
        Assert.That (ReferencedObject.ReferencingObject, Is.Not.Null.And.SameAs (this));
      }
    }

    [Serializable]
    public class CustomReferencingSerializableType : ReferencingSerializableType, ISerializable
    {
      public CustomReferencingSerializableType () { }

      public CustomReferencingSerializableType (SerializationInfo info, StreamingContext context)
      {
        ReferencedObject = (ReferencedType) info.GetValue ("ReferencedObject", typeof (ReferencedType));
      }

      public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
      {
        info.AddValue ("ReferencedObject", ReferencedObject);
      }
    }

    [Serializable]
    public class ReferencedType
    {
      public ReferencingSerializableType ReferencingObject;
    }

    public class ExplicitISerializableType : ISerializable
    {
      public ExplicitISerializableType (SerializationInfo info, StreamingContext context) { Dev.Null = info; Dev.Null = context; }
      void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    public class DerivedExplicitISerializableType : ExplicitISerializableType
    {
      public DerivedExplicitISerializableType (SerializationInfo info, StreamingContext context) : base (info, context) { }
    }

    public class ExplicitIDeserializationCallbackType : SerializableType, IDeserializationCallback
    {
      void IDeserializationCallback.OnDeserialization (object sender) { }
    }

    [Serializable]
    public class DerivedExplicitIDeserializationCallbackType : ExplicitIDeserializationCallbackType { }
  }
}