// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using NUnit.Framework;
using Remotion.Reflection;
using Rhino.Mocks;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ReflectionBasedMemberInfoNameResolverTest
  {
    private ReflectionBasedMemberInformationNameResolver _resolver;
    private IPropertyInformation _propertyInformationStub;
    private ITypeInformation _typeInformationStub;

    [SetUp]
    public void SetUp ()
    {
      _resolver = new ReflectionBasedMemberInformationNameResolver();
      _propertyInformationStub = MockRepository.GenerateStub<IPropertyInformation>();
      _typeInformationStub = MockRepository.GenerateStub<ITypeInformation>();
    }

    [Test]
    public void GetPropertyName ()
    {
      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.Class");

      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property");
      _propertyInformationStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (_typeInformationStub);

      Assert.That (_resolver.GetPropertyName (_propertyInformationStub), Is.EqualTo ("Namespace.Class.Property"));
    }

    [Test]
    public void GetTypeName ()
    {
      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.Class");

      Assert.That (_resolver.GetTypeName (_typeInformationStub), Is.EqualTo ("Namespace.Class"));
    }

    [Test]
    public void GetPropertyName_Twice_ReturnsSameResultFromCache ()
    {
      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.Class");
      _propertyInformationStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (_typeInformationStub);

      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property1").Repeat.Once();
      string name1 = _resolver.GetPropertyName (_propertyInformationStub);

      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property2");
      Assert.That (_propertyInformationStub.Name, Is.EqualTo ("Property2"));
      string name2 = _resolver.GetPropertyName (_propertyInformationStub);

      Assert.That (name1, Is.SameAs (name2));
      Assert.That (name1, Is.EqualTo ("Namespace.Class.Property1"));
    }

    [Test]
    public void GetTypeName_Twice_ReturnsSameResultFromCache ()
    {
      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.Class");
      string name1 = _resolver.GetTypeName (_typeInformationStub);

      _typeInformationStub.Stub (stub => stub.FullName).Return ("IgnoredNewTypeName");
      string name2 = _resolver.GetTypeName (_typeInformationStub);

      Assert.That (name1, Is.SameAs (name2));
      Assert.That (name1, Is.EqualTo ("Namespace.Class"));
    }

    [Test]
    public void GetPropertyAndTypeName_ForOverriddenProperty ()
    {
      var derivedTypeStub = MockRepository.GenerateStub<ITypeInformation>();
      derivedTypeStub.Stub (stub => stub.FullName).Return ("Namespace.Derived");
      _propertyInformationStub.Stub (stub => stub.DeclaringType).Return (derivedTypeStub);

      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.Class");
      _propertyInformationStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (_typeInformationStub);
      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property");
      Assert.That (_resolver.GetPropertyName (_propertyInformationStub), Is.EqualTo ("Namespace.Class.Property"));
    }

    [Test]
    public void GetPropertyAndTypeName_ForPropertyInClosedGenericType ()
    {
      var genericTypeDefinitionStub = MockRepository.GenerateStub<ITypeInformation>();
      genericTypeDefinitionStub.Stub (stub => stub.FullName).Return ("Namespace.OpenGeneric<>");

      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.ClosedGeneric");
      _typeInformationStub.Stub (stub => stub.IsGenericType).Return (true);
      _typeInformationStub.Stub (stub => stub.IsGenericTypeDefinition).Return (false);
      _typeInformationStub.Stub (stub => stub.GetGenericTypeDefinition()).Return (genericTypeDefinitionStub);

      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property");
      _propertyInformationStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (_typeInformationStub);
      Assert.That (_resolver.GetPropertyName (_propertyInformationStub), Is.EqualTo ("Namespace.OpenGeneric<>.Property"));
    }

    [Test]
    public void GetPropertyAndTypeName_ForPropertyInOpenGenericType ()
    {
      _typeInformationStub.Stub (stub => stub.FullName).Return ("Namespace.OpenGeneric<>");
      _typeInformationStub.Stub (stub => stub.IsGenericType).Return (true);
      _typeInformationStub.Stub (stub => stub.IsGenericTypeDefinition).Return (true);

      _propertyInformationStub.Stub (stub => stub.Name).Return ("Property");
      _propertyInformationStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (_typeInformationStub);
      Assert.That (_resolver.GetPropertyName (_propertyInformationStub), Is.EqualTo ("Namespace.OpenGeneric<>.Property"));
    }
  }
}