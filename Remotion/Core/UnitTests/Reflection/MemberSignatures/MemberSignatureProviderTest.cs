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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.UnitTests.Reflection.MemberSignatures
{
  [TestFixture]
  public class MemberSignatureProviderTest
  {
    [Test]
    public void GetMemberSignature_Constructor ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());

      var result = MemberSignatureProvider.GetMemberSignature (constructor);

      Assert.That (result, Is.TypeOf<MethodSignature> ());
      Assert.That (result.ToString(), Is.EqualTo("System.Void()"));
    }

    [Test]
    public void GetMemberSignature_Method ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method());

      var result = MemberSignatureProvider.GetMemberSignature (method);

      Assert.That (result, Is.TypeOf<MethodSignature> ());
      Assert.That (result.ToString (), Is.EqualTo ("System.Double()"));      
    }

    [Test]
    public void GetMemberSignature_Field ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType obj) => obj.Field);

      var result = MemberSignatureProvider.GetMemberSignature (method);

      Assert.That (result, Is.TypeOf<FieldSignature> ());
    }

    [Test]
    public void GetMemberSignature_Property ()
    {
      var property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.Property);

      var result = MemberSignatureProvider.GetMemberSignature (property);

      Assert.That (result, Is.TypeOf<PropertySignature> ());
      Assert.That (result.ToString (), Is.EqualTo ("System.String()"));
    }

    [Test]
    public void GetMemberSignature_Event ()
    {
      var eventInfo = typeof (DomainType).GetEvent ("Event");

      var result = MemberSignatureProvider.GetMemberSignature (eventInfo);

      Assert.That (result, Is.TypeOf<EventSignature> ());
      Assert.That (result.ToString (), Is.EqualTo ("System.Action"));
    }

    [Test]
    public void GetMemberSignature_Unsupported ()
    {
      var type = typeof(object);

      Assert.That (
          () => MemberSignatureProvider.GetMemberSignature (type),
          Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (
              "Cannot return a signature builder for member type 'TypeInfo'; only constructors, methods, fields, properties and events are supported."));
    }

    public class DomainType
    {
      internal int Field = 0;

      public double Method () { return 0.0; }

      public string Property { get; set; }

      public event Action Event;
    }
  }
}