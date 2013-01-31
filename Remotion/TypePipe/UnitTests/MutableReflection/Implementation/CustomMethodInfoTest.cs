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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomMethodInfoTest
  {
    private CustomType _declaringType;
    private string _name;
    private MethodAttributes _attributes;
    private TestableCustomMethodInfo _method;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _name = "abc";
      _attributes = (MethodAttributes) 7;

      _method = new TestableCustomMethodInfo (_declaringType, _name, _attributes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_method.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_method.Name, Is.EqualTo (_name));
      Assert.That (_method.Attributes, Is.EqualTo (_attributes));
    }

    [Test]
    public void CallingConvention ()
    {
      var instanceMethod = MutableMethodInfoObjectMother.Create (attributes: 0);
      var staticMethod = MutableMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (instanceMethod.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (staticMethod.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      _method.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (_method.GetCustomAttributes (false).Select (a => a.GetType ()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (_method.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_method.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_method.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var parameters =
          new[]
          {
              new ParameterDeclaration (typeof (int), "p1"),
              new ParameterDeclaration (typeof (string).MakeByRefType(), "p2", ParameterAttributes.Out)
          };
      var method = MutableMethodInfoObjectMother.Create (name: "Xxx", returnType: typeof (string), parameters: parameters);

      Assert.That (method.ToString (), Is.EqualTo ("String Xxx(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var method = MutableMethodInfoObjectMother.Create (
          declaringType: ProxyTypeObjectMother.Create (name: "AbcProxy"),
          name: "Xxx",
          returnType: typeof (void),
          parameters: new[] { new ParameterDeclaration (typeof (int), "p1") });

      var expected = "MutableMethod = \"Void Xxx(Int32)\", DeclaringType = \"AbcProxy\"";
      Assert.That (method.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void VirtualMethodsImplementedByMethodInfo ()
    {
      // None of these members should throw an exception 
      Dev.Null = _method.MemberType;
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.MetadataToken, "MetadataToken");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.Module, "Module");

      UnsupportedMemberTestHelper.CheckMethod (() => _method.Invoke (null, 0, null, null, null), "Invoke");
    }
  }
}