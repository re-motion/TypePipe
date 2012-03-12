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
using Remotion.Reflection.SignatureStringBuilding;

namespace Remotion.UnitTests.Reflection.SignatureStringBuilding
{
  [TestFixture]
  public class MemberSignatureStringBuilderProviderTest
  {
    [Test]
    public void GetSignatureStringBuilder_Constructor ()
    {
      var result = MemberSignatureStringBuilderProvider.GetSignatureBuilder (MemberTypes.Constructor);
      Assert.That (result, Is.TypeOf<MethodSignatureStringBuilder> ());
    }

    [Test]
    public void GetSignatureStringBuilder_Method ()
    {
      var result = MemberSignatureStringBuilderProvider.GetSignatureBuilder (MemberTypes.Method);
      Assert.That (result, Is.TypeOf<MethodSignatureStringBuilder>());
    }

    [Test]
    public void GetSignatureStringBuilder_Property ()
    {
      var result = MemberSignatureStringBuilderProvider.GetSignatureBuilder (MemberTypes.Property);
      Assert.That (result, Is.TypeOf<PropertySignatureStringBuilder>());
    }

    [Test]
    public void GetSignatureStringBuilder_Event ()
    {
      var result = MemberSignatureStringBuilderProvider.GetSignatureBuilder (MemberTypes.Event);
      Assert.That (result, Is.TypeOf<EventSignatureStringBuilder>());
    }

    [Test]
    public void GetSignatureStringBuilder_Unsupported ()
    {
      Assert.That (
          () => MemberSignatureStringBuilderProvider.GetSignatureBuilder (MemberTypes.TypeInfo),
          Throws.TypeOf<NotSupportedException>().With.Message.EqualTo (
              "Cannot return a signature builder for member type 'TypeInfo'; only methods, properties, and events are supported."));
    }
  }
}