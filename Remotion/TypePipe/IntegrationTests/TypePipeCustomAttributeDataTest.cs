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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class TypePipeCustomAttributeDataTest
  {
    [Test]
    public void StandardReflection ()
    {
      var type = typeof (DomainType);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.field);
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      var returnParameter = method.ReturnParameter;
      var parameter = method.GetParameters().Single();
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      var getter = property.GetGetMethod();
      var getterReturnParameter = getter.ReturnParameter;
      var setter = property.GetSetMethod();
      var @event = typeof (DomainType).GetEvents().Single();
      var nestedType = typeof (DomainType.NestedType);

      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (type), CustomAttributeData.GetCustomAttributes (type));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (field));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (ctor), CustomAttributeData.GetCustomAttributes (ctor));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (method));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (returnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (parameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (property), CustomAttributeData.GetCustomAttributes (property));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (getter), CustomAttributeData.GetCustomAttributes (getter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (getterReturnParameter), CustomAttributeData.GetCustomAttributes (getterReturnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (setter), CustomAttributeData.GetCustomAttributes (setter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (@event), CustomAttributeData.GetCustomAttributes (@event));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), CustomAttributeData.GetCustomAttributes (nestedType));
    }

    [Test]
    public void MutableReflection_Normal ()
    {
      var descriptor = UnderlyingTypeDescriptor.Create (typeof (DomainType));
      var mutableType = new MutableType (descriptor, new MemberSelector (new BindingFlagsEvaluator()), new RelatedMethodFinder());
      var field = mutableType.AllMutableFields.Single ();
      var constructor = mutableType.AllMutableConstructors.Single ();
      var method = mutableType.AllMutableMethods.Single (x => x.Name == "Method");
      // TODO 4793
      //var returnParameter = method.ReturnParameter;
      var parameter = (MutableParameterInfo) method.GetParameters ().Single ();
      // TODO 4791
      //var property = mutableType.GetProperties().Single();
      // TODO 4791
      //var getter = property.GetGetMethod();
      // TODO 4791
      //var getterReturnParameter = getter.ReturnParameter;
      // TODO 4791
      //var setter = property.GetGetMethod();
      // TODO 4791
      //var @event = mutableType.GetEvents().Single();
      // TODO 4791
      //var nestedType = mutableType.GetNestedTypes().Single();

      CheckAbcAttribute (
          TypePipeCustomAttributeData.GetCustomAttributes (mutableType), CustomAttributeData.GetCustomAttributes (mutableType.UnderlyingSystemType));
      CheckAbcAttribute (
          TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (field.UnderlyingSystemFieldInfo));
      CheckAbcAttribute (
          TypePipeCustomAttributeData.GetCustomAttributes (constructor),CustomAttributeData.GetCustomAttributes (constructor.UnderlyingSystemConstructorInfo));
      CheckAbcAttribute (
          TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (method.UnderlyingSystemMethodInfo));
      //CheckAbcAttribute (
      //    TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (returnParameter.UnderlyingParameterInfo));
      CheckAbcAttribute (
          TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (parameter.UnderlyingSystemParameterInfo));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (property), "property");    
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (getter), "getter");
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (getterReturnParameter), "getter return value");
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (setter), "setter");
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (@event), "event");
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), "nested type");
    }

    [Test]
    public void MutableReflection_MultipleAttribute ()
    {
      var result = TypePipeCustomAttributeData.GetCustomAttributes (typeof (DomainTypeWithComplexAttributes));

      var filteredResult = result.Where (x => x.Constructor.DeclaringType == typeof (MultipleAttribute))
          .Select (x => x.ConstructorArguments.Single());
      Assert.That (filteredResult, Is.EquivalentTo (new[] { "1", "2", "3" }));
    }

    [Test]
    public void MutableReflection_NamedArguments ()
    {
      var result = TypePipeCustomAttributeData.GetCustomAttributes (typeof (DomainTypeWithComplexAttributes));

      var filteredResult = result.Single (x => x.Constructor.DeclaringType == typeof (WithNamedArgumentsAttribute))
          .NamedArguments.Select (x => x.Value);
      Assert.That (filteredResult, Is.EquivalentTo (new[] { "1", "2", "3" }));
    }

    [Test]
    public void MutableReflection_SelectCorrectCtor ()
    {
      var defaultCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new WithMultipleCtorsAttribute ());
      var otherCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new WithMultipleCtorsAttribute (""));

      var result = TypePipeCustomAttributeData.GetCustomAttributes (typeof (DomainTypeWithComplexAttributes));

      var filteredResult = result.Where (x => x.Constructor.DeclaringType == typeof (WithMultipleCtorsAttribute));
      var attribute1 = filteredResult.Single (x => x.ConstructorArguments.Count == 0);
      var attribute2 = filteredResult.Single (x => x.ConstructorArguments.Count == 1);
      Assert.That (attribute1.Constructor, Is.EqualTo (defaultCtor));
      Assert.That (attribute2.Constructor, Is.EqualTo (otherCtor));
    }

    [Test]
    public void MutableReflection_WithComplexArguments ()
    {
      var result = TypePipeCustomAttributeData.GetCustomAttributes (typeof (DomainTypeWithComplexAttributes));

      var filteredResult = result.Single (x => x.Constructor.DeclaringType == typeof (WithComplexArgumentsAttribute));
      Assert.That (filteredResult.ConstructorArguments[0], Is.EqualTo (new[] { 1, 2, 3 }));
      Assert.That (filteredResult.ConstructorArguments[1], Is.EqualTo (new[] { typeof (double), typeof (string) }));
      Assert.That (filteredResult.ConstructorArguments[2], Is.EqualTo (new object[] { "s", 7, null, typeof (int), new[] { 4, 5 } }));
    }

    private void CheckAbcAttribute (IEnumerable<ICustomAttributeData> actualAttributes, IEnumerable<CustomAttributeData> expectedAttributes)
    {
      // Check value of AbcAttribute
      var actualAbcAttribute = actualAttributes.Single();
      var expectedAbcAttribute = expectedAttributes.Single();
      Assert.That (
          actualAbcAttribute.ConstructorArguments.Single(),
          Is.Not.Null.And.EqualTo (expectedAbcAttribute.ConstructorArguments.Single().Value));
    }

    [Abc ("class")]
    public class DomainType
    {
      [Abc ("field")]
      public string field;

      [Abc ("constructor")]
      public DomainType ()
      {
      }

      [Abc ("method")]
      [return: Abc ("return value")]
      public virtual void Method ([Abc ("parameter")] int p)
      {
      }

      [Abc ("property")]
      public string Property
      {
        [Abc ("getter")]
        [return: Abc ("getter return value")]
        get { return field; }

        [Abc ("setter")]
        // Annotate parameter?
        set { field = value; }
      }

      [Abc ("event")]
      public event Action<string> Action
      {
        [Abc ("event adder")]
        // Annotate parameter?
        add { throw new NotImplementedException(); }
        [Abc ("event remover")]
        // Annotate parameter?
        remove { throw new NotImplementedException(); }
      }

      [Abc ("nested type")]
      public class NestedType {}
    }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
    }

    // Order of attributes is not defined
    [Multiple ("3"), Multiple ("1"), Multiple ("2")]
    // Order of named arguments is not defined
    [WithNamedArguments (NamedArgument3 = "3", NamedArgument1 = "1", NamedArgument2 = "2")]
    // Select correct ctor
    [WithMultipleCtors ("other ctor"), WithMultipleCtors]
    // Complex arguments
    [WithComplexArguments (new[] { 1, 2, 3 }, new[] { typeof (double), typeof (string) }, new object[] { "s", 7, null, typeof (int), new[] { 4, 5 } })]
    public class DomainTypeWithComplexAttributes { }

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleAttribute : Attribute
    {
      public MultipleAttribute (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
    }

    public class WithNamedArgumentsAttribute : Attribute
    {
      public string NamedArgument1 { get; set; }
      public string NamedArgument2 { get; set; }
      public string NamedArgument3 { get; set; }
    }

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class WithMultipleCtorsAttribute : Attribute
    {
      public WithMultipleCtorsAttribute () { }
// ReSharper disable UnusedParameter.Local
      public WithMultipleCtorsAttribute (string constructorArgument) { }
// ReSharper restore UnusedParameter.Local
    }

    public class WithComplexArgumentsAttribute : Attribute
    {
// ReSharper disable UnusedParameter.Local
      public WithComplexArgumentsAttribute (int[] intArray, Type[] typeArray, object obj) { }
// ReSharper restore UnusedParameter.Local
    }
  }
}