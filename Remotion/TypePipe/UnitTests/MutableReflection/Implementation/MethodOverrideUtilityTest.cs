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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MethodOverrideUtilityTest
  {
    [Test]
    public void GetNameForExplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface obj) => obj.InterfaceMethod());

      var result = MethodOverrideUtility.GetNameForExplicitOverride (method);

      Assert.That (
          result,
          Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MethodOverrideUtilityTest.IDomainInterface.InterfaceMethod"));

      // Make sure that the explicit override naming scheme is equivalent to the naming scheme of explicit implementations in C#.
      Assert.That (typeof (DomainType).GetMethod (result, BindingFlags.NonPublic | BindingFlags.Instance), Is.Not.Null);
    }

    [Test]
    public void GetAttributesForExplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ToString ());
      Assert.That (
          method.Attributes,
          Is.EqualTo (MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig));

      var result = MethodOverrideUtility.GetAttributesForExplicitOverride (method);

      Assert.That (result, Is.EqualTo (MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig));
    }

    [Test]
    public void GetAttributesForImplicitOverride ()
    {
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.ProtecterOrInternalMethod());
      Assert.That (
          method.Attributes,
          Is.EqualTo (MethodAttributes.FamORAssem | MethodAttributes.Abstract | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig));

      var result = MethodOverrideUtility.GetAttributesForImplicitOverride (method);

      Assert.That (result, Is.EqualTo (MethodAttributes.Family | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig));
    }

    abstract class DomainType : IDomainInterface
    {
      protected internal abstract void ProtecterOrInternalMethod ();
      public abstract override string ToString ();

      void IDomainInterface.InterfaceMethod () { }
    }

    interface IDomainInterface
    {
      void InterfaceMethod ();
    }
  }
}