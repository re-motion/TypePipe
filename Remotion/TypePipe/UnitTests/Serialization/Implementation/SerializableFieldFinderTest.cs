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
using System.Reflection;
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class SerializableFieldFinderTest
  {
    private SerializableFieldFinder _finder;

    [SetUp]
    public void SetUp ()
    {
      _finder = new SerializableFieldFinder();
    }

    [Test]
    public void GetSerializableFieldMapping_Filtering ()
    {
      var field1 = NormalizingMemberInfoFromExpressionUtility.GetField (() => StaticField);
      var field2 = NormalizingMemberInfoFromExpressionUtility.GetField (() => InstanceField);
      var field3 = NormalizingMemberInfoFromExpressionUtility.GetField (() => NonSerializedField);

      var result = _finder.GetSerializableFieldMapping (new[] { field1, field2, field3 });

      Assert.That (result, Is.EqualTo (new[] { Tuple.Create ("<tp>InstanceField", field2) }));
    }

    [Test]
    public void GetSerializableFieldMapping_SameName ()
    {
      FieldInfo field1 = CustomFieldInfoObjectMother.Create (name: "abc", type: typeof (int));
      FieldInfo field2 = CustomFieldInfoObjectMother.Create (name: "abc", type: typeof (string));

      var result = _finder.GetSerializableFieldMapping (new[] { field1, field2 });

      Assert.That (
          result,
          Is.EqualTo (
              new[]
              {
                  Tuple.Create ("<tp>" + field1.DeclaringType.FullName + "::abc@System.Int32", field1),
                  Tuple.Create ("<tp>" + field2.DeclaringType.FullName + "::abc@System.String", field2)
              }));
    }

    static readonly int StaticField = 0;
    readonly int InstanceField = 0;
    [NonSerialized]
    readonly int NonSerializedField = 0;
  }
}