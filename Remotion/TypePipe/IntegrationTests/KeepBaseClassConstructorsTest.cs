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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class KeepBaseClassConstructorsTest : TypeAssemblerIntegrationTestBase
  {
    [Ignore("TODO 4694")]
    [Test]
    // TODO: Keep all or only accessible?
    public void KeepAllCtors ()
    {
      Assert.That (
          GetCtorSignatures (typeof (BaseClass)),
          Is.EquivalentTo (new[] { ".ctor(System.String)", ".ctor()", ".ctor(Int32)", ".ctor(System.String, Int32)" }));

      var type = AssembleType<BaseClass> (mutableType => { });

      Assert.That (type, Is.Not.SameAs (typeof (BaseClass))); // no shortcut for zero modifications (yet)
      //Assert.That (
      //    GetCtorSignatures (type),
      //    Is.EquivalentTo (new[] { ".ctor(System.String)", ".ctor()", ".ctor(Int32)", ".ctor(System.String, Int32)" }));

      //AssertConstructorUsage ("public", -10, type, true, "public");
      AssertConstructorUsage ("protected", -20, type, true);
      //AssertConstructorUsage ("interal", 37, type, false, 37);
      //AssertConstructorUsage ("private", 47, type, false, "private", 47);
    }

    public void AssertConstructorUsage (string expectedVal1, int expectedVal2, Type type, bool isPublic, params object[] ctorArguments)
    {
      var baseClass = (BaseClass) (isPublic
                                       ? PrivateInvoke.CreateInstancePublicCtor (type, ctorArguments)
                                       : PrivateInvoke.CreateInstanceNonPublicCtor (type, ctorArguments));

      Assert.That (baseClass, Is.Not.Null);
      Assert.That (baseClass.GetType(), Is.SameAs (type));
      Assert.That (baseClass.StringProperty, Is.EqualTo (expectedVal1));
      Assert.That (baseClass.IntProperty, Is.EqualTo (expectedVal2));
    }

    private IEnumerable<string> GetCtorSignatures (Type type)
    {
      return type.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .Select (ctor => ctor.ToString().Replace("Void ", ""))
          .ToArray(); // better error message
    }

    public class BaseClass
    {
      public string StringProperty { get; set; }
      public int IntProperty { get; set; }

      public BaseClass (string s)
      {
        StringProperty = s;
        IntProperty = -10;
      }

      protected BaseClass ()
      {
        StringProperty = "protected";
        IntProperty = -20;
      }

      internal BaseClass (int i)
      {
        StringProperty = "internal";
        IntProperty = i;
      }

      private BaseClass (string s, int i)
      {
        StringProperty = s;
        IntProperty = i;
      }
    }
  }
}