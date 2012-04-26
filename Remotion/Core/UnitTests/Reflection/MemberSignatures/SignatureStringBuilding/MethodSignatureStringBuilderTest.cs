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
using NUnit.Framework;
using Remotion.Reflection.MemberSignatures.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding
{
  [TestFixture]
  public class MethodSignatureStringBuilderTest
  {
    private MethodSignatureStringBuilder _builder;

    [SetUp]
    public void SetUp ()
    {
      _builder = new MethodSignatureStringBuilder ();
    }

    [Test]
    public void BuildSignatureString_MethodBase_NoParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithoutParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void()"));
    }

    [Test]
    public void BuildSignatureString_MethodBase_MethodBase_WithParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)"));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Closed generic methods are not supported.\r\nParameter name: methodBase")]
    public void BuildSignatureString_MethodBase_ClosedGenericMethod ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters").MakeGenericMethod (typeof (int), typeof (int));
      _builder.BuildSignatureString (method);
    }

    [Test]
    public void BuildSignatureString_MethodBase_WithGenericParameters ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters");
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)`2"));
    }

    [Test]
    public void BuildSignatureString_MethodBase_Constructor ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetConstructors().Single();
      var signature = _builder.BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)"));
    }

    [Test]
    public void BuildSignatureString_InterfaceMember_Method ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithoutParameters");
      var signature = ((IMemberSignatureStringBuilder) _builder).BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void()"));
    }

    [Test]
    public void BuildSignatureString_InterfaceMember_Constructor ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetConstructors ().Single ();
      var signature = ((IMemberSignatureStringBuilder) _builder).BuildSignatureString (method);

      Assert.That (signature, Is.EqualTo ("System.Void(System.String,System.DateTime)"));
    }

    [Test]
    public void BuildSignatureString_ExplicitSignature_ZeroGenericParameterCount ()
    {
      var returnType = typeof (string);
      var parameterTypes = new[] { typeof(int), typeof(DateTime) };
      var genericParameterCount = 0;

      var signature = _builder.BuildSignatureString (returnType, parameterTypes, genericParameterCount);

      Assert.That (signature, Is.EqualTo ("System.String(System.Int32,System.DateTime)"));
    }

    [Test]
    public void BuildSignatureString_ExplicitSignature_NonZeroGenericParameterCount ()
    {
      var returnType = typeof (string);
      var parameterTypes = new[] { typeof (int), typeof (DateTime) };
      var genericParameterCount = 7;

      var signature = _builder.BuildSignatureString (returnType, parameterTypes, genericParameterCount);

      Assert.That (signature, Is.EqualTo ("System.String(System.Int32,System.DateTime)`7"));
    }
  }
}
