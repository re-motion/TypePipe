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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.IntegrationTests.MutableReflection;
using Remotion.TypePipe.MutableReflection;

[assembly: TypePipeCustomAttributeDataTest.Abc ("assembly")]
[module: TypePipeCustomAttributeDataTest.Abc ("module")]

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataTest
  {
    private Assembly _assembly;
    private Module _module;
    private Type _type;
    private ConstructorInfo _typeInitializer;
    private FieldInfo _field;
    private ConstructorInfo _ctor;
    private MethodInfo _method;
    private ParameterInfo _returnParameter;
    private ParameterInfo _parameter;
    private PropertyInfo _property;
    private MethodInfo _getter;
    private ParameterInfo _getterReturnParameter;
    private MethodInfo _setter;
    private ParameterInfo _setterValueParameter;
    private EventInfo _event;
    private MethodInfo _eventAdder;
    private ParameterInfo _eventAdderParameter;
    private MethodInfo _eventRemover;
    private ParameterInfo _eventRemoveParameter;
    private Type _nestedType;
    private Type _genericType;

    [SetUp]
    public void SetUp ()
    {
      _assembly = GetType().Assembly;
      _module = GetType().Module;
      _type = typeof (DomainType);
      _typeInitializer = _type.TypeInitializer;
      _field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);
      _ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      _method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      _returnParameter = _method.ReturnParameter;
      _parameter = _method.GetParameters().Single();
      _property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      _getter = _property.GetGetMethod();
      _getterReturnParameter = _getter.ReturnParameter;
      _setter = _property.GetSetMethod();
      _setterValueParameter = _setter.GetParameters().Single();
      _event = typeof (DomainType).GetEvents().Single();
      _eventAdder = _event.GetAddMethod();
      _eventAdderParameter = _eventAdder.GetParameters().Single();
      _eventRemover = _event.GetRemoveMethod();
      _eventRemoveParameter = _eventRemover.GetParameters().Single();
      _nestedType = typeof (DomainType.NestedType);
      _genericType = typeof (DomainType.GenericType<>).GetGenericArguments().Single();
    }

    [Test]
    public void StandardReflection_BehavesEqually ()
    {
      CheckAbcAttributeOnly (TypePipeCustomAttributeData.GetCustomAttributes (_assembly), CustomAttributeData.GetCustomAttributes (_assembly));
      CheckAbcAttributeOnly (TypePipeCustomAttributeData.GetCustomAttributes (_module), CustomAttributeData.GetCustomAttributes (_module));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_type), CustomAttributeData.GetCustomAttributes (_type));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_typeInitializer), CustomAttributeData.GetCustomAttributes (_typeInitializer));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_field), CustomAttributeData.GetCustomAttributes (_field));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_ctor), CustomAttributeData.GetCustomAttributes (_ctor));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_method), CustomAttributeData.GetCustomAttributes (_method));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_returnParameter), CustomAttributeData.GetCustomAttributes (_returnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_parameter), CustomAttributeData.GetCustomAttributes (_parameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_property), CustomAttributeData.GetCustomAttributes (_property));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_getter), CustomAttributeData.GetCustomAttributes (_getter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_getterReturnParameter), CustomAttributeData.GetCustomAttributes (_getterReturnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_setterValueParameter), CustomAttributeData.GetCustomAttributes (_setterValueParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_setter), CustomAttributeData.GetCustomAttributes (_setter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_event), CustomAttributeData.GetCustomAttributes (_event));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_eventAdder), CustomAttributeData.GetCustomAttributes (_eventAdder));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_eventAdderParameter), CustomAttributeData.GetCustomAttributes (_eventAdderParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_eventRemover), CustomAttributeData.GetCustomAttributes (_eventRemover));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_eventRemoveParameter), CustomAttributeData.GetCustomAttributes (_eventRemoveParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_nestedType), CustomAttributeData.GetCustomAttributes (_nestedType));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (_genericType), CustomAttributeData.GetCustomAttributes (_genericType));
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
      var mutableType = MutableTypeObjectMother.Create (typeof (DomainType));
      var typeInitializer = mutableType.AddTypeInitializer (ctx => Expression.Empty());
      var field = mutableType.AddField ("_field", FieldAttributes.Private, typeof (int));
      var ctor = mutableType.AddedConstructors.Single();
      var method = mutableType.AddMethod (
          "Method", MethodAttributes.Public, typeof (int), new[] { new ParameterDeclaration (typeof (int)) }, ctx => Expression.Constant (7));
      var returnParameter = method.MutableReturnParameter;
      var parameter = method.MutableParameters.Single();
      var property = mutableType.AddProperty (
          "Property", typeof (int), new[] { new ParameterDeclaration (typeof (int)) }, MethodAttributes.Public, ctx => Expression.Default (typeof (int)), ctx => Expression.Empty());
      var event_ = mutableType.AddEvent ("Event", typeof (Action), MethodAttributes.Public, ctx => Expression.Empty(), ctx => Expression.Empty());
      // TODO 4791
      // var genericParmaeter = proxyType.MutableGenericParameter.Single();
      //var nestedType = MutableType.GetNestedTypes().Single();

      AddAbcAttribute (mutableType, "class");
      AddAbcAttribute (typeInitializer, "type initializer");
      AddAbcAttribute (field, "field");
      AddAbcAttribute (ctor, "constructor");
      AddAbcAttribute (method, "method");
      AddAbcAttribute (returnParameter, "return value");
      AddAbcAttribute (parameter, "parameter");
      AddAbcAttribute (property, "property");
      AddAbcAttribute (event_, "event");

      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (mutableType), CustomAttributeData.GetCustomAttributes (_type));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (_field));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (ctor),CustomAttributeData.GetCustomAttributes (_ctor));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (_method));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (_returnParameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (_parameter));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (property), CustomAttributeData.GetCustomAttributes (_property));
      CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (event_), CustomAttributeData.GetCustomAttributes (_event));
      //CheckAbcAttribute (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), "nested type");
      // Generic Parameters
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
      var result = GetCustomAttributeData (MethodBase.GetCurrentMethod());

      Assert.That (result.Single().NamedArguments.Select (x => x.Value), Is.EquivalentTo (new[] { "1", "2", "3" }));
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
      var result = GetCustomAttributeData (MethodBase.GetCurrentMethod());

      var attributeData = result.Single();
      Assert.That (attributeData.ConstructorArguments[0], Is.EqualTo (new[] { 1, 2, 3 }));
      Assert.That (attributeData.ConstructorArguments[1], Is.EqualTo (new[] { typeof (double), typeof (string) }));
      Assert.That (attributeData.ConstructorArguments[2], Is.EqualTo (new object[] { "s", 7, null, typeof (int), new[] { 4, 5 } }));
    }

    private void CheckAbcAttribute (IEnumerable<ICustomAttributeData> actualAttributes, IEnumerable<CustomAttributeData> expectedAttributes)
    {
      // Check value of AbcAttribute.
      var actualAbcAttribute = actualAttributes.Single();
      var expectedAbcAttribute = expectedAttributes.Single();
      Assert.That (
          actualAbcAttribute.ConstructorArguments.Single(),
          Is.Not.Null.And.EqualTo (expectedAbcAttribute.ConstructorArguments.Single().Value));
    }

    private void CheckAbcAttributeOnly (IEnumerable<ICustomAttributeData> actualAttributes, IEnumerable<CustomAttributeData> expectedAttributes)
    {
      CheckAbcAttribute (
          actualAttributes.Where (a => a.Type == typeof (AbcAttribute)),
          expectedAttributes.Where (a => a.Constructor.DeclaringType == typeof (AbcAttribute)));
    }

    private void AddAbcAttribute (IMutableInfo customAttributeTarget, string value)
    {
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (""));
      var attribute = new CustomAttributeDeclaration (ctor, new object[] { value });
      customAttributeTarget.AddCustomAttribute (attribute);
    }

    private IEnumerable<ICustomAttributeData> GetCustomAttributeData (MemberInfo member)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (member).Where (a => a.Type != typeof (TestAttribute));
    }

    [Abc ("class")]
    public class DomainType
    {
      [Abc ("type initializer")]
      static DomainType () { }

      [Abc ("field")]
      public string Field;

      [Abc ("constructor")]
      public DomainType () { }

      [Abc ("method")]
      [return: Abc ("return value")]
      public virtual void Method ([Abc ("parameter")] int p) { Dev.Null = p; }

      [Abc ("property")]
      public string Property
      {
        [Abc ("getter")]
        [return: Abc ("getter return value")]
        get { return Field; }

        [Abc ("setter")]
        [param: Abc ("setter value parameter")]
        set { Field = value; }
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

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
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

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleCtorsAttribute : Attribute
    {
      public MultipleCtorsAttribute () { }
      public MultipleCtorsAttribute (string constructorArgument) { Dev.Null = constructorArgument; }
    }

    public class ComplexArgumentsAttribute : Attribute
    {
      public ComplexArgumentsAttribute (int[] intArray, Type[] typeArray, object obj) { Dev.Null = intArray; Dev.Null = typeArray; Dev.Null = obj; }
    }
  }
}