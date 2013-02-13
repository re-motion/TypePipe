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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class CustomPropertyInfoTest
  {
    private CustomType _declaringType;
    private Type _type;
    private CustomParameterInfo _parameter;
    private CustomParameterInfo _indexParameter;
    private CustomMethodInfo _getMethod;
    private CustomMethodInfo _setMethod;

    private CustomPropertyInfo _readOnlyProperty;
    private CustomPropertyInfo _writeOnlyProperty;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create ();
      _type = ReflectionObjectMother.GetSomeType ();
      _parameter = CustomParameterInfoObjectMother.Create (type: _type);
      var indexParameterType = ReflectionObjectMother.GetSomeOtherType();
      _indexParameter = CustomParameterInfoObjectMother.Create (type: indexParameterType);
      _getMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Public, returnParameter: _parameter);
      _setMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Public, parameters: new[] { _indexParameter, _parameter });

      _readOnlyProperty = CustomPropertyInfoObjectMother.Create (getMethod: _getMethod);
      _writeOnlyProperty = CustomPropertyInfoObjectMother.Create (setMethod: _setMethod);
    }

    [Test]
    public void Initialization ()
    {
      var property = new TestableCustomPropertyInfo (_declaringType, "Property", (PropertyAttributes) 7, _getMethod, _setMethod, new[] { _parameter });

      Assert.That (property.Attributes, Is.EqualTo ((PropertyAttributes) 7));
      Assert.That (property.DeclaringType, Is.EqualTo (_declaringType));
      Assert.That (property.Name, Is.EqualTo ("Property"));
      Assert.That (property.GetGetMethod(), Is.SameAs (_getMethod));
      Assert.That (property.GetSetMethod(), Is.SameAs (_setMethod));
    }

    [Test]
    public void Initialization_Type ()
    {
      Assert.That (_readOnlyProperty.PropertyType, Is.EqualTo (_type));
      Assert.That (_writeOnlyProperty.PropertyType, Is.EqualTo (_type));
    }

    [Test]
    public void CanRead ()
    {
      Assert.That (_readOnlyProperty.CanRead, Is.True);
      Assert.That (_writeOnlyProperty.CanRead, Is.False);
    }

    [Test]
    public void CanWrite ()
    {
      Assert.That (_readOnlyProperty.CanWrite, Is.False);
      Assert.That (_writeOnlyProperty.CanWrite, Is.True);
    }

    [Test]
    public void GetGetMethod ()
    {
      var nonPublicMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Private, returnParameter: _parameter);
      var property1 = CustomPropertyInfoObjectMother.Create (getMethod: nonPublicMethod);
      var property2 = CustomPropertyInfoObjectMother.Create (getMethod: _getMethod);
      var property3 = CustomPropertyInfoObjectMother.Create (getMethod: null, setMethod: _setMethod);

      Assert.That (property1.GetGetMethod (true), Is.SameAs (nonPublicMethod));
      Assert.That (property1.GetGetMethod (false), Is.Null);
      Assert.That (property2.GetGetMethod (true), Is.SameAs (_getMethod));
      Assert.That (property2.GetGetMethod (false), Is.SameAs (_getMethod));
      Assert.That (property3.GetGetMethod (true), Is.Null);
      Assert.That (property3.GetGetMethod (false), Is.Null);
    }

    [Test]
    public void GetSetMethod ()
    {
      var nonPublicMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Private, parameters: new[] { _parameter });
      var property1 = CustomPropertyInfoObjectMother.Create (setMethod: nonPublicMethod);
      var property2 = CustomPropertyInfoObjectMother.Create (setMethod: _setMethod);
      var property3 = CustomPropertyInfoObjectMother.Create (setMethod: null);

      Assert.That (property1.GetSetMethod (true), Is.SameAs (nonPublicMethod));
      Assert.That (property1.GetSetMethod (false), Is.Null);
      Assert.That (property2.GetSetMethod (true), Is.SameAs (_setMethod));
      Assert.That (property2.GetSetMethod (false), Is.SameAs (_setMethod));
      Assert.That (property3.GetSetMethod (true), Is.Null);
      Assert.That (property3.GetSetMethod (false), Is.Null);
    }

    [Test]
    public void GetAccessors ()
    {
      var nonPublicSetMethod = CustomMethodInfoObjectMother.Create (attributes: MethodAttributes.Private, parameters: new[] { _parameter });
      var property = CustomPropertyInfoObjectMother.Create (getMethod: _getMethod, setMethod: nonPublicSetMethod);

      Assert.That (property.GetAccessors (true), Is.EquivalentTo (new[] { _getMethod, nonPublicSetMethod }));
      Assert.That (property.GetAccessors (false), Is.EquivalentTo (new[] { _getMethod }));
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      var property = CustomPropertyInfoObjectMother.Create (customAttributes: new[] { customAttribute });

      Assert.That (property.GetCustomAttributes (false).Select (a => a.GetType()), Is.EqualTo (new[] { typeof (ObsoleteAttribute) }));
      Assert.That (property.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (property.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (property.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var returnParameter = CustomParameterInfoObjectMother.Create (type: type);
      var method = CustomMethodInfoObjectMother.Create (returnParameter: returnParameter);
      var name = "MyProperty";
      var property = CustomPropertyInfoObjectMother.Create (name: name, getMethod: method);

      Assert.That (property.ToString(), Is.EqualTo (type.Name + " MyProperty"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = CustomTypeObjectMother.Create ();
      var type = ReflectionObjectMother.GetSomeType ();
      var returnParameter = CustomParameterInfoObjectMother.Create (type: type);
      var method = CustomMethodInfoObjectMother.Create (returnParameter: returnParameter);
      var name = "MyProperty";
      var property = CustomPropertyInfoObjectMother.Create (declaringType, name, getMethod: method);

      // Note: ToDebugString is defined in CustomFieldInfo base class.
      Assertion.IsNotNull (property.DeclaringType);
      var declaringTypeName = property.DeclaringType.Name;
      var propertyTypeName = property.PropertyType.Name;
      var propertyName = property.Name;
      var expected = "TestableCustomProperty = \"" + propertyTypeName + " " + propertyName + "\", DeclaringType = \"" + declaringTypeName + "\"";

      Assert.That (property.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      var property = CustomPropertyInfoObjectMother.Create();

      UnsupportedMemberTestHelper.CheckProperty (() => property.ReflectedType, "ReflectedType");

      UnsupportedMemberTestHelper.CheckMethod (() => property.SetValue (null, null, null), "SetValue");
      UnsupportedMemberTestHelper.CheckMethod (() => property.GetValue (null, null), "GetValue");
    }

    public int this[string index]
    {
      get { return 0; }
    }
  }
}