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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.UnitTests.Reflection.MemberSignatures
{
  [TestFixture]
  public class MethodSignatureTest
  {
    private MethodInfo _genericMethod1;
    private MethodInfo _genericMethod2;
    private MethodInfo _genericMethod3;

    [SetUp]
    public void SetUp ()
    {
      _genericMethod1 = MemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M1<Dev.T, Dev.T> (null));
      _genericMethod2 = MemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M2<Dev.T, Dev.T> (null));
      _genericMethod3 = MemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M3<Dev.T, Dev.T> (null));
    }

    [Test]
    public void Create_NonGenericMethod ()
    {
      var method = MemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.Equals(null));
      var signature = MethodSignature.Create (method);

      Assert.That (signature.ReturnType, Is.SameAs (typeof (bool)));
      Assert.That (signature.GenericParameterCount, Is.EqualTo (0));
      Assert.That (signature.ParameterTypes, Is.EqualTo (new[] { typeof (object) }));
    }

    [Test]
    public void Create_OpenGenericMethod ()
    {
      var signature = MethodSignature.Create (_genericMethod1);

      Assert.That (signature.ReturnType, Is.SameAs (_genericMethod1.ReturnType));
      Assert.That (signature.GenericParameterCount, Is.EqualTo (2));

      var parameter = _genericMethod1.GetParameters ().Single();
      Assert.That (signature.ParameterTypes, Is.EqualTo (new[] { parameter.ParameterType }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Closed generic methods are not supported.\r\nParameter name: methodBase")]
    public void Create_ClosedGenericMethod ()
    {
      var method = _genericMethod1.MakeGenericMethod (typeof (int), typeof (int));
      MethodSignature.Create (method);
    }

    [Test]
    public void ToString_NonGenericMethod ()
    {
      var signature = new MethodSignature (typeof (string), new[] { typeof (int) }, 0);

      Assert.That (signature.ToString(), Is.EqualTo ("System.String(System.Int32)"));
    }

    [Test]
    public void ToString_GenericMethod ()
    {
      var signature1 = MethodSignature.Create (_genericMethod1);
      Assert.That (signature1.ToString (), Is.EqualTo ("[0]([1])`2"));

      var signature2 = MethodSignature.Create (_genericMethod2);
      Assert.That (signature2.ToString (), Is.EqualTo ("[0]([1])`2"));

      var signature3 = MethodSignature.Create (_genericMethod3);
      Assert.That (signature3.ToString (), Is.EqualTo ("[1]([0])`2"));
    }

    [Test]
    public void Equals_True ()
    {
      var signature1 = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);
      var signature2 = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);

      Assert.That (signature1.Equals (signature2), Is.True);
    }

    [Test]
    public void Equals_False ()
    {
      var signature = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);
      Assert.That (signature.Equals (null), Is.False);

      var signatureWithDifferentMethodType = new MethodSignature (typeof (string), new[] { typeof (double), typeof (string) }, 12);
      Assert.That (signature.Equals (signatureWithDifferentMethodType), Is.False);

      var signatureWithDifferentIndexParameters = new MethodSignature (typeof (int), new[] { typeof (string), typeof (double) }, 12);
      Assert.That (signature.Equals (signatureWithDifferentIndexParameters), Is.False);

      var signatureWithDifferentGenericParameterCount = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 7);
      Assert.That (signature.Equals (signatureWithDifferentGenericParameterCount), Is.False);
    }

    [Test]
    public void Equals_WithUsedGenericParameters ()
    {
      var signature1 = MethodSignature.Create (_genericMethod1);
      var signature2 = MethodSignature.Create (_genericMethod2);
      var signature3 = MethodSignature.Create (_genericMethod3);

      Assert.That (signature1.Equals (signature2), Is.True);
      Assert.That (signature1.Equals (signature3), Is.False);
    }

    [Test]
    public void Equals_Object ()
    {
      var signature = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);

      object otherSignatureAsObject = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);
      Assert.That (signature.Equals (otherSignatureAsObject), Is.True);

      Assert.That (signature.Equals ((object) null), Is.False);

      object completelyUnrelatedObject = new object ();
      Assert.That (signature.Equals (completelyUnrelatedObject), Is.False);
    }

    [Test]
    public void GetHashCode_ForEqualObjects ()
    {
      var signature1 = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);
      var signature2 = new MethodSignature (typeof (int), new[] { typeof (double), typeof (string) }, 12);

      Assert.That (signature1.GetHashCode (), Is.EqualTo (signature2.GetHashCode ()));
    }

    [Test]
    public void GetHashCode_ForEqualObjects_WithUsedGenericParameters ()
    {
      var signature1 = MethodSignature.Create (_genericMethod1);
      var signature2 = MethodSignature.Create (_genericMethod2);

      Assert.That (signature1.GetHashCode(), Is.EqualTo (signature2.GetHashCode()));
    }

// ReSharper disable ClassNeverInstantiated.Global
    public class ClassWithGenericMethods
// ReSharper restore ClassNeverInstantiated.Global
    {
      public T1 M1<T1, T2> (T2 t)
      {
        throw new NotImplementedException();
      }

      public T1 M2<T1, T2> (T2 t)
      {
        throw new NotImplementedException ();
      }

      // should be different!
      public T2 M3<T1, T2> (T1 t)
      {
        throw new NotImplementedException ();
      }
    }
  }
}