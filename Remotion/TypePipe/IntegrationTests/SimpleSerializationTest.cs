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
using Remotion.TypePipe;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [Ignore ("TODO 5217")]
  [TestFixture]
  public class SimpleSerializationTest : ObjectFactoryIntegrationTestBase
  {
    [Test]
    public void Standard_NoModifications ()
    {
      var factory = CreateObjectFactory();
      var instance = factory.CreateObject<SerializableType>();

      CheckInstanceIsSerializable (instance);
    }

    [Test]
    public void Standard_AddedFields ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant());
      var instance = factory.CreateObject<SerializableType>();

      CheckInstanceIsSerializableAndAddedFields (instance);
    }

    [Test]
    public void Custom_AddedFields ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant());
      var instance = factory.CreateObject<CustomSerializableType>();

      CheckInstanceIsSerializableAndAddedFields (instance);
    }

    [Test]
    public void InstanceInitialization ()
    {
      var factory = CreateObjectFactory (CreateFieldAddingParticipant(), CreateInitializationAddingParticipant());
      var instance1 = factory.CreateObject<SerializableType>();
      var instance2 = factory.CreateObject<CustomSerializableType>();

      CheckInstanceIsSerializableAndAddedFields (instance1, "abc def", 8, 1);
      CheckInstanceIsSerializableAndAddedFields (instance2, "abc def", 8, 1);
    }

    [Test]
    public void CannotSerialize ()
    {
      var factory = CreateObjectFactory();

      // TODO: Apply this comment to implementation to the code that deals with the first case: TODO RM-4695
      var message = "The underlying type implements ISerializable but GetObjectData cannot be overrided. "
                    + "Make sure that GetObjectData is implemented implicitly (not explicitly) and virtual";
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeCannotOverrideNonVirtualGetOjbectData)),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeCannotOverrideExplicitlyImplementedGetOjbectData)),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (message));
      Assert.That (
          () => factory.GetAssembledType (typeof (CustomSerializableTypeWithoutDeserializationConstructor)),
          Throws.TypeOf<NotSupportedException>()
                .With.Message.EqualTo ("The underlying type implements ISerializable but no deserialization constructor was found."));
    }

    private SerializableType CheckInstanceIsSerializable (SerializableType instance, string expectedStringFieldValue = "abc")
    {
      Assert.That (instance.GetType().IsSerializable, Is.True);
      instance.String = "abc";

      var memoryStream = new MemoryStream();
      var binaryFormatter = new BinaryFormatter();

      binaryFormatter.Serialize (memoryStream, instance);
      var deserializedInstance = (SerializableType) binaryFormatter.Deserialize (memoryStream);

      Assert.That (deserializedInstance.GetType(), Is.SameAs (instance.GetType()));
      Assert.That (deserializedInstance.String, Is.EqualTo (expectedStringFieldValue));

      return deserializedInstance;
    }

    private void CheckInstanceIsSerializableAndAddedFields (
        SerializableType instance, string expectedStringFieldValue = "abc", int expectedIntFieldValue = 7, int expectedSkippedIntField = 0)
    {
      PrivateInvoke.SetPublicField (instance, "IntField", 7);
      PrivateInvoke.SetPublicField (instance, "SkippedIntField", 7);

      var deserialized = CheckInstanceIsSerializable (instance, expectedStringFieldValue);

      Assert.That (PrivateInvoke.GetPublicField (deserialized, "IntField"), Is.EqualTo (expectedIntFieldValue));
      Assert.That (PrivateInvoke.GetPublicField (deserialized, "SkippedIntField"), Is.EqualTo (expectedSkippedIntField));
    }

    private IParticipant CreateFieldAddingParticipant ()
    {
      var attributeConstructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new NonSerializedAttribute());
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

            // TODO if method necessary: make ExpressionHelper.StringConcatMethod field
            mutableType.AddInstanceInitialization (ctx => Expression.AddAssign (Expression.Field (ctx.This, stringField), Expression.Constant (" def")));
            mutableType.AddInstanceInitialization (ctx => Expression.Increment (Expression.Field (ctx.This, intField)));
            mutableType.AddInstanceInitialization (ctx => Expression.Increment (Expression.Field (ctx.This, skippedIntField)));
          });
    }

    [Serializable]
    public class SerializableType
    {
      public string String;
    }

    [Serializable]
    public class CustomSerializableType : SerializableType, ISerializable
    {
      public CustomSerializableType (SerializationInfo info, StreamingContext context)
      {
        String = info.GetString ("key1");
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