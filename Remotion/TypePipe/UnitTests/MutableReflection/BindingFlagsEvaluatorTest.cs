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
  public class BindingFlagsEvaluatorTest
  {
    private BindingFlagsEvaluator _bindingFlagsEvaluator;

    [SetUp]
    public void SetUp ()
    {
      _bindingFlagsEvaluator = new BindingFlagsEvaluator();
    }

    [Test]
    public void HasRightVisibility_Public ()
    {
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Public, BindingFlags.Public), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamORAssem, BindingFlags.Public), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Family, BindingFlags.Public), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Assembly, BindingFlags.Public), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamANDAssem, BindingFlags.Public), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Private, BindingFlags.Public), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.PrivateScope, BindingFlags.Public), Is.False);
    }

    [Test]
    public void HasRightVisibility_NonPublic ()
    {
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Public, BindingFlags.NonPublic), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamORAssem, BindingFlags.NonPublic), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Family, BindingFlags.NonPublic), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Assembly, BindingFlags.NonPublic), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamANDAssem, BindingFlags.NonPublic), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Private, BindingFlags.NonPublic), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.PrivateScope, BindingFlags.NonPublic), Is.True);
    }
  }
}