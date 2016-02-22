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
using System.Text;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class MethodSignatureStringBuilderHelperTest
  {
    private MethodSignatureStringBuilderHelper _helper;
    private StringBuilder _sb;

    [SetUp]
    public void SetUp ()
    {
      _helper = new MethodSignatureStringBuilderHelper ();
      _sb = new StringBuilder ();
    }

    [Test]
    public void AppendTypeString_SimpleType ()
    {
      _helper.AppendTypeString (_sb, typeof (void));
      
      Assert.That (_sb.ToString(), Is.EqualTo ("System.Void"));
    }

    [Test]
    public void AppendTypeString_GenericMethodParameter ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType dt) => dt.MethodWithUsedGenericParameters<Dev.T, Dev.T>(null));
      var genericParameter = method.GetGenericArguments()[0];
      _helper.AppendTypeString (_sb, genericParameter);

      Assert.That (_sb.ToString (), Is.EqualTo ("[0]"));
    }

    [Test]
    public void AppendTypeString_GenericTypeParameter ()
    {
      var genericParameter = typeof (GenericDomainType<,>).GetGenericArguments ()[0];
      _helper.AppendTypeString (_sb, genericParameter);

      Assert.That (_sb.ToString (), Is.EqualTo ("[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainType`2]"));
    }

    [Test]
    public void AppendTypeString_ClosedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainType<int, string>));

      Assert.That (_sb.ToString(), Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainType`2[System.Int32,System.String]"));
    }

    [Test]
    public void AppendTypeString_GenericTypeDefinition ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainType<,>));

      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainType`2"));
    }

    [Test]
    public void AppendTypeString_NestedType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainTypeWithNestedType<,>.Nested));

      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainTypeWithNestedType`2+Nested"));
    }

    [Test]
    public void AppendTypeString_ClosedNestedType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainTypeWithNestedType<int,string>.Nested));

      Assert.That (_sb.ToString (), Is.EqualTo (
          "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainTypeWithNestedType`2+Nested[System.Int32,System.String]"));
    }

    [Test]
    public void AppendTypeString_NestedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainTypeWithNestedType<,>.NestedGeneric<>));

      Assert.That (_sb.ToString (), Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainTypeWithNestedType`2+NestedGeneric`1"));
    }

    [Test]
    public void AppendTypeString_ClosedNestedGenericType ()
    {
      _helper.AppendTypeString (_sb, typeof (GenericDomainTypeWithNestedType<int,string>.NestedGeneric<double>));

      Assert.That (_sb.ToString (), Is.EqualTo (
          "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureStringBuilderHelperTest+GenericDomainTypeWithNestedType`2+NestedGeneric`1[System.Int32,System.String,System.Double]"));
    }

    [Test]
    public void AppendSeparatedTypeStrings_None ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new Type[0]);

      Assert.That (_sb.ToString (), Is.Empty);
    }

    [Test]
    public void AppendSeparatedTypeStrings_One ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new[] { typeof (int) });

      Assert.That (_sb.ToString (), Is.EqualTo ("System.Int32"));
    }

    [Test]
    public void AppendSeparatedTypeStrings_Many ()
    {
      _helper.AppendSeparatedTypeStrings (_sb, new[] { typeof (int), typeof (double), typeof (string) });

      Assert.That (_sb.ToString (), Is.EqualTo ("System.Int32,System.Double,System.String"));
    }

// ReSharper disable ClassNeverInstantiated.Global
    public class DomainType
// ReSharper restore ClassNeverInstantiated.Global
    {
      public T1 MethodWithUsedGenericParameters<T1, T2> (T2 p1)
      {
        Dev.Null = p1;
        return default (T1);
      }
    }

    // ReSharper disable UnusedTypeParameter
    public class GenericDomainType<TType1, TType2>
    {
    }
    // ReSharper restore UnusedTypeParameter

    // ReSharper disable UnusedTypeParameter
    public static class GenericDomainTypeWithNestedType<TType1, TType2>
    {
      public class Nested
      {
      }

      public class NestedGeneric<TNested1>
      {
      }
    }
    // ReSharper restore UnusedTypeParameter

  }
}
