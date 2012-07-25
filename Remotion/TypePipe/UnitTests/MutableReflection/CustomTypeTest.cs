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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class CustomTypeTest
  {
    private IMemberSelector _memberSelector;
    private Type _baseType;
    private string _name;
    private string _namespace;
    private string _fullName;

    private CustomType _customTypePartialMock;

    [SetUp]
    public void SetUp ()
    {
      _memberSelector = MockRepository.GenerateStrictMock<IMemberSelector>();
      _baseType = ReflectionObjectMother.GetSomeType();
      _name = "type name";
      _namespace = "namespace";
      _fullName = "full type name";

      _customTypePartialMock = MockRepository.GeneratePartialMock<CustomType>(_memberSelector, _baseType, _name, _namespace, _fullName);
    }

    [Test]
    public void Initialization_NullBaseType ()
    {
      MockRepository.GeneratePartialMock<CustomType> (_memberSelector, null, _name, _namespace, _fullName);
    }

    [Test]
    public void Assembly ()
    {
      Assert.That (_customTypePartialMock.Assembly, Is.Null);
    }

    [Test]
    public void Module ()
    {
      Assert.That (_customTypePartialMock.Module, Is.Null);
    }

    [Test]
    public void BaseType ()
    {
      Assert.That (_baseType, Is.Not.Null);

      Assert.That (_customTypePartialMock.BaseType, Is.SameAs (_baseType));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_name, Is.Not.Null.And.Not.Empty);

      Assert.That (_customTypePartialMock.Name, Is.EqualTo (_name));
    }

    [Test]
    public void Namespace ()
    {
      Assert.That (_namespace, Is.Not.Null.And.Not.Empty);

      Assert.That (_customTypePartialMock.Namespace, Is.EqualTo (_namespace));
    }

    [Test]
    public void FullName ()
    {
      Assert.That (_fullName, Is.Not.Null.And.Not.Empty);

      Assert.That (_customTypePartialMock.FullName, Is.EqualTo (_fullName));
    }

    [Test]
    public void GetElementType ()
    {
      Assert.That (_customTypePartialMock.GetElementType (), Is.Null);
    }

    [Test]
    public void HasElementTypeImpl ()
    {
      Assert.That (_customTypePartialMock.HasElementType, Is.False);
    }

    [Test]
    public void IsByRefImpl ()
    {
      Assert.That (_customTypePartialMock.IsByRef, Is.False);
    }

    [Test]
    public void IsArrayImpl ()
    {
      Assert.That (_customTypePartialMock.IsArray, Is.False);
    }

    [Test]
    public void IsPointerImpl ()
    {
      Assert.That (_customTypePartialMock.IsPointer, Is.False);
    }

    [Test]
    public void IsPrimitiveImpl ()
    {
      Assert.That (_customTypePartialMock.IsPrimitive, Is.False);
    }

    [Test]
    public void IsCOMObjectImpl ()
    {
      Assert.That (_customTypePartialMock.IsCOMObject, Is.False);
    }

    [Test]
    public void UnsupportedMembers ()
    {
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.MetadataToken, "Property", "MetadataToken");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.GUID, "Property", "GUID");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.AssemblyQualifiedName, "Property", "AssemblyQualifiedName");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.StructLayoutAttribute, "Property", "StructLayoutAttribute");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.GenericParameterAttributes, "Property", "GenericParameterAttributes");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.GenericParameterPosition, "Property", "GenericParameterPosition");
      CheckThrowsNotSupported (() => Dev.Null = _customTypePartialMock.TypeHandle, "Property", "TypeHandle");

      CheckThrowsNotSupported (() => _customTypePartialMock.GetDefaultMembers (), "Method", "GetDefaultMembers");
      CheckThrowsNotSupported (() => _customTypePartialMock.GetInterfaceMap (null), "Method", "GetInterfaceMap");
      CheckThrowsNotSupported (() => _customTypePartialMock.InvokeMember (null, 0, null, null, null), "Method", "InvokeMember");
      CheckThrowsNotSupported (() => _customTypePartialMock.MakePointerType (), "Method", "MakePointerType");
      CheckThrowsNotSupported (() => _customTypePartialMock.MakeByRefType (), "Method", "MakeByRefType");
      CheckThrowsNotSupported (() => _customTypePartialMock.MakeArrayType (), "Method", "MakeArrayType");
      CheckThrowsNotSupported (() => _customTypePartialMock.MakeArrayType (7), "Method", "MakeArrayType");
      CheckThrowsNotSupported (() => _customTypePartialMock.GetArrayRank (), "Method", "GetArrayRank");
      CheckThrowsNotSupported (() => _customTypePartialMock.GetGenericParameterConstraints (), "Method", "GetGenericParameterConstraints");
      CheckThrowsNotSupported (() => _customTypePartialMock.MakeGenericType (), "Method", "MakeGenericType");
      CheckThrowsNotSupported (() => _customTypePartialMock.GetGenericArguments (), "Method", "GetGenericArguments");
      CheckThrowsNotSupported (() => _customTypePartialMock.GetGenericTypeDefinition (), "Method", "GetGenericTypeDefinition");
    }

    private void CheckThrowsNotSupported (TestDelegate memberInvocation, string memberType, string memberName)
    {
      var message = string.Format ("{0} {1} is not supported.", memberType, memberName);
      Assert.That (memberInvocation, Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (message));
    }
  }
}