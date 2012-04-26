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
using NUnit.Framework;
using Remotion.Reflection.MemberSignatures;
using Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.MemberSignatures
{
  [TestFixture]
  public class MethodSignatureTest
  {
    [Test]
    public void Create_OpenGenericMethod ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters");
      var signature = MethodSignature.Create (method);

      Assert.That (signature.ReturnType, Is.SameAs (typeof (void)));
      Assert.That (signature.GenericParameterCount, Is.EqualTo (2));
      Assert.That (signature.ParameterTypes, Is.EqualTo (new[] { typeof (string), typeof (DateTime) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Closed generic methods are not supported.\r\nParameter name: methodBase")]
    public void Create_ClosedGenericMethod ()
    {
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithGenericParameters").MakeGenericMethod (typeof (int), typeof (int));
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
      var method = typeof (ClassForMethodSignatureStringBuilding<,>).GetMethod ("MethodWithUsedGenericParameters");
      var signature = MethodSignature.Create (method);

      Assert.That (signature.ToString (), Is.EqualTo ("[0]([1])`2"));
    }
  }
}