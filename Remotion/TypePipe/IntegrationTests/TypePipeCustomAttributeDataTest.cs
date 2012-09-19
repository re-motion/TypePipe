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
using System.Collections.ObjectModel;
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

      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (type), CustomAttributeData.GetCustomAttributes (type));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (field), CustomAttributeData.GetCustomAttributes (field));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (ctor), CustomAttributeData.GetCustomAttributes (ctor));
      CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (method), CustomAttributeData.GetCustomAttributes (method));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (returnParameter), CustomAttributeData.GetCustomAttributes (returnParameter));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (parameter), CustomAttributeData.GetCustomAttributes (parameter));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (property), CustomAttributeData.GetCustomAttributes (property));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (@event), CustomAttributeData.GetCustomAttributes (@event));
      //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (nestedType), CustomAttributeData.GetCustomAttributes (nestedType));
    }

    [Test]
    public void TypePipeCustomAttributeData_MutableReflection ()
    {
      AssembleType<DomainType> (
          mutableType =>
          {
            //var attribute1 = BuildExpectedAttributeData ("class");
            //var attribute2 = BuildExpectedAttributeData ("class", "multiple");
            //var attribute3Ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute ());
            //var attribute3 = BuildExpectedAttributeData (null, "different ctor", attribute3Ctor);
            //CheckEquals(TypePipeCustomAttributeData.GetCustomAttributes(mutableType), new[] { attribute1, attribute2, attribute3 });

            //var attribute8 = BuildExpectedAttributeData ("field");
            //var field = method.AllMutableFields.Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(field), new[] { attribute8 });

            //var attribute4 = BuildExpectedAttributeData ("constructor");
            //var constructor = mutableType.AllMutableConstructors.Single();
            //CheckEquals(TypePipeCustomAttributeData.GetCustomAttributes(constructor), new[] { attribute4 });

            var attribute5 = BuildExpectedAttributeData ("method");
            var method = mutableType.AllMutableMethods.Single (x => x.Name == "Method");
            CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes (method), new[] { attribute5 });

            //var attribute6 = BuildExpectedAttributeData("return value");
            //var returnParameter = method.ReturnParameter;
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(returnParameter), new[] { attribute6 });

            //var attribute7 = BuildExpectedAttributeData ("parameter");
            //var parameter = method.GetParameter().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(parameter), new[] { attribute7 });

            //var attribute8 = BuildExpectedAttributeData ("property");
            //var property = method.GetProperties().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(property), new[] { attribute8 });

            //var attribute9 = BuildExpectedAttributeData ("getter");
            //var getter = property.GetGetMethod();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(getter), new[] { attribute9 });
            
            //var attribute10 = BuildExpectedAttributeData ("getter return value");
            //var getterReturnParameter = getter.ReturnParameter;
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(getterReturnParameter), new[] { attribute10 });

            //var attribute11 = BuildExpectedAttributeData ("setter");
            //var setter = property.GetGetMethod();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(setter), new[] { attribute11 });

            //var attribute12 = BuildExpectedAttributeData ("event");
            //var @event = type.GetEvents().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(@event), new[] { attribute12 });

            //var attribute13 = BuildExpectedAttributeData ("nested type");
            //var nestedType = type.GetNestedTypes().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(nestedType), new[] { attribute13 });
          });
    }

    private void CheckEquals (IEnumerable<ICustomAttributeData> actual, IEnumerable<CustomAttributeData> expected)
    {
      CheckEquals (actual, expected.Select (ConvertToComparable));
    }

    private ICustomAttributeData ConvertToComparable (CustomAttributeData customAttributeData)
    {
      return new CustomAttributeDataStub
      {
        Constructor = customAttributeData.Constructor,
        ConstructorArguments = customAttributeData.ConstructorArguments.Select (x => x.Value).ToList ().AsReadOnly (),
        NamedArguments = customAttributeData.NamedArguments.Select (
            x =>
            new CustomAttributeNamedArgumentStub { MemberInfo = x.MemberInfo, Value = x.TypedValue.Value })
            .Cast<ICustomAttributeNamedArgument> ().ToList ().AsReadOnly ()
      };
    }

    private void CheckEquals (IEnumerable<ICustomAttributeData> actual, IEnumerable<ICustomAttributeData> expected)
    {
      Assert.That (actual.Count(), Is.EqualTo (expected.Count()));
      var attributeDatas = actual.Zip (expected, (a, e) => new { Actual = a, Expected = e });
      foreach (var attributeData in attributeDatas)
      {
        Assert.That (attributeData.Actual.Constructor, Is.EqualTo (attributeData.Expected.Constructor));
        Assert.That (attributeData.Actual.ConstructorArguments, Is.EqualTo (attributeData.Expected.ConstructorArguments));

        Assert.That (attributeData.Actual.NamedArguments.Count (), Is.EqualTo (attributeData.Expected.NamedArguments.Count ()));
        var namedArguments = attributeData.Actual.NamedArguments.Zip (attributeData.Expected.NamedArguments, (a, e) => new { Actual = a, Expected = e });
        foreach (var namedArgument in namedArguments)
        {
          Assert.That (namedArgument.Actual.MemberInfo, Is.EqualTo (namedArgument.Expected.MemberInfo));
          Assert.That (namedArgument.Actual.Value, Is.EqualTo (namedArgument.Expected.Value));
        }
      }
    }

    private ICustomAttributeData BuildExpectedAttributeData (
        string ctorArgumentValue, string namedArgumentValue = null, ConstructorInfo constructor = null)
    {
      constructor = constructor ?? NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (""));

      var namedArguments = new List<ICustomAttributeNamedArgument>();
      if (namedArgumentValue != null)
      {
        var namedArgumentMember = NormalizingMemberInfoFromExpressionUtility.GetProperty ((AbcAttribute obj) => obj.NamedArgument);
        var namedArgument = new CustomAttributeNamedArgumentStub { MemberInfo = namedArgumentMember, Value = namedArgumentValue };
        namedArguments.Add (namedArgument);
      }

      return new CustomAttributeDataStub
             {
                 Constructor = constructor,
                 ConstructorArguments = new object[] { ctorArgumentValue }.ToList().AsReadOnly(),
                 NamedArguments = namedArguments.AsReadOnly()
             };
    }

    private class CustomAttributeDataStub : ICustomAttributeData
    {
      public ConstructorInfo Constructor { get; set; }
      public ReadOnlyCollection<object> ConstructorArguments { get; set; }
      public IEnumerable<ICustomAttributeNamedArgument> NamedArguments { get; set; }
    }

    private class CustomAttributeNamedArgumentStub : ICustomAttributeNamedArgument
    {
      public MemberInfo MemberInfo { get; set; }
      public object Value { get; set; }
    }

    [Abc ("class")]
    [Abc ("class", NamedArgument = "multiple")]
    [Abc (NamedArgument = "different ctor")]
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

    [AttributeUsageAttribute (AttributeTargets.All, AllowMultiple = true)]
    public class AbcAttribute : Attribute
    {
      public AbcAttribute ()
      {
        ConstructorArgument = "default ctor";
      }

      public AbcAttribute (string constructorArgument)
      {
        ConstructorArgument = constructorArgument;
      }

      public string ConstructorArgument { get; set; }
      public string NamedArgument { get; set; }
    }
  }
}