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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomConstructorInfoTest
  {
    private CustomType _declaringType;
    private MethodAttributes _attributes;

    private TestableCustomConstructorInfo _constructor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _attributes = (MethodAttributes) 7;

      _constructor = new TestableCustomConstructorInfo (_declaringType, _attributes);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_constructor.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_constructor.Attributes, Is.EqualTo (_attributes));
    }

    [Test]
    public void Name ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create (attributes: 0);
      var typeInitializer = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (constructor.Name, Is.EqualTo (".ctor"));
      Assert.That (typeInitializer.Name, Is.EqualTo (".cctor"));
    }

    [Test]
    public void CallingConvention ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create (attributes: 0);
      var typeInitializer = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (constructor.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (typeInitializer.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      _constructor.CustomAttributeDatas = new[] { CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute)) };

      Assert.That (_constructor.GetCustomAttributes (false).Select (a => a.GetType ()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (_constructor.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_constructor.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_constructor.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var ctor = MutableConstructorInfoObjectMother.Create (
          parameters:
              new[]
              {
                  new ParameterDeclaration (typeof (int), "p1"),
                  new ParameterDeclaration (typeof (string).MakeByRefType(), "p2", attributes: ParameterAttributes.Out)
              });

      Assert.That (ctor.ToString (), Is.EqualTo ("Void .ctor(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = ProxyTypeObjectMother.Create (name: "Abc");
      var ctor = MutableConstructorInfoObjectMother.Create (declaringType, parameters: new[] { new ParameterDeclaration (typeof (int), "p1") });

      var expected = "MutableConstructor = \"Void .ctor(Int32)\", DeclaringType = \"Abc\"";
      Assert.That (ctor.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.MethodHandle, "MethodHandle");
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.ReflectedType, "ReflectedType");

      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (null, 0, null, null, null), "Invoke");
      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (0, null, null, null), "Invoke");
    }
  }
}