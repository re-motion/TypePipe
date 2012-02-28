// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.TypePipe.Utilities;

namespace Remotion.TypePipe.UnitTests.Utilities
{
  [TestFixture]
  public class SlotTest
  {
    [Test]
    public void ShouldThrowForEmptySlotWithoutDefault ()
    {
      // Arrange
      var slot = Slot.New<string> ();

      // Act
      TestDelegate action = () => slot.Get ();

      // Assert
      Assert.That (slot.HasItem, Is.False);
      Assert.That (slot.HasDefault, Is.False);
      Assert.That (slot.CanGet, Is.False);

      Assert.That (action, Throws.InvalidOperationException
        .With.Message.EqualTo ("Item not set"));
    }

    [Test]
    public void ShouldNotThrowForEmptySlotWithDefault ()
    {
      // Arrange
      var slot = Slot.WithDefault ("def");

      // Act
      string item = null;
      TestDelegate action = () => item = slot.Get();

      // Assert
      Assert.That (slot.HasItem, Is.False);
      Assert.That (slot.HasDefault, Is.True);
      Assert.That (slot.CanGet, Is.True);

      Assert.That (action, Throws.Nothing);
      Assert.That (item, Is.EqualTo ("def"));
    }

    [Test]
    public void ShouldNotThrowForStandardUsageWithoutDefault ()
    {
      // Arrange
      var slot = Slot.New<string> ();

      // Act
      slot.Set ("abc");
      string item = null;
      TestDelegate action = () => item = slot.Get();

      // Assert
      Assert.That (slot.HasItem, Is.True);
      Assert.That (slot.HasDefault, Is.False);
      Assert.That (slot.CanGet, Is.True);

      Assert.That (action, Throws.Nothing);
      Assert.That (item, Is.EqualTo ("abc"));
    }

    [Test]
    public void ShouldNotThrowForStandardUsageWithDefaultAndReturnItem ()
    {
      // Arrange
      var slot = Slot.WithDefault ("def");

      // Act
      slot.Set ("abc");
      string item = null;
      TestDelegate action = () => item = slot.Get();

      // Assert
      Assert.That (slot.HasItem, Is.True);
      Assert.That (slot.HasDefault, Is.True);
      Assert.That (slot.CanGet, Is.True);

      Assert.That (action, Throws.Nothing);
      Assert.That (item, Is.EqualTo ("abc"));
    }

    [Test]
    public void ShouldThrowForSecondSet ()
    {
      // Arrange
      var slot = Slot.New<string> ();

      // Act
      slot.Set ("first");
      TestDelegate action = () => slot.Set ("second");

      // Assert
      Assert.That (slot.HasItem, Is.True);

      Assert.That (action, Throws.InvalidOperationException
        .With.Message.EqualTo ("Item already set"));
    }

    [Test]
    public void ShouldUseProvidedItemNameForExceptionMessages ()
    {
      // Arrange
      var slot = Slot.New<string> ("blub");

      // Act
      TestDelegate getWithNoItem = () => slot.Get ();
      TestDelegate setTooOften = () => slot.Set ("second");

      // Assert
      Assert.That (getWithNoItem, Throws.InvalidOperationException
        .With.Message.EqualTo ("blub not set"));

      slot.Set ("first");

      Assert.That (setTooOften, Throws.InvalidOperationException
        .With.Message.EqualTo ("blub already set"));
    }

    [Test]
    public void ShouldAlsoWorkWithValueTypes ()
    {
      // Arrange
      var slot = Slot.New<int> ();

      // Act
      slot.Set (7);
      int item = slot.Get();

      // Assert
      Assert.That (item, Is.EqualTo (7));

      Assert.That (slot.HasItem, Is.True);
      Assert.That (slot.HasDefault, Is.False);
      Assert.That (slot.CanGet, Is.True);
    }

    [Test]
    public void ShouldAlsoWorkWithValueTypesAndDefaults ()
    {
      // Arrange
      var slot = Slot.WithDefault ('u');

      // Act
      char item = slot.Get();

      // Assert
      Assert.That (item, Is.EqualTo ('u'));

      Assert.That (slot.HasItem, Is.False);
      Assert.That (slot.HasDefault, Is.True);
      Assert.That (slot.CanGet, Is.True);
    }

    [Test]
    public void ShouldThrowForNullByDefault ()
    {
      // Arrange 
      var slot = Slot.New<string> ("blub");

      // Act
      TestDelegate action = () => slot.Set (null);

      // Assert
      Assert.That (action, Throws.TypeOf<ArgumentNullException>()
        .With.Property ("ParamName").EqualTo ("blub"));
    }

    [Test]
    public void ShouldNotThrowForNullIfConfigured ()
    {
      // Arrange 
      var slot = Slot.New<string> (allowsNull: true);

      // Act
      TestDelegate action = () => slot.Set (null);

      // Assert
      Assert.That (action, Throws.Nothing);
    }
  }
}