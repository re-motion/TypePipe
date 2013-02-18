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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutablePropertyInfoTest
  {
    private ProxyType _declaringType;
    private string _name;
    private PropertyAttributes _attributes;
    private Type _type;
    private ParameterDeclaration[] _indexParameters;
    private MutableMethodInfo _getMethod;
    private MutableMethodInfo _setMethod;

    private MutablePropertyInfo _property;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ProxyTypeObjectMother.Create();
      _name = "Property";
      _attributes = (PropertyAttributes) 7;
      _type = ReflectionObjectMother.GetSomeType();
      _indexParameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      _getMethod = MutableMethodInfoObjectMother.Create (returnType: _type, parameters: _indexParameters);
      _setMethod = MutableMethodInfoObjectMother.Create (parameters: _indexParameters.Concat (ParameterDeclarationObjectMother.Create (_type)));

      _property = new MutablePropertyInfo (_declaringType, _name, _attributes, _getMethod, _setMethod);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_property.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_property.Name, Is.EqualTo (_name));
      Assert.That (_property.Attributes, Is.EqualTo (_attributes));
      Assert.That (_property.PropertyType, Is.SameAs (_type));
      Assert.That (_property.MutableGetMethod, Is.SameAs (_getMethod));
      Assert.That (_property.MutableSetMethod, Is.SameAs (_setMethod));

      var actualIndexParameters = _property.GetIndexParameters();
      Assert.That (actualIndexParameters, Has.Length.EqualTo (2));
      CheckParameter (actualIndexParameters[0], _property, 0, _indexParameters[0].Name, _indexParameters[0].Type, _indexParameters[0].Attributes);
      CheckParameter (actualIndexParameters[1], _property, 1, _indexParameters[1].Name, _indexParameters[1].Type, _indexParameters[1].Attributes);
    }

    [Test]
    public void Initialization_ReadOnly ()
    {
      var property = new MutablePropertyInfo (_declaringType, _name, _attributes, getMethod: _getMethod, setMethod: null);

      Assert.That (property.MutableSetMethod, Is.Null);
    }

    [Test]
    public void Initialization_WriteOnly ()
    {
      var property = new MutablePropertyInfo (_declaringType, _name, _attributes, getMethod: null, setMethod: _setMethod);

      Assert.That (property.MutableGetMethod, Is.Null);
      var actualIndexParameters = property.GetIndexParameters();
      Assert.That (actualIndexParameters, Has.Length.EqualTo (2));
      CheckParameter (actualIndexParameters[0], property, 0, _indexParameters[0].Name, _indexParameters[0].Type, _indexParameters[0].Attributes);
      CheckParameter (actualIndexParameters[1], property, 1, _indexParameters[1].Name, _indexParameters[1].Type, _indexParameters[1].Attributes);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _property.AddCustomAttribute (declaration);

      Assert.That (_property.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));
      Assert.That (_property.GetCustomAttributeData ().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
    }

    [Test]
    public void ToDebugString ()
    {
      // Note: ToDebugString is defined in CustomFieldInfo base class.
      Assertion.IsNotNull (_property.DeclaringType);
      var declaringTypeName = _property.DeclaringType.Name;
      var propertyTypeName = _property.PropertyType.Name;
      var propertyName = _property.Name;
      var expected = "MutableProperty = \"" + propertyTypeName + " " + propertyName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (_property.ToDebugString (), Is.EqualTo (expected));
    }

    private static void CheckParameter (
        ParameterInfo parameter,
        MemberInfo expectedMember,
        int expectedPosition,
        string expectedName,
        Type expectedType,
        ParameterAttributes expectedAttributes)
    {
      Assert.That (parameter, Is.TypeOf<PropertyParameterInfoWrapper>());
      Assert.That (parameter.Member, Is.SameAs (expectedMember));
      Assert.That (parameter.Position, Is.EqualTo (expectedPosition));
      Assert.That (parameter.Name, Is.EqualTo (expectedName));
      Assert.That (parameter.ParameterType, Is.SameAs (expectedType));
      Assert.That (parameter.Attributes, Is.EqualTo (expectedAttributes));
    }
  }
}