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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class SignatureDebugStringGeneratorTest
  {
    [Test]
    public void GetTypeSignature ()
    {
      var standardType = typeof (DomainType);
      var genericType = typeof (DomainType.NestedGenericType<int, DomainType>);
      var genericTypeDefinition = typeof (DomainType.NestedGenericType<,>);

      var standard = SignatureDebugStringGenerator.GetTypeSignature (standardType);
      var generic = SignatureDebugStringGenerator.GetTypeSignature (genericType);
      var genericDefinition = SignatureDebugStringGenerator.GetTypeSignature (genericTypeDefinition);

      Assert.That (standard, Is.EqualTo ("DomainType"));
      Assert.That (generic, Is.EqualTo ("NestedGenericType`2[Int32,DomainType]"));
      Assert.That (genericDefinition, Is.EqualTo ("NestedGenericType`2[T1,T2]"));
    }

    [Test]
    public void GetFieldSignature ()
    {
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);

      var result = SignatureDebugStringGenerator.GetFieldSignature (field);

      Assert.That (result, Is.EqualTo ("IEnumerable`1[DomainType] Field"));
    }

    [Test]
    public void GetConstructorSignature ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType(7, out Dev<string>.Dummy));

      var result = SignatureDebugStringGenerator.GetConstructorSignature (constructor);

      Assert.That (result, Is.EqualTo ("Void .ctor(Int32, String&)"));
    }

    [Test]
    public void GetMethodSignature ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (ref Dev<int>.Dummy, null));

      var result = SignatureDebugStringGenerator.GetMethodSignature (method);

      Assert.That (result, Is.EqualTo ("String Method(Int32&, Dictionary`2[Int32,DateTime])"));
    }

    [Test]
    public void GetMethodSignature_GenericMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType obj) => obj.GenericMethod<int, int>());

      var result = SignatureDebugStringGenerator.GetMethodSignature (method);

      Assert.That (result, Is.EqualTo ("Void GenericMethod[T1,T2]()"));
    }

    [Test]
    public void GetParameterSignature ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (ref Dev<int>.Dummy, null));
      var ctor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (7, out Dev<string>.Dummy));

      var result1 = SignatureDebugStringGenerator.GetParameterSignature (method.GetParameters()[0]);
      var result2 = SignatureDebugStringGenerator.GetParameterSignature (method.GetParameters()[1]);
      var result3 = SignatureDebugStringGenerator.GetParameterSignature (ctor.GetParameters()[1]);

      Assert.That (result1, Is.EqualTo ("Int32& i"));
      Assert.That (result2, Is.EqualTo ("Dictionary`2[Int32,DateTime] dictionary"));
      Assert.That (result3, Is.EqualTo ("String& s"));
    }

    [Test]
    public void GetPropertySignature ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);

      var result = SignatureDebugStringGenerator.GetPropertySignature (property);

      Assert.That (result, Is.EqualTo ("String Property"));
    }

    [Test]
    public void GetEventSignature ()
    {
      var event_ = typeof (DomainType).GetEvent ("Event");

      var result = SignatureDebugStringGenerator.GetEventSignature (event_);

      Assert.That (result, Is.EqualTo ("EventHandler Event"));
    }

    class DomainType
    {
// ReSharper disable UnusedTypeParameter
      internal class NestedGenericType<T1, T2> { }
// ReSharper restore UnusedTypeParameter

      internal readonly IEnumerable<DomainType> Field;

      public DomainType (int i, out string s)
      {
        Dev.Null = i;
        s = null;
        Field = null;
      }

      public string Method (ref int i, Dictionary<int, DateTime> dictionary)
      {
        i++;
        Dev.Null = dictionary;
        return ""; 
      }

      public void GenericMethod<T1, T2> () {}

      public string Property { get { return ""; } }

      public event EventHandler Event;
    }
  }
}