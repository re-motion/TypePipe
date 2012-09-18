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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataUtilityTest
  {
    [Test]
    public void Create_FromDeclaration ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute ("", 7));
      var ctorArguments = new object[] { null, 7 };
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);
      var namedArguments = new[]
                           {
                               new NamedAttributeArgumentDeclaration (field, 7),
                               new NamedAttributeArgumentDeclaration (property, "blubprop")
                           };

      var declaration = new CustomAttributeDeclaration (constructor, ctorArguments, namedArguments);

      var result = TypePipeCustomAttributeDataUtility.Create (declaration);

      Assert.That (result.Constructor, Is.SameAs (declaration.Constructor));
      Assert.That (result.ConstructorArguments[0].ArgumentType, Is.EqualTo(typeof(string)));
      Assert.That (result.ConstructorArguments[0].Value, Is.Null);
      Assert.That (result.ConstructorArguments[1].ArgumentType, Is.EqualTo (typeof (int)));
      Assert.That (result.ConstructorArguments[1].Value, Is.EqualTo (7));
      Assert.That (result.NamedArguments[0].MemberInfo, Is.SameAs (field));
      Assert.That (result.NamedArguments[0].TypedValue.ArgumentType, Is.EqualTo (typeof (object)));
      Assert.That (result.NamedArguments[0].TypedValue.Value, Is.EqualTo (7));
      Assert.That (result.NamedArguments[1].MemberInfo, Is.SameAs (property));
      Assert.That (result.NamedArguments[1].TypedValue.ArgumentType, Is.EqualTo (typeof (string)));
      Assert.That (result.NamedArguments[1].TypedValue.Value, Is.EqualTo ("blubprop"));
    }

    public class DomainAttribute : Attribute
    {
      public object Field;

      public DomainAttribute (string ctorArgument1, int ctorArgument2)
      {
        Field = ctorArgument1;
        Property = ctorArgument2.ToString();
      }

      public string Property { get; set; }
    }
  }
}