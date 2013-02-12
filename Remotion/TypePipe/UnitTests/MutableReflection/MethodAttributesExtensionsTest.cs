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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MethodAttributesExtensionsTest
  {
    [Test]
    public void AdjustVisibilityForAssemblyBoundaries ()
    {
      Assert.That (MethodAttributes.Public.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.Public));
      Assert.That (MethodAttributes.FamORAssem.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.Family));
      Assert.That (MethodAttributes.Family.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.Family));
      Assert.That (MethodAttributes.FamANDAssem.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.FamANDAssem));
      Assert.That (MethodAttributes.Assembly.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.Assembly));
      Assert.That (MethodAttributes.Private.AdjustVisibilityForAssemblyBoundaries(), Is.EqualTo (MethodAttributes.Private));
    }

    [Test]
    public void ChangeVisibility ()
    {
      var originalAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

      var adjustedAttributes = originalAttributes.ChangeVisibility (MethodAttributes.Private);

      var expectedAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Virtual;
      Assert.That (adjustedAttributes, Is.EqualTo (expectedAttributes));
    }

    [Test]
    public void IsSet ()
    {
      var attributes = (MethodAttributes) 3;

      // Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 0), Is.False); // wrong usage
      Assert.That (attributes.IsSet ((MethodAttributes) 1), Is.True);
      Assert.That (attributes.IsSet ((MethodAttributes) 2), Is.True);
      Assert.That (attributes.IsSet ((MethodAttributes) 3), Is.True);
      Assert.That (attributes.IsSet ((MethodAttributes) 4), Is.False);
    }

    [Test]
    public void Set ()
    {
      var attributes = (MethodAttributes) 2;

      Assert.That (attributes.Set ((MethodAttributes) 6), Is.EqualTo ((MethodAttributes) 6));
      Assert.That (attributes.Set (0), Is.EqualTo ((MethodAttributes) 2));
    }

    [Test]
    public void Unset ()
    {
      var attributes = (MethodAttributes) 3;

      Assert.That (attributes.Unset ((MethodAttributes) 2), Is.EqualTo ((MethodAttributes) 1));
      Assert.That (attributes.Unset (0), Is.EqualTo ((MethodAttributes) 3));
    }
  }
}