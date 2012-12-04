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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.FunctionalProgramming;
using Remotion.ServiceLocation;
using Remotion.TypePipe.IntegrationTests.TypeAssembly;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.Serialization;

namespace Remotion.TypePipe.IntegrationTests
{
  [Ignore("TODO 5223")]
  [TestFixture]
  public class ComplexSerializationTest : ObjectFactoryIntegrationTestBase
  {
    private IObjectFactoryRegistry _registry;

    public override void SetUp ()
    {
      base.SetUp ();

      _registry = SafeServiceLocator.Current.GetInstance<IObjectFactoryRegistry> ();
    }

    [Test]
    public void Standard_NoModifications ()
    {
      var factory = CreateObjectFactory ();
      var instance = factory.CreateObject<SerializableType> ();

      CheckInstanceIsSerializable (instance);
    }

    [Test]
    public void Standard_AddedFields ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant ());
      var instance = factory.CreateObject<SerializableType> ();

      CheckInstanceIsSerializableAndAddedFields (instance);
    }

    [Test]
    public void Custom_AddedFields ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant ());
      var instance = factory.CreateObject<CustomSerializableType> ();

      CheckInstanceIsSerializableAndAddedFields (instance, ctorWasCalled: true);
    }

    [Test]
    public void InstanceInitialization ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant (), CreateInitializationAddingParticipant ());
      var instance1 = factory.CreateObject<SerializableType> ();
      var instance2 = factory.CreateObject<CustomSerializableType> ();

      CheckInstanceIsSerializableAndAddedFields (instance1, "abc init", 8, 1);
      CheckInstanceIsSerializableAndAddedFields (instance2, "abc init", 8, 1, ctorWasCalled: true);
    }

    [Test]
    public void InstanceInitialization_PreserveCallback ()
    {
      var factory = CreateObjectFactory (
          CreateFieldAddingParticipant (), CreateInitializationAddingParticipant (), CreateCallbackImplementingParticipant ());
      var instance1 = factory.CreateObject<SerializableType> ();
      var instance2 = factory.CreateObject<CustomSerializableType> ();

      CheckInstanceIsSerializableAndAddedFields (instance1, "abc callback:False init", 8, 1);
      CheckInstanceIsSerializableAndAddedFields (instance2, "abc callback:True init", 8, 1, ctorWasCalled: true);
    }

    [Test]
    // TODO: [ExpectedException]
    public void ViaRegistry_CannotFindParticipantConfigurationForDeserialization ()
    {
      var factory = base.CreateObjectFactory (new SerializationParticipant ("key"));
      _registry.Register ("other key", factory);

      var instance = factory.CreateObject<SerializableType>();

      CheckInstanceIsSerializable (instance);
    }

    [Test]
    public void CannotSerialize ()
    {
      SkipSavingAndPeVerification ();

      var factory = CreateObjectFactory (CreateFieldAddingParticipant ());

      var message = "The underlying type implements ISerializable but GetObjectData cannot be overridden. "
                    + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual.";
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeCannotOverrideNonVirtualGetOjbectData)),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeCannotOverrideExplicitlyImplementedGetOjbectData)),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeWithoutDeserializationConstructor)),
          Throws.TypeOf<InvalidOperationException> ()
                .With.Message.EqualTo ("The underlying type implements 'ISerializable' but does not define a deserialization constructor."));
    }

    private new IObjectFactory CreateObjectFactory (params IParticipant[] participants)
    {
      var key = "participant configuration key";
      var allParticipants = participants.Concat (new SerializationParticipant (key));
      var factory = CreateObjectFactory (allParticipants, stackFramesToSkip: 1);
      _registry.Register (key, factory);

      return factory;
    }

    private void CheckInstanceIsSerializable (
        SerializableType instance,
        string expectedStringFieldValue = "abc",
        Action<SerializableType, object[]> additionalAssertionAction = null,
        object[] additonalExpectedValues = null)
    {
      Assert.That (instance.GetType ().IsSerializable, Is.True);
      instance.String = "abc";

      var memoryStream = new MemoryStream ();
      new BinaryFormatter ().Serialize (memoryStream, instance);
      memoryStream.Position = 0;

      // NO FLUSH!
      //FlushAndTrackFilesForCleanup ();
      AppDomainRunner.Run (
          args =>
          {
            var memStream = (MemoryStream) args[0];
            var expectedAssemblyQualifiedName = (string) args[1];
            var expectedFieldValue = (string) args[2];
            var additonalAssertions = (Action<SerializableType, object[]>) args[3];
            var additonalExpectedVals = (object[]) args[4];

            var deserializedInstance = (SerializableType) new BinaryFormatter ().Deserialize (memStream);

            Assert.That (deserializedInstance.GetType ().AssemblyQualifiedName, Is.EqualTo (expectedAssemblyQualifiedName));
            // TODO 5223: correct?
            //Assert.That (deserializedInstance.GetType(), Is.EqualTo (instance.GetType()));
            Assert.That (deserializedInstance.String, Is.EqualTo (expectedFieldValue));

            if (additonalAssertions != null)
              additonalAssertions (deserializedInstance, additonalExpectedVals);
          },
          memoryStream,
          instance.GetType ().AssemblyQualifiedName,
          expectedStringFieldValue,
          additionalAssertionAction,
          additonalExpectedValues);
    }

    private void CheckInstanceIsSerializableAndAddedFields (
        SerializableType instance,
        string expectedStringFieldValue = "abc",
        int expectedIntFieldValue = 7,
        int expectedSkippedIntField = 0,
        bool ctorWasCalled = false)
    {
      PrivateInvoke.SetPublicField (instance, "IntField", 7);
      PrivateInvoke.SetPublicField (instance, "SkippedIntField", 7);

      CheckInstanceIsSerializable (
          instance,
          expectedStringFieldValue,
          (deserialized, args) =>
          {
            Assert.That (deserialized.ConstructorCalled, Is.EqualTo (args[0]));
            Assert.That (PrivateInvoke.GetPublicField (deserialized, "IntField"), Is.EqualTo (args[1]));
            Assert.That (PrivateInvoke.GetPublicField (deserialized, "SkippedIntField"), Is.EqualTo (args[2]));
          },
          new object[] { ctorWasCalled, expectedIntFieldValue, expectedSkippedIntField });
    }

    private IParticipant CreateFieldAddingParticipant ()
    {
      var attributeConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute ());
      return CreateParticipant (
          mutableType =>
          {
            mutableType.AddField ("IntField", typeof (int), FieldAttributes.Public);
            mutableType.AddField ("SkippedIntField", typeof (int), FieldAttributes.Public)
                       .AddCustomAttribute (new CustomAttributeDeclaration (attributeConstructor, new object[0]));
          });
    }

    private IParticipant CreateInitializationAddingParticipant ()
    {
      return CreateParticipant (
          mutableType =>
          {
            var stringField = mutableType.GetField ("String");
            var intField = mutableType.GetField ("IntField");
            var skippedIntField = mutableType.GetField ("SkippedIntField");

            mutableType.AddInstanceInitialization (
                ctx =>
                Expression.AddAssign (Expression.Field (ctx.This, stringField), Expression.Constant (" init"), ExpressionHelper.StringConcatMethod));
            mutableType.AddInstanceInitialization (ctx => Expression.PreIncrementAssign (Expression.Field (ctx.This, intField)));
            mutableType.AddInstanceInitialization (ctx => Expression.PreIncrementAssign (Expression.Field (ctx.This, skippedIntField)));
          });
    }

    private IParticipant CreateCallbackImplementingParticipant ()
    {
      return CreateParticipant (
          mutableType =>
          {
            var stringField = mutableType.GetField ("String");
            var ctorCalledField = mutableType.GetField ("ConstructorCalled");
            var callback = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDeserializationCallback obj) => obj.OnDeserialization (null));

            mutableType.AddInterface (typeof (IDeserializationCallback));
            var method = mutableType.GetOrAddMutableMethod (callback);
            method.SetBody (
                ctx =>
                Expression.AddAssign (
                    Expression.Field (ctx.This, stringField),
                    ExpressionHelper.StringConcat (Expression.Constant (" callback:"), Expression.Field (ctx.This, ctorCalledField)),
                    ExpressionHelper.StringConcatMethod));
          });
    }

    [Serializable]
    public class SerializableType
    {
      public string String;

      [NonSerialized]
      public bool ConstructorCalled;

      public SerializableType ()
      {
        ConstructorCalled = true;
      }
    }

    [Serializable]
    public class CustomSerializableType : SerializableType, ISerializable
    {
      public CustomSerializableType () { }
      public CustomSerializableType (SerializationInfo info, StreamingContext context)
      {
        String = info.GetString ("key1");
        ConstructorCalled = true;
      }

      public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
      {
        info.AddValue ("key1", String);
      }
    }

    public class CustomSerializableTypeCannotOverrideNonVirtualGetOjbectData : ISerializable
    {
      public void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    public class CustomSerializableTypeCannotOverrideExplicitlyImplementedGetOjbectData : ISerializable
    {
      void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) { }
    }

    public class CustomSerializableTypeWithoutDeserializationConstructor : ISerializable
    {
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
  }
}