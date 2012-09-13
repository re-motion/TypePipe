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

namespace TypePipe.IntegrationTests
{
  [Ignore("TODO 5043")]
  [TestFixture]
  public class TypePipeCustomAttributeDataTest
  {
    [Test]
    public void TypePipeCustomAttributeData_StandardReflection ()
    {
      var type = typeof (DomainType);
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.field);
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (7));

      //CheckEquals (CustomAttributeData.GetCustomAttributes (type), TypePipeCustomAttributeData.GetCustomAttributes (type));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (ctor), TypePipeCustomAttributeData.GetCustomAttributes (ctor));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (field), TypePipeCustomAttributeData.GetCustomAttributes (field));
      //CheckEquals (CustomAttributeData.GetCustomAttributes (method), TypePipeCustomAttributeData.GetCustomAttributes (method));
      // TODO: properties, events, nested classes
    }

    [Test]
    public void TypePipeCustomAttributeData_MutableReflection ()
    {

    }

    [Abc ("class")]
    [Abc ("class", NamedArgument = "multiple")]
    [Abc (NamedArgument = "different ctor")]
    public class DomainType
    {
      [Abc ("constructor")]
      public DomainType () { }

      [Abc ("method")]
      [return: Abc ("return value")]
      public void Method ([Abc ("Parameter")] int p) { }

      [Abc ("field")]
      public string field;

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