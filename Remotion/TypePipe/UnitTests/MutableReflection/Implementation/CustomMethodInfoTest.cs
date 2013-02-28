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
    private MethodInfo _genericMethodUnderlyingDefinition;
    private CustomMethodInfo _genericMethod;

    private Type _typeParameter;
    private CustomMethodInfo _genericMethodDefinition;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create();
      _name = "abc";
      _attributes = (MethodAttributes) 7;
      _returnParameter = CustomParameterInfoObjectMother.Create();

      _method = new TestableCustomMethodInfo (_declaringType, _name, _attributes, false, null, Type.EmptyTypes)
                {
                    ReturnParameter_ = _returnParameter
                };

      _typeArgument = ReflectionObjectMother.GetSomeType();
      _genericMethodUnderlyingDefinition = ReflectionObjectMother.GetSomeGenericMethodDefinition();
      _genericMethod = CustomMethodInfoObjectMother.Create (
          isGenericMethod: true, genericMethodDefintion: _genericMethodUnderlyingDefinition, typeArguments: new[] { _typeArgument });

      _typeParameter = ReflectionObjectMother.GetSomeGenericParameter();
      _genericMethodDefinition = CustomMethodInfoObjectMother.Create (isGenericMethod: true, typeArguments: new[] { _typeParameter });
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
      Assert.That (_method.GetGenericArguments(), Is.Empty);
      Assert.That (
          () => _method.GetGenericMethodDefinition (),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "GetGenericMethodDefinition can only be called on generic methods (IsGenericMethod must be true)."));
    }

    [Test]
    public void Initialization_GenericMethod ()
    {
      Assert.That (_genericMethod.IsGenericMethod, Is.True);
      Assert.That (_genericMethod.IsGenericMethodDefinition, Is.False);
      Assert.That (_genericMethod.ContainsGenericParameters, Is.False);
      Assert.That (_genericMethod.GetGenericArguments(), Is.EqualTo (new[] { _typeArgument }));
      Assert.That (_genericMethod.GetGenericMethodDefinition(), Is.SameAs (_genericMethodUnderlyingDefinition));
    }

    [Test]
    public void Initialization_GenericMethodDefinition ()
    {
      Assert.That (_genericMethodDefinition.IsGenericMethod, Is.True);
      Assert.That (_genericMethodDefinition.IsGenericMethodDefinition, Is.True);
      Assert.That (_genericMethodDefinition.ContainsGenericParameters, Is.True);
      Assert.That (_genericMethodDefinition.GetGenericArguments(), Is.EqualTo (new[] { _typeParameter }));
      Assert.That (_genericMethodDefinition.GetGenericMethodDefinition(), Is.SameAs (_genericMethodDefinition));
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

    [Ignore]
    [Test]
    public void MakeGenericMethod ()
    {
      var result = _genericMethodDefinition.MakeGenericMethod (_typeArgument);

      Assert.That (result.IsGenericMethod, Is.True);
      Assert.That (result.IsGenericMethodDefinition, Is.False);
      Assert.That (result.GetGenericMethodDefinition(), Is.SameAs (_genericMethodDefinition));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "MakeGenericMethod can only be called on generic method definitions (IsGenericMethodDefinition must be true).")]
    public void MakeGenericMethod_NoGenericMethodDefinition ()
    {
      _genericMethod.MakeGenericMethod();
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

      UnsupportedMemberTestHelper.CheckMethod (() => _method.GetMethodBody(), "GetMethodBody");
      UnsupportedMemberTestHelper.CheckMethod (() => _method.Invoke (null, 0, null, null, null), "Invoke");
    }
  }
}