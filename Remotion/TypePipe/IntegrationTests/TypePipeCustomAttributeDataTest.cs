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
using Remotion.FunctionalProgramming;

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class TypePipeCustomAttributeDataTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void TypePipeCustomAttributeData_StandardReflection ()
    {
      var type = typeof (DomainType);
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.field);
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));
      var returnParameter = method.ReturnParameter;
      var parameter = method.GetParameters().Single();
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);
      var @event = typeof (DomainType).GetEvents().Single();
      var nestedType = typeof (DomainType.NestedType);

      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (type), CustomAttributeData.GetCustomAttributes (type));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (field));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (ctor), CustomAttributeData.GetCustomAttributes (ctor));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (method));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (returnParameter));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (parameter));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (property), CustomAttributeData.GetCustomAttributes (property));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (@event), CustomAttributeData.GetCustomAttributes (@event));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), CustomAttributeData.GetCustomAttributes (nestedType));
    }

    [Test]
    public void TypePipeCustomAttributeData_MutableReflection ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (mutableType), "class");

            var field = mutableType.AllMutableFields.Single();
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (field), "field");

            var constructor = mutableType.AllMutableConstructors.Single();
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (constructor), "constructor");

            var method = mutableType.AllMutableMethods.Single (x => x.Name == "Method");
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (method), "method");

            // TODO 4793
            //var returnParameter = method.ReturnParameter;
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), "return value");

            var parameter = method.GetParameters().Single();
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (parameter), "parameter");

            // TODO 4791
            //var property = mutableType.GetProperties().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (property), "property");

            // TODO 4791
            //var getter = property.GetGetMethod();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (getter), "getter");

            // TODO 4791
            //var getterReturnParameter = getter.ReturnParameter;
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (getterReturnParameter), "getter return value");

            // TODO 4791
            //var setter = property.GetGetMethod();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (setter), "setter");

            // TODO 4791
            //var @event = mutableType.GetEvents().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (@event), "event");

            // TODO 4791
            //var nestedType = mutableType.GetNestedTypes().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), "nested type");
          });
    }

    private void CheckEquals (IEnumerable<ICustomAttributeData> actual, IEnumerable<CustomAttributeData> expected)
    {
      Assert.That (actual.Count (), Is.EqualTo (expected.Count ()));
      var attributeDatas = actual.Zip (expected, (a, e) => new { Actual = a, Expected = e });
      foreach (var attributeData in attributeDatas)
      {
        Assert.That (attributeData.Actual.Constructor, Is.EqualTo (attributeData.Expected.Constructor));
        Assert.That (attributeData.Actual.ConstructorArguments, Is.EqualTo (attributeData.Expected.ConstructorArguments.Select(x => x.Value)));

        Assert.That (attributeData.Actual.NamedArguments.Count (), Is.EqualTo (attributeData.Expected.NamedArguments.Count ()));
        var namedArguments = attributeData.Actual.NamedArguments.Zip (attributeData.Expected.NamedArguments, (a, e) => new { Actual = a, Expected = e });
        foreach (var namedArgument in namedArguments)
        {
          Assert.That (namedArgument.Actual.MemberInfo, Is.EqualTo (namedArgument.Expected.MemberInfo));
          Assert.That (namedArgument.Actual.Value, Is.EqualTo (namedArgument.Expected.TypedValue.Value));
        }
      }
    }

    private void CheckEquals (IEnumerable<ICustomAttributeData> customAttributeDatas, string ctorArgument)
    {
      var abcAttribute = customAttributeDatas.SingleOrDefault (a => a.Constructor.DeclaringType == typeof (AbcAttribute));
      Assert.That (abcAttribute, Is.Not.Null);

      Assert.That (abcAttribute.ConstructorArguments.Single(), Is.EqualTo (ctorArgument));
    }

    // Order attributes is not defined
    [MutlipleAttribute ("3"), MutlipleAttribute ("1"), MutlipleAttribute("2")]
    // Order of named arguments is not defined
    [WithNamedArguments(NamedArgument3 = "3", NamedArgument1 = "1", NamedArgument2 = "2")]
    // Select correct ctor
    [WithMultipleCtors ("explicit ctor"), WithMultipleCtors]
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

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class MutlipleAttribute : Attribute
    {
      public MutlipleAttribute (string constructorArgument)
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
    public class WithMultipleCtors : Attribute
    {
      public WithMultipleCtors ()
      {
        ConstructorArgument = "default ctor";
      }

      public WithMultipleCtors (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
    }
  }
}