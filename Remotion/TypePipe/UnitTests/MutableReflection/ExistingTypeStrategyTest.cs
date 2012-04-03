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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ExistingTypeStrategyTest
  {
    private IMemberFilter _memberFilterStub;
    private Type _originalType;
    private ExistingTypeStrategy _existingTypeStrategy;

    [SetUp]
    public void SetUp ()
    {
      _memberFilterStub = MockRepository.GenerateStub<IMemberFilter>();
      _originalType = typeof (ExampleType);
      _existingTypeStrategy = ExistingTypeStrategyObjectMother.Create (originalType: _originalType, memberFilter: _memberFilterStub);
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
    public void GetUnderlyingSystemType ()
    {
      Assert.That (_existingTypeStrategy.UnderlyingSystemType, Is.SameAs (typeof (ExampleType)));
    }

    [Test]
    public void GetBaseType ()
    {
      Assert.That (_existingTypeStrategy.BaseType, Is.EqualTo (typeof (ExampleType).BaseType));
    }

    [Test]
    public void GetName ()
    {
      Assert.That (_existingTypeStrategy.Name, Is.EqualTo (_originalType.Name));
    }

    [Test]
    public void GetNamespace ()
    {
      Assert.That (_existingTypeStrategy.Namespace, Is.EqualTo (_originalType.Namespace));
    }

    [Test]
    public void GetFullName ()
    {
      Assert.That (_existingTypeStrategy.FullName, Is.EqualTo (_originalType.FullName));
    }

    [Test]
    public void GetToStringRepresentation ()
    {
      Assert.That (_existingTypeStrategy.StringRepresentation, Is.EqualTo (_originalType.ToString()));
    }

    [Test]
    public void GetAttributeFlagsImpl ()
    {
      Assert.That (_existingTypeStrategy.Attributes, Is.EqualTo (typeof(ExampleType).Attributes));
    }

    [Test]
    public void GetInterfaces ()
    {
      Assert.That (_existingTypeStrategy.Interfaces, Is.EquivalentTo (new[] { typeof (IDisposable) }));
    }

    [Test]
    public void GetFields ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
      var allFields = typeof (ExampleType).GetFields(bindingFlags);
      var filteredFields = new FieldInfo[0];
      _memberFilterStub.Stub (stub => stub.FilterFields (allFields)).Return (filteredFields);

      var fields = _existingTypeStrategy.Fields;

      Assert.That (fields, Is.SameAs (filteredFields));
    }

    [Test]
    public void GetConstructors ()
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var allConstructors = typeof (ExampleType).GetConstructors(bindingFlags);
      var filteredCtors = new ConstructorInfo[0];
      _memberFilterStub.Stub (stub => stub.FilterConstructors (allConstructors)).Return (filteredCtors);

      var fields = _existingTypeStrategy.Constructors;

      Assert.That (fields, Is.SameAs (filteredCtors));
    }

    public class ExampleType : IDisposable
    {
      public int SomeField;

      public void Dispose ()
      {
      }
    }

    private ExistingTypeStrategy Create (Type originalType)
    {
      return ExistingTypeStrategyObjectMother.Create (originalType: originalType);
    }
  }
}