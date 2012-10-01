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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomAttributeDataAdapterTest
  {
    [Test]
    [Domain ("ctor", 7, Property = "prop", Field = typeof (double))]
    public void Initialization_Simple ()
    {
      var customAttributeData = GetCustomAttributeData (MethodBase.GetCurrentMethod());

      var result = new CustomAttributeDataAdapter (customAttributeData);

      Assert.That (result.Constructor, Is.SameAs (customAttributeData.Constructor));
      Assert.That (result.ConstructorArguments, Is.EqualTo (new object[] { "ctor", 7 }));
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
    [Domain (new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } }, 0)]
    public void Initialization_Complex ()
    {
      var customAttributeData = GetCustomAttributeData (MethodBase.GetCurrentMethod());

      var result = new CustomAttributeDataAdapter (customAttributeData);

      Assert.That (result.ConstructorArguments[0], Is.EqualTo (new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } }));
    }

    private CustomAttributeData GetCustomAttributeData (MethodBase testMethod)
    {
      return CustomAttributeData.GetCustomAttributes (testMethod).Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute));
    }

    private class DomainAttribute : Attribute
    {
      public object Field;

      public DomainAttribute (object ctorArgument1, int ctorArgument2)
      {
        Dev.Null = ctorArgument1;
        Dev.Null = ctorArgument2;
        Dev.Null = Field;
        Dev.Null = Property;
      }

      public string Property { get; set; }
    }

    private enum MyEnum { A, B, C }
  }
}