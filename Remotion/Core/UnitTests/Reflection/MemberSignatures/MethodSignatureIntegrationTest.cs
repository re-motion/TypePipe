// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.UnitTests.Reflection.MemberSignatures
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
              "System.Void"
              + "(Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2[System.Int32,System.String])"));
    }

    [Test]
    public void MethodSignatureString_NestedType ()
    {
      var method = typeof (DomainType<,>).GetMethod ("MethodWithNestedType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+Nested"
              + "[[0/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2]])"));
    }

    [Test]
    public void MethodSignatureString_NestedGenericType ()
    {
      var method = typeof (DomainType<,>).GetMethod ("MethodWithNestedGenericType");
      var signature = BuildSignatureString (method);

      Assert.That (
          signature,
          Is.EqualTo (
              "System.Void(Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
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
              + "Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[0]],"
              + "Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1"
              + "[[0/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[1/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2],"
              + "[0/Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2]])`1"));
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
              + "Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1[System.Int32,System.String,[0]],"
              +
              "Remotion.UnitTests.Reflection.MemberSignatures.MethodSignatureIntegrationTest+DomainType`2+NestedGeneric`1[System.Int32,System.String,System.Int32]"
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
