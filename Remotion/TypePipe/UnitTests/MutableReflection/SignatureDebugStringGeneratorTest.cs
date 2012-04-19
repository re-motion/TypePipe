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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class SignatureDebugStringGeneratorTest
  {
    [Test]
    public void GetFieldSignature ()
    {
      var field = MemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);

      var result = SignatureDebugStringGenerator.GetFieldSignature (field);

      Assert.That (result, Is.EqualTo ("IEnumerable`1[DomainType] Field"));
    }

    [Test]
    public void GetConstructorSignature ()
    {
      var constructor = MemberInfoFromExpressionUtility.GetConstructor (() => new DomainType(7, ref Dev<string>.Dummy));

      var result = SignatureDebugStringGenerator.GetConstructorSignature (constructor);

      Assert.That (result, Is.EqualTo ("Void .ctor(Int32, String&)"));
    }

    [Test]
    public void GetMethodSignature ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (ref Dev<int>.Dummy, null));

      var result = SignatureDebugStringGenerator.GetMethodSignature (method);

      Assert.That (result, Is.EqualTo ("String Method(Int32&, Dictionary`2[Int32,DateTime])"));
    }

    class DomainType
    {
      internal readonly IEnumerable<DomainType> Field;

      public DomainType (int i, ref string s)
      {
        Dev.Null = i;
        s = null;
        Field = null;
      }

      public string Method (ref int i, Dictionary<int, DateTime> dictionary)
      {
        i = 0;
        Dev.Null = dictionary;
        return ""; 
      }
    }
  }
}