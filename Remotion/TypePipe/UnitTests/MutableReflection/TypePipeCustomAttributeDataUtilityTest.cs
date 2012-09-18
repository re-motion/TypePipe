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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataUtilityTest
  {
    [Test]
    public void Create_FromDeclaration ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute (new object(), "", 7));
      var ctorArguments = new object[] { null, "ctor", 7 };
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);
      var namedArguments = new[]
                           {
                               new NamedAttributeArgumentDeclaration (field, null),
                               new NamedAttributeArgumentDeclaration (property, "named")
                           };
      var declaration = new CustomAttributeDeclaration (constructor, ctorArguments, namedArguments);

      var result = TypePipeCustomAttributeDataUtility.Create (declaration);

      CheckTypePipeCustomAttributeData (result);
    }

    [Test]
    [Domain (null, "ctor", 7, Field = null, Property = "named")]
    public void Create_FromCustomAttributeData ()
    {
      var method = MethodBase.GetCurrentMethod();
      var customAttributeData = CustomAttributeData.GetCustomAttributes (method).Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute));

      var result = TypePipeCustomAttributeDataUtility.Create (customAttributeData);

      CheckTypePipeCustomAttributeData (result);
    }

    [Test]
    [Domain (7, "7", 7, Field = "field")]
    public void ReflectionBehavior_AssignableType ()
    {
      var method = MethodBase.GetCurrentMethod ();
      var customAttributeData = CustomAttributeData.GetCustomAttributes (method).Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute));

      Assert.That (customAttributeData.ConstructorArguments[0].ArgumentType, Is.SameAs (typeof (int)));
      Assert.That (customAttributeData.ConstructorArguments[1].ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (customAttributeData.NamedArguments.Single().TypedValue.ArgumentType, Is.SameAs (typeof (string)));
    }

    [Test]
    [Domain ((object) null, null, 7, Field = null)]
    public void ReflectionBehavior_Null ()
    {
      var method = MethodBase.GetCurrentMethod ();
      var customAttributeData = CustomAttributeData.GetCustomAttributes (method).Single (a => a.Constructor.DeclaringType == typeof (DomainAttribute));

      Assert.That (customAttributeData.ConstructorArguments[0].ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (customAttributeData.ConstructorArguments[1].ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (customAttributeData.NamedArguments.Single().TypedValue.ArgumentType, Is.SameAs (typeof (string)));
    }

    // Reflection behavior is very 'specific'.
    // ArgumentType == value.GetType()
    // If value == null then ArgumentType == typeof (string)
    private void CheckTypePipeCustomAttributeData (TypePipeCustomAttributeData result)
    {
      var expectedCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainAttribute (new object (), "", 7));
      var exptectedField = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainAttribute obj) => obj.Field);
      var exptectedProperty = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainAttribute obj) => obj.Property);

      Assert.That (result.Constructor, Is.EqualTo (expectedCtor));
      Assert.That (result.ConstructorArguments, Has.Count.EqualTo (3));
      Assert.That (result.ConstructorArguments[0].ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (result.ConstructorArguments[0].Value, Is.Null);
      Assert.That (result.ConstructorArguments[1].ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (result.ConstructorArguments[1].Value, Is.EqualTo ("ctor"));
      Assert.That (result.ConstructorArguments[2].ArgumentType, Is.SameAs (typeof (int)));
      Assert.That (result.ConstructorArguments[2].Value, Is.EqualTo (7));
      Assert.That (result.NamedArguments, Has.Count.EqualTo (2));
      Assert.That (result.NamedArguments[0].MemberInfo, Is.EqualTo (exptectedField));
      Assert.That (result.NamedArguments[0].TypedValue.ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (result.NamedArguments[0].TypedValue.Value, Is.Null);
      Assert.That (result.NamedArguments[1].MemberInfo, Is.EqualTo (exptectedProperty));
      Assert.That (result.NamedArguments[1].TypedValue.ArgumentType, Is.SameAs (typeof (string)));
      Assert.That (result.NamedArguments[1].TypedValue.Value, Is.EqualTo ("named"));
    }

    public class DomainAttribute : Attribute
    {
      public object Field;

      public DomainAttribute (object ctorArgument1, string ctorArgument2, int ctorArgument3)
      {
        Field = ctorArgument1;
        Property = ctorArgument2;
        Dev.Null = ctorArgument3;
      }

      public string Property { get; set; }
    }
  }
}