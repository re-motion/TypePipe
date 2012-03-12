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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ExistingTypeInfoTest
  {
    private ExistingTypeInfo _existingTypeInfo;

    [SetUp]
    public void SetUp ()
    {
      _existingTypeInfo = ExistingTypeInfoObjectMother.Create (originalType: (typeof (ExampleType)));
    }

    [Test]
    public void Initialization_ThrowsIfOriginalTypeCannotBeSubclassed ()
    {
      var msg = "Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic parameters and "
              + "must have an accessible constructor.\r\nParameter name: originalType";
      // sealed
      Assert.That (() => Create (typeof (string)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // interface
      Assert.That (() => Create (typeof (IDisposable)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // value type
      Assert.That (() => Create (typeof (int)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // enum
      Assert.That (() => Create (typeof (ExpressionType)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // delegate
      Assert.That (() => Create (typeof (Delegate)), Throws.ArgumentException.With.Message.EqualTo (msg));
      Assert.That (() => Create (typeof (MulticastDelegate)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // open generics
      Assert.That (() => Create (typeof (List<>)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // closed generics
      Assert.That (() => Create (typeof (List<int>)), Throws.Nothing);
      // no accessible co
      Assert.That (() => Create (typeof (BlockExpression)), Throws.ArgumentException.With.Message.EqualTo (msg));
    }

    [Test]
    public void OriginalType ()
    {
      Assert.That (_existingTypeInfo.OriginalType, Is.SameAs (typeof (ExampleType)));
    }

    [Test]
    public void GetBaseType ()
    {
      Assert.That (_existingTypeInfo.GetBaseType(), Is.EqualTo (typeof (ExampleType).BaseType));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_existingTypeInfo.GetAttributeFlags(), Is.EqualTo (typeof(ExampleType).Attributes));
    }

    [Test]
    public void GetInterfaces ()
    {
      Assert.That (_existingTypeInfo.GetInterfaces(), Is.EquivalentTo (new[] { typeof (IDisposable) }));
    }

    [Test]
    public void GetFields ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

      var fieldNames = _existingTypeInfo.GetFields (bindingFlags).Select(field => field.Name);
      Assert.That (fieldNames, Is.EquivalentTo (new[] { "_firstField", "_secondField" }));
    }

    [Test]
    public void GetConstructors ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

      var ctorSignatures = _existingTypeInfo.GetConstructors (bindingFlags).Select (ctor => ctor.GetParameters().Select (p => p.ParameterType));
      Assert.That (ctorSignatures, Is.EqualTo (new[] { new Type[0], new[] { typeof (int) } }));
    }

    public class ExampleType : IDisposable
    {
      public ExampleType ()
      {
      }

      private ExampleType (int i)
      {
      }

      public void Dispose ()
      {
      }

      protected string _firstField;
      public static object _secondField;
    }

    private ExistingTypeInfo Create (Type originalType)
    {
      return ExistingTypeInfoObjectMother.Create (originalType: originalType);
    }
  }
}