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
using Remotion.Reflection.MemberSignatures.SignatureStringBuilding;
using Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding.TestDomain;

namespace Remotion.UnitTests.Reflection.MemberSignatures.SignatureStringBuilding
{
  [TestFixture]
  public class EventSignatureStringBuilderTest
  {
    private EventSignatureStringBuilder _builder;

    [SetUp]
    public void SetUp ()
    {
      _builder = new EventSignatureStringBuilder ();
    }

    [Test]
    public void BuildSignatureString_EventInfo ()
    {
      var eventInfo = typeof (ClassForEventSignatureStringBuilding).GetEvent ("Event");
      var signature = _builder.BuildSignatureString (eventInfo);

      Assert.That (signature, Is.EqualTo ("System.EventHandler"));
    }

    [Test]
    public void BuildSignatureString_ExplicitSignature ()
    {
      var eventHandlerType = typeof (EventHandler);
      var signature = _builder.BuildSignatureString (eventHandlerType);

      Assert.That (signature, Is.EqualTo ("System.EventHandler"));
    }
  }
}
