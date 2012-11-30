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
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
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
    public void HasRightAttributes_MethodAttributes ()
    {
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (MethodAttributes.Public, BindingFlags.Public | BindingFlags.Instance), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (MethodAttributes.Public, BindingFlags.NonPublic | BindingFlags.Instance), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (MethodAttributes.Public, BindingFlags.Public | BindingFlags.Static), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (MethodAttributes.Public, BindingFlags.NonPublic | BindingFlags.Static), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (MethodAttributes.Public, BindingFlags.Public), Is.False);
    }

    [Test]
    public void HasRightAttributes_FieldAttributes ()
    {
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (FieldAttributes.Public, BindingFlags.Public | BindingFlags.Instance), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (FieldAttributes.Public, BindingFlags.NonPublic | BindingFlags.Instance), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (FieldAttributes.Public, BindingFlags.Public | BindingFlags.Static), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (FieldAttributes.Public, BindingFlags.NonPublic | BindingFlags.Static), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightAttributes (FieldAttributes.Public, BindingFlags.Public), Is.False);
    }

    [Test]
    public void HasRightAttributes_AssertThatFieldAttributesHaveSameValuesAsMethodAttributes ()
    {
      Assert.That ((int) FieldAttributes.Public, Is.EqualTo((int) MethodAttributes.Public));
      Assert.That ((int) FieldAttributes.FamORAssem, Is.EqualTo ((int) MethodAttributes.FamORAssem));
      Assert.That ((int) FieldAttributes.Family, Is.EqualTo ((int) MethodAttributes.Family));
      Assert.That ((int) FieldAttributes.Assembly, Is.EqualTo ((int) MethodAttributes.Assembly));
      Assert.That ((int) FieldAttributes.FamANDAssem, Is.EqualTo ((int) MethodAttributes.FamANDAssem));
      Assert.That ((int) FieldAttributes.Private, Is.EqualTo ((int) MethodAttributes.Private));
      Assert.That ((int) FieldAttributes.PrivateScope, Is.EqualTo ((int) MethodAttributes.PrivateScope));

      Assert.That ((int) FieldAttributes.Static, Is.EqualTo ((int) MethodAttributes.Static));
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

    [Test]
    public void HasRightVisibility_Both ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Public, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamORAssem, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Family, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Assembly, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.FamANDAssem, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.Private, bindingFlags), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightVisibility (MethodAttributes.PrivateScope, bindingFlags), Is.True);
    }

    [Test]
    public void HasRightInstanceOrStaticFlag ()
    {
      MethodAttributes methodAttributesInstance = 0;
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesInstance, BindingFlags.Instance), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesInstance, BindingFlags.Static), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesInstance, BindingFlags.Instance | BindingFlags.Static), Is.True);

      MethodAttributes methodAttributesStatic = MethodAttributes.Static;
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesStatic, BindingFlags.Instance), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesStatic, BindingFlags.Static), Is.True);
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesStatic, BindingFlags.Instance | BindingFlags.Static), Is.True);
    }

    [Test]
    public void HasRightInstanceOrStaticFlag_BindingFlagsNotInstanceNorStatic ()
    {
      MethodAttributes methodAttributesInstance = 0;
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (methodAttributesInstance, BindingFlags.Default), Is.False);
      Assert.That (_bindingFlagsEvaluator.HasRightInstanceOrStaticFlag (MethodAttributes.Static, BindingFlags.Default), Is.False);
    }
  }
}