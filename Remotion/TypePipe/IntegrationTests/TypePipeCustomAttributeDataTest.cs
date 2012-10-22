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
      var setterValueParameter = setter.GetParameters().Single();
      var @event = typeof (DomainType).GetEvents().Single();
      var eventAdder = @event.GetAddMethod();
      var eventAdderParameter = eventAdder.GetParameters().Single();
      var eventRemover = @event.GetRemoveMethod();
      var eventRemoveParameter = eventRemover.GetParameters().Single();
      var nestedType = typeof (DomainType.NestedType);
      var genericType = typeof (DomainType.GenericType<>).GetGenericArguments().Single();

      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (type), CustomAttributeData.GetCustomAttributes (type));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (field));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (ctor), CustomAttributeData.GetCustomAttributes (ctor));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (method));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (returnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (parameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (property), CustomAttributeData.GetCustomAttributes (property));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (getter), CustomAttributeData.GetCustomAttributes (getter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (getterReturnParameter), CustomAttributeData.GetCustomAttributes (getterReturnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (setterValueParameter), CustomAttributeData.GetCustomAttributes (setterValueParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (setter), CustomAttributeData.GetCustomAttributes (setter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (@event), CustomAttributeData.GetCustomAttributes (@event));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (eventAdder), CustomAttributeData.GetCustomAttributes (eventAdder));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (eventAdderParameter), CustomAttributeData.GetCustomAttributes (eventAdderParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (eventRemover), CustomAttributeData.GetCustomAttributes (eventRemover));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (eventRemoveParameter), CustomAttributeData.GetCustomAttributes (eventRemoveParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), CustomAttributeData.GetCustomAttributes (nestedType));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (genericType), CustomAttributeData.GetCustomAttributes (genericType));
    }

    [Test]
    public void InheritedMethod_FromObject ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ToString());
      Assert.That (() => TypePipeCustomAttributeData.GetCustomAttributes (method, true), Throws.Nothing);
    }

    [Test]
    public void MutableReflection ()
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
      // TODO 4791
      // setter value parameter, Adder (+ parameter), Remover (+ parameter), generic type, Invoker?

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
      // setter value parameter
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (@event), "event");
      // Event Adder
      // Event Adder Parameter
      // Event Remover
      // Event Remover Parameter
      // Invoker?
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), "nested type");
      // Generic Type
    }

    [Test]
    [Multiple ("3"), Multiple ("1"), Multiple ("2")]
    public void MultipleAttributes ()
    {
      var result = GetCustomAttributeData (MethodBase.GetCurrentMethod());

      Assert.That (result.Select (x => x.ConstructorArguments.Single()), Is.EquivalentTo (new[] { "1", "2", "3" }));
    }

    [Test]
    [NamedArguments (NamedArgument3 = "3", NamedArgument1 = "1", NamedArgument2 = "2")]
    public void NamedArguments ()
    {
      var attributeData = GetCustomAttributeData (MethodBase.GetCurrentMethod()).Single();

      Assert.That (attributeData.NamedArguments.Select (x => x.Value), Is.EquivalentTo (new[] { "1", "2", "3" }));
    }

    [Test]
    [MultipleCtors ("other ctor"), MultipleCtors]
    public void CorrectCtor ()
    {
      var defaultCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new MultipleCtorsAttribute ());
      var otherCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new MultipleCtorsAttribute (""));

      var result = GetCustomAttributeData (MethodBase.GetCurrentMethod()).ToArray();

      var data1 = result.Single (x => x.ConstructorArguments.Count == 0);
      var data2 = result.Single (x => x.ConstructorArguments.Count == 1);
      Assert.That (data1.Constructor, Is.EqualTo (defaultCtor));
      Assert.That (data2.Constructor, Is.EqualTo (otherCtor));
    }

    [Test]
    [ComplexArguments (new[] { 1, 2, 3 }, new[] { typeof (double), typeof (string) }, new object[] { "s", 7, null, typeof (int), new[] { 4, 5 } })]
    public void WithComplexArguments ()
    {
      var attributeData = GetCustomAttributeData (MethodBase.GetCurrentMethod()).Single();

      Assert.That (attributeData.ConstructorArguments[0], Is.EqualTo (new[] { 1, 2, 3 }));
      Assert.That (attributeData.ConstructorArguments[1], Is.EqualTo (new[] { typeof (double), typeof (string) }));
      Assert.That (attributeData.ConstructorArguments[2], Is.EqualTo (new object[] { "s", 7, null, typeof (int), new[] { 4, 5 } }));
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

    private IEnumerable<ICustomAttributeData> GetCustomAttributeData (MemberInfo member)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (member).Where (a => a.Type != typeof (TestAttribute));
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
        [param: Abc ("setter value parameter")]
        set { field = value; }
      }

      [Abc ("event")]
      public event Action<string> Action
      {
        [Abc ("event adder")]
        [param: Abc ("event adder parameter")]
        add { throw new NotImplementedException(); }

        [Abc ("event remover")]
        [param: Abc ("event remover parameter")]
        remove { throw new NotImplementedException(); }
      }

      [Abc ("nested type")]
      public class NestedType {}

// ReSharper disable UnusedTypeParameter
      public class GenericType<[Abc("type parameter")] T> { }
// ReSharper restore UnusedTypeParameter
    }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
    }

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleAttribute : Attribute
    {
      public MultipleAttribute (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
    }

    public class NamedArgumentsAttribute : Attribute
    {
      public string NamedArgument1 { get; set; }
      public string NamedArgument2 { get; set; }
      public string NamedArgument3 { get; set; }
    }

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleCtorsAttribute : Attribute
    {
      public MultipleCtorsAttribute () { }
// ReSharper disable UnusedParameter.Local
      public MultipleCtorsAttribute (string constructorArgument) { }
// ReSharper restore UnusedParameter.Local
    }

    public class ComplexArgumentsAttribute : Attribute
    {
// ReSharper disable UnusedParameter.Local
      public ComplexArgumentsAttribute (int[] intArray, Type[] typeArray, object obj) { }
// ReSharper restore UnusedParameter.Local
    }
  }
}