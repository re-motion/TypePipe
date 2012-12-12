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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class SerializationParticipantTest
  {
    private const string c_factoryIdentifier = "expectedObjectFactory identifier";

    private SerializationParticipant _participant;

    [SetUp]
    public void SetUp ()
    {
      _participant = new SerializationParticipant (c_factoryIdentifier);
    }

    [Test]
    public void PartialCacheKeyProvider ()
    {
      Assert.That (_participant.PartialCacheKeyProvider, Is.Null);
    }

    [Test]
    public void ModifyType_SerializableType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableType));

      _participant.ModifyType (mutableType);

      Assert.That (mutableType.AddedInterfaces, Is.EqualTo (new[] { typeof (ISerializable) }));
      Assert.That (mutableType.AllMutableMethods.Count(), Is.EqualTo (1));
      Assert.That (mutableType.AddedConstructors, Is.Empty);
      var method = mutableType.AddedMethods.Single();

      var serializationInfo = method.ParameterExpressions[0];
      var expectedMethodBody = Expression.Block (
          typeof (void),
          Expression.Call (serializationInfo, "SetType", Type.EmptyTypes, Expression.Constant (typeof (ObjectWithoutDeserializationConstructorProxy))),
          Expression.Call (
              serializationInfo,
              "AddValue",
              Type.EmptyTypes,
              Expression.Constant ("<tp>underlyingType"),
              Expression.Constant (typeof (SerializableType).AssemblyQualifiedName)),
          Expression.Call (
              serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>factoryIdentifier"), Expression.Constant (c_factoryIdentifier)),
          Expression.Call (
              typeof (ReflectionSerializationHelper), "AddFieldValues", Type.EmptyTypes, serializationInfo, new ThisExpression (mutableType)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedMethodBody, method.Body);
    }

    [Test]
    public void ModifyType_SerializableInterfaceType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (SerializableInterfaceType));
      var method = mutableType.ExistingMutableMethods.Single();
      var oldBody = method.Body;

      _participant.ModifyType (mutableType);

      Assert.That (mutableType.AddedInterfaces, Is.Empty);
      Assert.That (mutableType.AddedMethods, Is.Empty);

      var serializationInfo = method.ParameterExpressions[0];
      var expectedBody = Expression.Block (
          oldBody,
          Expression.Call (serializationInfo, "SetType", Type.EmptyTypes, Expression.Constant (typeof (ObjectWithDeserializationConstructorProxy))),
          Expression.Call (
              serializationInfo,
              "AddValue",
              Type.EmptyTypes,
              Expression.Constant ("<tp>underlyingType"),
              Expression.Constant (typeof (SerializableInterfaceType).AssemblyQualifiedName)),
          Expression.Call (
              serializationInfo, "AddValue", Type.EmptyTypes, Expression.Constant ("<tp>factoryIdentifier"), Expression.Constant (c_factoryIdentifier)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, method.Body);
    }

    [Test]
    public void ModifyType_SomeType ()
    {
      var mutableType = MutableTypeObjectMother.CreateForExisting (typeof (SomeType));

      _participant.ModifyType (mutableType);

      Assert.That (mutableType.AddedInterfaces, Is.Empty);
      Assert.That (mutableType.AllMutableMethods, Is.Empty);
    }

    public class SomeType { }

    [Serializable]
    public class SerializableType { }

    [Serializable]
    class SerializableInterfaceType : ISerializable
    {
      public virtual void GetObjectData (SerializationInfo info, StreamingContext context) { }
    }
  }
}