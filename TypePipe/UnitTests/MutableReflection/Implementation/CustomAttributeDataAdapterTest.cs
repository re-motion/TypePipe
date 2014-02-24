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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomAttributeDataAdapterTest
  {
    [Test]
    [Domain (null)]
    public void Type ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod ());

      Assert.That (result.Type, Is.SameAs (typeof (DomainAttribute)));
    }

    [Test]
    [Domain (null)]
    public void Constructor ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod());

      var expected = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute (null));
      Assert.That (result.Constructor, Is.EqualTo (expected));
    }

    [Test]
    [Domain (typeof (string), 7, "abc")]
    public void ConstructorArguments_Simple ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod());

      Assert.That (result.ConstructorArguments, Is.EqualTo (new object[] { typeof (string), 7 , "abc"}));
    }

    [Test]
    [Domain (new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } })]
    public void ConstructorArguments_Complex ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod());

      Assert.That (result.ConstructorArguments[0], Is.EqualTo (new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } }));
    }

    [Test]
    [Domain (null, Property = "prop", Field = typeof (double))]
    public void NamedArguments ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod());

      Assert.That (result.NamedArguments, Has.Count.EqualTo (2));
      Assert.That (
          result.NamedArguments,
          Has.Some.Matches<ICustomAttributeNamedArgument> (
              x => x.MemberInfo.Name == "Property" && x.MemberType == typeof (string) && x.Value.Equals ("prop")));
      Assert.That (
          result.NamedArguments,
          Has.Some.Matches<ICustomAttributeNamedArgument> (
              x => x.MemberInfo.Name == "Field" && x.MemberType == typeof (object) && x.Value.Equals (typeof (double))));
    }

    [Test]
    [Domain (new object[] { 1, new[] { 2, 3 } }, Field = new object[] { new[] { 4 }, 5, 6 })]
    public void PropertiesCreateNewInstances ()
    {
      var result = GetCustomAttributeDataAdapter (MethodBase.GetCurrentMethod());

      Assert.That (result.ConstructorArguments, Is.Not.SameAs (result.ConstructorArguments));
      Assert.That (result.ConstructorArguments.Single(), Is.Not.SameAs (result.ConstructorArguments.Single()));
      Assert.That (((object[]) result.ConstructorArguments.Single())[1], Is.Not.SameAs (((object[]) result.ConstructorArguments.Single())[1]));

      Assert.That (result.NamedArguments, Is.Not.SameAs (result.NamedArguments));
      Assert.That (result.NamedArguments.Single().Value, Is.Not.SameAs (result.NamedArguments.Single()));
      Assert.That (((object[]) result.NamedArguments.Single().Value)[0], Is.Not.SameAs (((object[]) result.NamedArguments.Single().Value)[0]));
    }

    private CustomAttributeDataAdapter GetCustomAttributeDataAdapter (MethodBase testMethod)
    {
      var customAttributeData = CustomAttributeData.GetCustomAttributes (testMethod).Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute));
      return new CustomAttributeDataAdapter (customAttributeData);
    }

    private class DomainAttribute : Attribute
    {
      public object Field;

      public DomainAttribute (object ctorArgument)
      {
        Dev.Null = ctorArgument;
      }

      public DomainAttribute (Type ctorArgument1, int ctorArgument2, object ctorArgument3)
      {
        Dev.Null = ctorArgument1;
        Dev.Null = ctorArgument2;
        Dev.Null = ctorArgument3;
        Dev.Null = Field;
        Dev.Null = Property;
      }

      public string Property { get; set; }
    }

    private enum MyEnum { A, B, C }
  }
}