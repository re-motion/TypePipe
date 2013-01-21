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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class KeepConstructorsTest : TypeAssemblerIntegrationTestBase
  {
    private const BindingFlags c_ctorBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    [Test]
    public void KeepPublicAndProtectedConstructors ()
    {
      Assert.That (
          GetCtorSignatures (typeof (DomainType)),
          Is.EquivalentTo (new[] { ".ctor(System.String)", ".ctor()", ".ctor(Double)", ".ctor(Int32)", ".ctor(System.String, Int32)" }));

      var type = AssembleType<DomainType> (proxyType => { });

      Assert.That (type, Is.Not.SameAs (typeof (DomainType))); // No shortcut for zero modifications (yet).
      Assert.That (GetCtorSignatures (type), Is.EquivalentTo (new[] { ".ctor(System.String)", ".ctor()", ".ctor(Double)" }));

      CheckConstructorUsage ("public", -10, MethodAttributes.Public, type, "public");
      CheckConstructorUsage ("protected", -20, MethodAttributes.Family, type);
      CheckConstructorUsage ("protected internal", -30, MethodAttributes.Family, type, 17.7);
    }

    [Test]
    public void ConstructorWithOutAndRefParameters ()
    {
      var type = AssembleType<DomainTypeWithWeirdCtor> (proxyType => { });

      Assert.That (GetCtorSignatures (type), Is.EquivalentTo (new[] { ".ctor(Int32 ByRef, System.String ByRef)" }));
      
      var ctor = type.GetConstructors().Single();
      var parameters = new object[] { null, "in" };
      var instance = ctor.Invoke (parameters);

      Assert.That (instance, Is.Not.Null);
      Assert.That (instance.GetType(), Is.SameAs (type));
      Assert.That (parameters[0], Is.EqualTo (88));
      Assert.That (parameters[1], Is.EqualTo ("in and out"));
    }

    private void CheckConstructorUsage (
        string expectedVal1,
        int expectedVal2,
        MethodAttributes expectedVisibility,
        Type generatedType,
        params object[] ctorArguments)
    {
      var ctorParameterTypes = ctorArguments.Select (arg => arg.GetType()).ToArray();
      var constructor = generatedType.GetConstructor (c_ctorBindingFlags, null, ctorParameterTypes, null);
      var additionalMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

      Assert.That (constructor.Attributes, Is.EqualTo (expectedVisibility | additionalMethodAttributes));

      var instance = (DomainType) constructor.Invoke (ctorArguments);

      Assert.That (instance, Is.Not.Null);
      Assert.That (instance.GetType(), Is.SameAs (generatedType));
      Assert.That (instance.StringProperty, Is.EqualTo (expectedVal1));
      Assert.That (instance.IntProperty, Is.EqualTo (expectedVal2));
    }

    private IEnumerable<string> GetCtorSignatures (Type type)
    {
      return type.GetConstructors (c_ctorBindingFlags).Select (ctor => ctor.ToString().Replace ("Void ", ""));
    }

// ReSharper disable MemberCanBePrivate.Global
    public class DomainType
// ReSharper restore MemberCanBePrivate.Global
    {
      public string StringProperty { get; set; }
      public int IntProperty { get; set; }

      public DomainType (string s)
      {
        StringProperty = s;
        IntProperty = -10;
      }

      protected DomainType ()
      {
        StringProperty = "protected";
        IntProperty = -20;
      }

      protected internal DomainType (double x)
      {
        StringProperty = "protected internal";
        IntProperty = -30;
        Dev.Null = x;
      }

      internal DomainType (int i)
      {
        StringProperty = "internal";
        IntProperty = i;
      }

// ReSharper disable UnusedMember.Local
      private DomainType (string s, int i)
// ReSharper restore UnusedMember.Local
      {
        StringProperty = s;
        IntProperty = i;
      }
    }

    public class DomainTypeWithWeirdCtor
    {
      public DomainTypeWithWeirdCtor (out int i, ref string s)
      {
        i = 88;
        s = s + " and out";
      }
    }
  }
}