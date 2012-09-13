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
using Remotion.Development.UnitTesting.Reflection;

namespace TypePipe.IntegrationTests
{
  [Ignore ("TODO 5043")]
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

      //CheckEquals (CustomAttributeData.GetCustomAttributes (type), TypePipeCustomAttributeData.GetCustomAttributes (type));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (field), TypePipeCustomAttributeData.GetCustomAttributes (field));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (ctor), TypePipeCustomAttributeData.GetCustomAttributes (ctor));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (method), TypePipeCustomAttributeData.GetCustomAttributes (method));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (returnParameter), TypePipeCustomAttributeData.GetCustomAttributes (returnParameter));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (parameter), TypePipeCustomAttributeData.GetCustomAttributes (parameter));
      // TODO: properties, events, nested classes
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

            //var attribute5 = BuildExpectedAttributeData ("method");
            //var method = mutableType.AllMutableMethods.Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(method), new[] { attribute5 });

            //var attribute6 = BuildExpectedAttributeData("return value");
            //var returnParameter = method.ReturnParameter;
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(returnParameter), new[] { attribute6 });

            //var attribute7 = BuildExpectedAttributeData ("Parameter");
            //var parameter = method.GetParameter().Single();
            //CheckEquals (TypePipeCustomAttributeData.GetCustomAttributes(parameter), new[] { attribute7 });
            // TODO: properties, events, nested classes
          });
    }

    // TODO: swap arguments
    //private void CheckEquals (IEnumerable<CustomAttributeData> expected, IEnumerable<TypePipeCustoAttributeData> actual)
    //{
    //}

    //private void CheckEquals (IEnumerable<TypePipeCustoAttributeData> actual, IEnumerable<TypePipeCustoAttributeData> expected)
    //{
    //}

    //private TypeCustomAttributeData BuildExpectedAttributeData (
    //    string constructorArgument, string namedArgument = null, ConstructorInfo constructorInfo = null)
    //{
    //  constructorInfo = constructorInfo ?? NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (""));
    //  
    //}

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
      public virtual void Method ([Abc ("Parameter")] int p)
      {
      }


      //[Abc ("property")]
      //public string Property
      //{
      //  [Abc ("getter")]
      //  [return: Abc ("getter return value")]
      //  get { return _field; }

      //  [Abc ("setter")]
      //  // Annotate parameter?
      //  set { _field = value; }
      //}
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