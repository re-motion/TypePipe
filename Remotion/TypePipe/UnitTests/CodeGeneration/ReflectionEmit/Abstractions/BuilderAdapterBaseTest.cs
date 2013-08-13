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
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.Abstractions
{
  [TestFixture]
  public class BuilderAdapterBaseTest
  {
    [Test]
    public void SetCustomAttribute ()
    {
      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((AbcAttribute obj) => obj.StringProperty);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((AbcAttribute obj) => obj.IntField);

      var wasCalled = false;
      Action<CustomAttributeBuilder> setCustomAttributeMethod = customAttributeBuilder =>
      {
        wasCalled = true;
        Assert.That (customAttributeBuilder, Is.Not.Null);

        CheckCustomAttributeBuilder (
            customAttributeBuilder,
            attributeCtor,
            new object[] { typeof (int) },
            new[] { property },
            new object[] { "def" },
            new[] { field },
            new object[] { 8 });
      };
      var adapterBasePartialMock = MockRepository.GeneratePartialMock<BuilderAdapterBase> (setCustomAttributeMethod);
      var declaration = new CustomAttributeDeclaration (
          attributeCtor, new object[] { typeof (int) }, new NamedArgumentDeclaration (property, "def"), new NamedArgumentDeclaration (field, 8));

      adapterBasePartialMock.SetCustomAttribute (declaration);

      Assert.That (wasCalled, Is.True);
    }

    private void CheckCustomAttributeBuilder (
        CustomAttributeBuilder builder,
        ConstructorInfo expectedCtor,
        object[] expectedCtorArgs,
        PropertyInfo[] expectedPropertyInfos,
        object[] expectedPropertyValues,
        FieldInfo[] expectedFieldInfos,
        object[] expectedFieldValues)
    {
      var actualConstructor = (ConstructorInfo) PrivateInvoke.GetNonPublicField (builder, "m_con");
      var actualConstructorArgs = (object[]) PrivateInvoke.GetNonPublicField (builder, "m_constructorArgs");
      var actualBlob = (byte[]) PrivateInvoke.GetNonPublicField (builder, "m_blob");

      Assert.That (actualConstructor, Is.SameAs (expectedCtor));
      Assert.That (actualConstructorArgs, Is.EqualTo (expectedCtorArgs));

      var testBuilder = new CustomAttributeBuilder (
          expectedCtor, expectedCtorArgs, expectedPropertyInfos, expectedPropertyValues, expectedFieldInfos, expectedFieldValues);
      var expectedBlob = (byte[]) PrivateInvoke.GetNonPublicField (testBuilder, "m_blob");
      Assert.That (actualBlob, Is.EqualTo (expectedBlob));
    }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute (object ctorArg ) { Dev.Null = ctorArg; }

      public int IntField;
      public string StringProperty { get; set; }
    }
  }
}