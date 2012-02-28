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
      var slot = Slot.New<string>();

      // Act
      TestDelegate action = () => slot.Get();

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
      var slot = Slot.New<string>();

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
      var slot = Slot.New<string>();

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
      TestDelegate getWithNoItem = () => slot.Get();
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
      var slot = Slot.New<int>();

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