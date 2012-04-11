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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingTypeDescriptorTest
  {
    [Test]
    public void Create ()
    {
      var originalType = typeof (ExampleType);
      var memberFilterMock = MockRepository.GenerateStrictMock<IMemberFilter> ();

      // Fields
      var allFields = typeof (ExampleType).GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var filteredFields = new[] { ReflectionObjectMother.GetSomeField () };
      memberFilterMock.Expect (mock => mock.FilterFields (allFields)).Return (filteredFields);

      // Ctors
      var allInstanceConstructors = typeof (ExampleType).GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var filteredCtors = new[] { ReflectionObjectMother.GetSomeConstructor () };
      memberFilterMock.Expect (mock => mock.FilterConstructors (allInstanceConstructors)).Return (filteredCtors).Repeat.AtLeastOnce();

      // Methods
      var allMethods = typeof (ExampleType).GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var filteredMethods = new[] { ReflectionObjectMother.GetSomeMethod() };
      memberFilterMock.Expect (mock => mock.FilterMethods (allMethods)).Return (filteredMethods);

      var descriptor = UnderlyingTypeDescriptor.Create (originalType, memberFilterMock);

      memberFilterMock.VerifyAllExpectations();

      Assert.That (descriptor.UnderlyingSystemType, Is.SameAs (typeof (ExampleType)));
      Assert.That (descriptor.BaseType, Is.EqualTo (typeof (ExampleType).BaseType));
      Assert.That (descriptor.Name, Is.EqualTo (originalType.Name));
      Assert.That (descriptor.Namespace, Is.EqualTo (originalType.Namespace));
      Assert.That (descriptor.FullName, Is.EqualTo (originalType.FullName));
      Assert.That (descriptor.StringRepresentation, Is.EqualTo (originalType.ToString ()));
      Assert.That (descriptor.Attributes, Is.EqualTo (typeof (ExampleType).Attributes));
      Assert.That (descriptor.Interfaces, Is.EquivalentTo (new[] { typeof (IDisposable) }));
      Assert.That (descriptor.Fields, Is.EqualTo (filteredFields));
      Assert.That (descriptor.Constructors, Is.EqualTo (filteredCtors));
      Assert.That (descriptor.Methods, Is.EqualTo (filteredMethods));
    }

    [Test]
    public void Create_ThrowsIfOriginalTypeCannotBeSubclassed ()
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
      // no accessible ctor
      Assert.That (() => Create (typeof (ExampleType), returnConstructors: false), Throws.ArgumentException.With.Message.EqualTo (msg));
    }

    private UnderlyingTypeDescriptor Create (Type originalType, bool returnConstructors = true)
    {
      var memberFilter = MockRepository.GenerateStub<IMemberFilter> ();
      memberFilter
          .Stub (stub => stub.FilterConstructors (Arg<IEnumerable<ConstructorInfo>>.Is.Anything))
          .Do ((Func<IEnumerable<ConstructorInfo>, IEnumerable<ConstructorInfo>>) (a => returnConstructors ? a : new ConstructorInfo[0]));
      memberFilter
          .Stub (stub => stub.FilterFields (Arg<IEnumerable<FieldInfo>>.Is.Anything))
          .Do ((Func<IEnumerable<FieldInfo>, IEnumerable<FieldInfo>>) (a => a));
      memberFilter
          .Stub (stub => stub.FilterMethods (Arg<IEnumerable<MethodInfo>>.Is.Anything))
          .Do ((Func<IEnumerable<MethodInfo>, IEnumerable<MethodInfo>>) (a => a));

      return UnderlyingTypeDescriptor.Create (originalType, memberFilter);
    }

// ReSharper disable MemberCanBePrivate.Global
    public class ExampleType : IDisposable
// ReSharper restore MemberCanBePrivate.Global
    {
      public int PublicField;
      private int _nonPublicField = 0;
      public static int StaticField;

      // Public ctor
      public ExampleType ()
      {
      }

      // Non-public ctor
      protected ExampleType (int i)
      {
        Dev.Null = i;
      }

      static ExampleType ()
      {
        Dev.Null = 17;
      }

      public void Dispose ()
      {
        Dev.Null = _nonPublicField;
      }

// ReSharper disable UnusedMember.Local
      private void PrivateMethod () { }
// ReSharper restore UnusedMember.Local
      protected void ProtecteMethod () { }
      public static void PublicStaticMethod () { }
    }
  }
}