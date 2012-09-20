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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using System.Collections.Generic;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingTypeDescriptorTest
  {
    [Test]
    public void Create_ForExisting ()
    {
      var originalType = typeof (ExampleType);

      var allFields = typeof (ExampleType).GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var instanceConstructors = typeof (ExampleType).GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var allMethods = typeof (ExampleType).GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var genericMethod = typeof (ExampleType).GetMethod ("GenericMethod");
      var nonGenericMethods = allMethods.Except (new[] { genericMethod });

      var descriptor = UnderlyingTypeDescriptor.Create (originalType);

      Assert.That (descriptor.UnderlyingSystemMember, Is.SameAs (typeof (ExampleType)));
      Assert.That (descriptor.BaseType, Is.EqualTo (typeof (ExampleType).BaseType));
      Assert.That (descriptor.Name, Is.EqualTo (originalType.Name));
      Assert.That (descriptor.Namespace, Is.EqualTo (originalType.Namespace));
      Assert.That (descriptor.FullName, Is.EqualTo (originalType.FullName));
      Assert.That (descriptor.Attributes, Is.EqualTo (typeof (ExampleType).Attributes));
      Assert.That (descriptor.Interfaces, Is.EquivalentTo (new[] { typeof (IDisposable) }));
      Assert.That (descriptor.Fields, Is.EqualTo (allFields));
      Assert.That (descriptor.Constructors, Is.EqualTo (instanceConstructors));
      Assert.That (descriptor.Methods, Is.EqualTo (nonGenericMethods));
      Assert.That (
          descriptor.CustomAttributeDataProvider.Invoke ().Select (ad => ad.Constructor.DeclaringType),
          Is.EquivalentTo (new[] { typeof (AbcAttribute), typeof (DefAttribute) }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Original type must not be another mutable type.\r\nParameter name: originalType")]
    public void Create_ForExisting_ThrowsIfOriginalTypeIsMutableType ()
    {
      Create (MutableTypeObjectMother.Create());
    }

    [Test]
    public void Create_ForExisting_ThrowsIfOriginalTypeCannotBeSubclassed ()
    {
      var msg = "Original type must not be sealed, abstract, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, "
                + "a generic parameter, contain generic parameters and must have an accessible constructor.\r\nParameter name: originalType";
      // sealed
      Assert.That (() => Create (typeof (string)), Throws.ArgumentException.With.Message.EqualTo (msg));
      // abstract
      Assert.That (() => Create (typeof (AbstractDomainType)), Throws.ArgumentException.With.Message.EqualTo (msg));
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
      // generic parameter
      Assert.That (() => Create (typeof (List<>).GetGenericArguments().Single()), Throws.ArgumentException.With.Message.EqualTo (msg));
      // array
      Assert.That (() => Create (typeof (int).MakeArrayType()), Throws.ArgumentException.With.Message.EqualTo (msg));
      // by ref
      Assert.That (() => Create (typeof (int).MakeByRefType()), Throws.ArgumentException.With.Message.EqualTo (msg));
      // pointer
      Assert.That (() => Create (typeof (int).MakePointerType ()), Throws.ArgumentException.With.Message.EqualTo (msg));
      // no accessible ctor
      Assert.That (() => Create (typeof (TypeWithoutAccessibleConstructor)), Throws.ArgumentException.With.Message.EqualTo (msg));
    }

    private UnderlyingTypeDescriptor Create (Type originalType)
    {
      return UnderlyingTypeDescriptor.Create (originalType);
    }

// ReSharper disable MemberCanBePrivate.Global
    [Abc, Def]
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

      public T1 GenericMethod<T1, T2> (T2 t2)
      {
        return default (T1);
      }
    }

    public class TypeWithoutAccessibleConstructor
    {
// ReSharper disable UnusedMember.Local
      private TypeWithoutAccessibleConstructor (string s) { }
// ReSharper restore UnusedMember.Local
      internal TypeWithoutAccessibleConstructor () { }
    }

    public abstract class AbstractDomainType { }

    public class AbcAttribute : Attribute { }
    public class DefAttribute : Attribute { }
  }
}