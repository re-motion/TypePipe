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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class MemberSignatureProviderTest
  {
    [Test]
    public void GetMemberSignature_NestedType ()
    {
      var nestedType = typeof (DomainType.NestedType);

      var result = MemberSignatureProvider.GetMemberSignature (nestedType);

      Assert.That (result, Is.TypeOf<NestedTypeSignature>());
      Assert.That (result.ToString(), Is.EqualTo ("`0"));
    }

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
      public class NestedType {}

      internal int Field = 0;

      public double Method () { return 0.0; }

      public string Property { get; set; }

      public event Action Event;
    }
  }
}