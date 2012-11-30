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
using Remotion.Development.UnitTesting.Reflection;
using System.Collections.Generic;
using Remotion.TypePipe.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Descriptors
{
  [TestFixture]
  public class TypeDescriptorTest
  {
    [Test]
    public void Create_ForExisting ()
    {
      var underlyingType = typeof (DomainType);

      var allFields = typeof (DomainType).GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var instanceConstructors = typeof (DomainType).GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var allMethods = typeof (DomainType).GetMethods (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      var genericMethod = typeof (DomainType).GetMethod ("GenericMethod");
      var nonGenericMethods = allMethods.Except (new[] { genericMethod });

      var descriptor = TypeDescriptor.Create (underlyingType);

      Assert.That (descriptor.UnderlyingSystemInfo, Is.SameAs (typeof (DomainType)));
      Assert.That (descriptor.DeclaringType, Is.EqualTo (typeof (TypeDescriptorTest)));
      Assert.That (descriptor.BaseType, Is.EqualTo (typeof (DomainType).BaseType));
      Assert.That (descriptor.Name, Is.EqualTo (underlyingType.Name));
      Assert.That (descriptor.Namespace, Is.EqualTo (underlyingType.Namespace));
      Assert.That (descriptor.FullName, Is.EqualTo (underlyingType.FullName));
      Assert.That (descriptor.Attributes, Is.EqualTo (typeof (DomainType).Attributes));

      Assert.That (descriptor.Interfaces, Is.EquivalentTo (new[] { typeof (IDisposable) }));
      Assert.That (descriptor.Fields, Is.EqualTo (allFields));
      Assert.That (descriptor.Constructors, Is.EqualTo (instanceConstructors));
      Assert.That (descriptor.Methods, Is.EqualTo (nonGenericMethods));

      Assert.That (
          descriptor.CustomAttributeDataProvider.Invoke().Select (ad => ad.Type),
          Is.EquivalentTo (new[] { typeof (AbcAttribute), typeof (DefAttribute) }));

      var interfaceMap = descriptor.InterfaceMappingProvider (typeof (IDisposable));
      var interfaceMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDisposable obj) => obj.Dispose());
      var implementation = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Dispose());
      Assert.That (interfaceMap.InterfaceType, Is.SameAs (typeof (IDisposable)));
      Assert.That (interfaceMap.TargetType, Is.SameAs (typeof (DomainType)));
      Assert.That (interfaceMap.InterfaceMethods, Is.EqualTo (new[] { interfaceMethod }));
      Assert.That (interfaceMap.TargetMethods, Is.EqualTo (new[] { implementation }));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Original type must not be another mutable type.\r\nParameter name: underlyingType")]
    public void Create_ForExisting_ThrowsIfUnderlyingTypeIsMutableType ()
    {
      Create (MutableTypeObjectMother.Create());
    }

    [Test]
    public void Create_ForExisting_ThrowsIfUnderlyingTypeCannotBeSubclassed ()
    {
      var msg = "Original type must not be sealed, an interface, a value type, an enum, a delegate, an array, a byref type, a pointer, "
                + "a generic parameter, contain generic parameters and must have an accessible constructor.\r\nParameter name: underlyingType";
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

    private TypeDescriptor Create (Type underlyingType)
    {
      return TypeDescriptor.Create (underlyingType);
    }

    [Abc, Def]
    public class DomainType : IDisposable
    {
      static DomainType () { }

      public int PublicField;
      private int _nonPublicField = 0;
      public static int StaticField;

      // Public ctor
      public DomainType ()
      {
      }

      // Non-public ctor
      protected DomainType (int i)
      {
        Dev.Null = i;
      }

      public void Dispose ()
      {
        Dev.Null = _nonPublicField;
      }

      private void PrivateMethod () { }
      protected void ProtecteMethod () { }
      public static void PublicStaticMethod () { }

      public T1 GenericMethod<T1, T2> (T2 t2)
      {
        return default (T1);
      }
    }

    public class TypeWithoutAccessibleConstructor
    {
      internal TypeWithoutAccessibleConstructor () { }
    }

    public class AbcAttribute : Attribute { }
    public class DefAttribute : Attribute { }
  }
}