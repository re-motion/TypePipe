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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataWithInheritanceTest
  {
    [Test]
    public void GetCustomAttributes_Type_NoInheritance ()
    {
      var customAttributes = TypePipeCustomAttributeData.GetCustomAttributes ((MemberInfo) typeof (DomainType), false);
      var customAttributeTypes = customAttributes.Select (a => a.Constructor.DeclaringType).ToArray ();

      Assert.That (customAttributeTypes, Is.Empty);
    }

    [Test]
    public void GetCustomAttributes_Type_Inheritance ()
    {
      var customAttributes = TypePipeCustomAttributeData.GetCustomAttributes ((MemberInfo) typeof (DomainType), true);
      var customAttributeTypes = customAttributes.Select (a => a.Constructor.DeclaringType).ToArray();
      Assert.That (customAttributeTypes, Is.EqualTo (new[] { typeof (InheritableAttribute) }));
    }


    [Inheritable, NonInheritable]
    class BaseType { }
    class DomainType : BaseType { }


    [AttributeUsage (AttributeTargets.All, Inherited = true)]
    public sealed class InheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = false)]
    public sealed class NonInheritableAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public sealed class InheritableAllowMultipleAttribute : Attribute
    {
      public InheritableAllowMultipleAttribute (string arg) { Dev.Null = arg; }
    }

    [AttributeUsage (AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class InheritableNonMultipleAttribute : Attribute
    {
      public InheritableNonMultipleAttribute (string arg) { Dev.Null = arg; }
    }
  }
}