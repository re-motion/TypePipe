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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MethodAttributeUtilityTest
  {
    [Test]
    public void AdjustVisibility ()
    {
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.Public), Is.EqualTo (MethodAttributes.Public));
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.FamORAssem), Is.EqualTo (MethodAttributes.Family));
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.Family), Is.EqualTo (MethodAttributes.Family));
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.FamANDAssem), Is.EqualTo (MethodAttributes.FamANDAssem));
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.Assembly), Is.EqualTo (MethodAttributes.Assembly));
      Assert.That (MethodAttributeUtility.AdjustVisibility (MethodAttributes.Private), Is.EqualTo (MethodAttributes.Private));
    }

    [Test]
    public void ChangeVisibility ()
    {
      var originalAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

      var adjustedAttributes = MethodAttributeUtility.ChangeVisibility (originalAttributes, MethodAttributes.Private);

      var expectedAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Virtual;
      Assert.That (adjustedAttributes, Is.EqualTo (expectedAttributes));
    }

    [Test]
    public void IsSet ()
    {
      var attributes = (MethodAttributes) 3;

      // Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 0), Is.False); // wrong usage
      Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 1), Is.True);
      Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 2), Is.True);
      Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 3), Is.True);
      Assert.That (MethodAttributeUtility.IsSet (attributes, (MethodAttributes) 4), Is.False);
    }
  }
}