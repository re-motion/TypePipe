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
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class EnumFlagsExtensionsTest
  {
    [Test]
    public void IsSet ()
    {
      var attributes = (TypeAttributes) 3;

      // Assert.That (TypeAttributes.IsSet (attributes, (TypeAttributes) 0), Is.False); // wrong usage
      Assert.That (attributes.IsSet ((TypeAttributes) 1), Is.True);
      Assert.That (attributes.IsSet ((TypeAttributes) 2), Is.True);
      Assert.That (attributes.IsSet ((TypeAttributes) 3), Is.True);
      Assert.That (attributes.IsSet ((TypeAttributes) 4), Is.False);
    }

    [Test]
    public void IsSet_Mask ()
    {
      var attributes = (TypeAttributes) 1;

      Assert.That (attributes.IsSet ((TypeAttributes) 2, (TypeAttributes) 0), Is.True);
      Assert.That (attributes.IsSet ((TypeAttributes) 1, (TypeAttributes) 0), Is.False);
      Assert.That (attributes.IsSet ((TypeAttributes) 1, (TypeAttributes) 1), Is.True);
      Assert.That (attributes.IsSet ((TypeAttributes) 2, (TypeAttributes) 1), Is.False);
    }

    [Test]
    public void IsUnset ()
    {
      var attributes = (TypeAttributes) 3;

      // Assert.That (TypeAttributes.IsUnset (attributes, (TypeAttributes) 0), Is.False); // wrong usage
      Assert.That (attributes.IsUnset ((TypeAttributes) 1), Is.False);
      Assert.That (attributes.IsUnset ((TypeAttributes) 2), Is.False);
      Assert.That (attributes.IsUnset ((TypeAttributes) 3), Is.False);
      Assert.That (attributes.IsUnset ((TypeAttributes) 4), Is.True);
    }

    [Test]
    public void Set ()
    {
      var attributes = (TypeAttributes) 2;

      Assert.That (attributes.Set ((TypeAttributes) 6), Is.EqualTo ((TypeAttributes) 6));
      Assert.That (attributes.Set ((TypeAttributes) 0), Is.EqualTo ((TypeAttributes) 2));
    }

    [Test]
    public void Unset ()
    {
      var attributes = (TypeAttributes) 3;

      Assert.That (attributes.Unset ((TypeAttributes) 2), Is.EqualTo ((TypeAttributes) 1));
      Assert.That (attributes.Unset ((TypeAttributes) 0), Is.EqualTo ((TypeAttributes) 3));
    }
  }
}