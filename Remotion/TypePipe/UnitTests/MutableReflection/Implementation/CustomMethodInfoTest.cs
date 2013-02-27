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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomMethodInfoTest
  {
    private CustomType _declaringType;
    private string _name;
    private MethodAttributes _attributes;
    private ParameterInfo _returnParameter;

    private TestableCustomMethodInfo _method;

    private Type _typeArgument;
    private CustomMethodInfo _genericDefinition;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _name = "abc";
      _attributes = (MethodAttributes) 7;
      _returnParameter = CustomParameterInfoObjectMother.Create();

      _method = new TestableCustomMethodInfo (_declaringType, _name, _attributes)
                {
                    TypeArguments = Type.EmptyTypes,
                    ReturnParameter_ = _returnParameter
                };

      _typeArgument = ReflectionObjectMother.GetSomeType();
      _genericDefinition = CustomMethodInfoObjectMother.Create (typeArguments: new[] { _typeArgument });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_method.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_method.Name, Is.EqualTo (_name));
      Assert.That (_method.Attributes, Is.EqualTo (_attributes));
      Assert.That (_method.ReturnParameter, Is.SameAs (_returnParameter));
      Assert.That (_method.ReturnTypeCustomAttributes, Is.SameAs (_returnParameter));
      Assert.That (_method.ReturnType, Is.SameAs (_returnParameter.ParameterType));
      Assert.That (_method.IsGenericMethod, Is.False);
      Assert.That (_method.IsGenericMethodDefinition, Is.False);
      Assert.That (_method.ContainsGenericParameters, Is.False);
    }

    [Test]
    public void Initialization_GenericMethodDefinition ()
    {
      Assert.That (_genericDefinition.IsGenericMethod, Is.True);
      Assert.That (_genericDefinition.IsGenericMethodDefinition, Is.True);
      Assert.That (_genericDefinition.ContainsGenericParameters, Is.True);
    }

    [Test]
    public void CallingConvention ()
    {
      var instanceMethod = CustomMethodInfoObjectMother.Create (attributes: 0);
      var staticMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (instanceMethod.CallingConvention, Is.EqualTo (CallingConventions.Standard | CallingConventions.HasThis));
      Assert.That (staticMethod.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void ReturnType ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      _method.ReturnParameter_ = CustomParameterInfoObjectMother.Create (type: type);

      Assert.That (_method.ReturnType, Is.SameAs (type));
    }

    [Test]
    public void GetGenericMethodDefinition ()
    {
      Assert.That (_genericDefinition.GetGenericMethodDefinition(), Is.SameAs (_genericDefinition));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "GetGenericMethodDefinition can only be called on generic methods (IsGenericMethod must be true).")]
    public void GetGenericMethodDefinition_ThrowsIfNonGeneric ()
    {
      Assert.That (_method.IsGenericMethod, Is.False);
      _method.GetGenericMethodDefinition();
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
              CustomParameterInfoObjectMother.Create (type: typeof (int)),
              CustomParameterInfoObjectMother.Create (type: typeof (string).MakeByRefType())
          };
      var returnParameter = CustomParameterInfoObjectMother.Create (type: typeof (string));
      var method = CustomMethodInfoObjectMother.Create (name: "Xxx", returnParameter: returnParameter, parameters: parameters);

      Assert.That (method.ToString (), Is.EqualTo ("String Xxx(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var method = CustomMethodInfoObjectMother.Create (
          declaringType: ProxyTypeObjectMother.Create (name: "AbcProxy"),
          name: "Xxx",
          returnParameter: CustomParameterInfoObjectMother.Create (position: -1, type: typeof (void)),
          parameters: new[] { CustomParameterInfoObjectMother.Create (type: typeof (int)) });

      var expected = "TestableCustomMethod = \"Void Xxx(Int32)\", DeclaringType = \"AbcProxy\"";
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
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.ReflectedType, "ReflectedType");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.MetadataToken, "MetadataToken");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.Module, "Module");
      UnsupportedMemberTestHelper.CheckProperty (() => Dev.Null = _method.MethodHandle, "MethodHandle");

      UnsupportedMemberTestHelper.CheckMethod (() => _method.MakeGenericMethod (Type.EmptyTypes), "MakeGenericMethod");
      UnsupportedMemberTestHelper.CheckMethod (() => _method.GetMethodBody(), "GetMethodBody");
      UnsupportedMemberTestHelper.CheckMethod (() => _method.Invoke (null, 0, null, null, null), "Invoke");
    }
  }
}