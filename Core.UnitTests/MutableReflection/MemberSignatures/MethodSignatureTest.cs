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
using System.Text;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.MemberSignatures;
using Moq;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
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
      _genericMethod1 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M1<Dev.T, Dev.T> (null));
      _genericMethod2 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M2<Dev.T, Dev.T> (null));
      _genericMethod3 = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((ClassWithGenericMethods c) => c.M3<Dev.T, Dev.T> (null));
    }

    [Test]
    public void Create_NonGenericMethod ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.Equals(null));
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
    public void Create_ClosedGenericMethod ()
    {
      var method = _genericMethod1.MakeGenericMethod (typeof (int), typeof (int));
      Assert.That (
          () => MethodSignature.Create (method),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Closed generic methods are not supported.\r\nParameter name: methodBase"));
    }

    [Test]
    public void Initialization_UsesInjectedSignatureBuilder ()
    {
      var returnType = typeof (int);
      var parameterTypes = new[] { typeof (double) };
      var signatureBuilderMock = new Mock<IMethodSignatureStringBuilderHelper> (MockBehavior.Strict);
      signatureBuilderMock.Setup (mock => mock.AppendTypeString (It.IsAny<StringBuilder>(), returnType)).Verifiable();
      signatureBuilderMock.Setup (mock => mock.AppendSeparatedTypeStrings (It.IsAny<StringBuilder>(), parameterTypes)).Verifiable();
      var signature = new MethodSignature (returnType, parameterTypes, 0, signatureBuilderMock.Object);

      Dev.Null = signature.ToString();

      signatureBuilderMock.Verify();
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
    public void AreEqual ()
    {
      Assert.That (MethodSignature.AreEqual (_genericMethod1, _genericMethod2), Is.True);
      Assert.That (MethodSignature.AreEqual (_genericMethod1, _genericMethod3), Is.False);
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