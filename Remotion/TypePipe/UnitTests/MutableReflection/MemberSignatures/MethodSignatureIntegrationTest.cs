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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class MethodSignatureIntegrationTest
  {
    [Test]
    public void MethodSignatureString_ClosedGenericType ()
    {
      var method = typeof (DomainType<int, string>).GetMethod ("MethodWithClosedGenericType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2[System.Int32,System.String])"));
    }

    [Test]
    public void MethodSignatureString_NestedType ()
    {
      var method = typeof (DomainType<,>).GetMethod ("MethodWithNestedType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+Nested"
              + "[[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2]])"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType ()
    {
      var method = typeof (DomainType<,>).GetMethod ("MethodWithNestedGenericType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "System.Int32])"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType_ClosedWithDifferentTypeParameters ()
    {
      var method = typeof (DomainType<,>).GetMethod ("MethodWithPartiallyClosedNestedGenericType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void("
              + "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[0]],"
              + "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[0/Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2]])`1"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType_ClosedWithOuterTypeParameters ()
    {
      var method = typeof (DomainType<int, string>).GetMethod ("MethodWithPartiallyClosedNestedGenericType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void("
              + "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1[System.Int32,System.String,[0]],"
              + "Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1[System.Int32,System.String,System.Int32]"
              + ")`1"));
    }

    private string BuildSignatureString (MethodBase method)
    {
      var methodSignature = MethodSignature.Create (method);
      return methodSignature.ToString();
    }

    // ReSharper disable UnusedTypeParameter
    // ReSharper disable UnusedMember.Global
    // ReSharper disable ClassNeverInstantiated.Global
    public class DomainType<TType1, TType2>
    {
      public DomainType (string p1, DateTime p2)
      {
        Dev.Null = p1;
        Dev.Null = p2;
      }

      public void MethodWithClosedGenericType (DomainType<int, string> p1)
      {
        Dev.Null = p1;
      }

      public void MethodWithNestedType (Nested p1)
      {
        Dev.Null = p1;
      }

      public void MethodWithNestedGenericType (NestedGeneric<int> p1)
      {
        Dev.Null = p1;
      }

      public void MethodWithPartiallyClosedNestedGenericType<T1> (NestedGeneric<T1> p1, NestedGeneric<TType1> p2)
      {
        Dev.Null = p1;
        Dev.Null = p2;
      }

      public class Nested
      {
      }

      public class NestedGeneric<TNested>
      {
      }
    }
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedTypeParameter
  }
}
