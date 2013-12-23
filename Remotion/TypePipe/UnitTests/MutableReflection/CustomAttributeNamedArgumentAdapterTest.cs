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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomAttributeNamedArgumentAdapterTest
  {
    [Test]
    [Domain (Property = "named arg")]
    public void Initialization_Simple ()
    {
      var namedArgument = CustomAttributeNamedArgument (MethodBase.GetCurrentMethod ());

      var result = new CustomAttributeNamedArgumentAdapter (namedArgument);

      var member = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);
      Assert.That (result.MemberInfo, Is.EqualTo (member));
      Assert.That (result.MemberType, Is.EqualTo (typeof (string)));
      Assert.That (result.Value, Is.EqualTo ("named arg"));
    }

    [Test]
    [Domain (Field = new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } })]
    public void Initialization_Complex ()
    {
      var namedArgument = CustomAttributeNamedArgument (MethodBase.GetCurrentMethod());

      var result = new CustomAttributeNamedArgumentAdapter (namedArgument);

      var member = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);
      Assert.That (result.MemberInfo, Is.EqualTo (member));
      Assert.That (result.MemberType, Is.EqualTo (typeof (object)));
      Assert.That (result.Value, Is.EqualTo (new object[] { "s", 7, null, typeof (double), MyEnum.B, new[] { 4, 5 } }));
    }

    private static CustomAttributeNamedArgument CustomAttributeNamedArgument (MethodBase testMethod)
    {
      return CustomAttributeData.GetCustomAttributes (testMethod)
          .Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute))
          .NamedArguments.Single();
    }

    private class DomainAttribute : Attribute
    {
      public object Field;

      public DomainAttribute ()
      {
        Dev.Null = Field;
      }

      public string Property { get; set; }

      public string Method ()
      {
        return null;
      }
    }

    private enum MyEnum { A, B, C }
  }
}