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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class FieldDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var name = "Field";
      var attributes = FieldAttributes.Static | FieldAttributes.Family;
      var fieldType = ReflectionObjectMother.GetSomeType();

      var descriptor = FieldDescriptor.Create (fieldType, name, attributes);

      Assert.That (descriptor.UnderlyingSystemInfo, Is.Null);
      Assert.That (descriptor.Name, Is.EqualTo (name));
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.Type, Is.SameAs (fieldType));
      Assert.That (descriptor.CustomAttributeDataProvider.Invoke(), Is.Empty);
    }

    [Test]
    public void Create_ForExisting ()
    {
      var underlyingField = NormalizingMemberInfoFromExpressionUtility.GetField ((FieldDescriptorTest obj) => obj._testField);

      var descriptor = FieldDescriptor.Create (underlyingField);

      Assert.That (descriptor.UnderlyingSystemInfo, Is.SameAs (underlyingField));
      Assert.That (descriptor.Name, Is.EqualTo ("_testField"));
      Assert.That (descriptor.Attributes, Is.EqualTo (FieldAttributes.Private));
      Assert.That (descriptor.Type, Is.SameAs (typeof(int)));

      Assert.That (
          descriptor.CustomAttributeDataProvider.Invoke ().Select (ad => ad.Type),
          Is.EquivalentTo (new[] { typeof (AbcAttribute), typeof (DefAttribute) }));
    }

    [Abc, Def]
    private int _testField = 7;

    public class AbcAttribute : Attribute { }
    public class DefAttribute : Attribute { }
  }
}