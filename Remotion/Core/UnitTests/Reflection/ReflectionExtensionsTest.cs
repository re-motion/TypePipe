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
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class ReflectionExtensionsTest
  {
    private class TheType
    {
      public int TheProperty { get; set; }
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeEqualsOrignalDeclaringType_True ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation>();
      var typeInformationStub = MockRepository.GenerateStub<ITypeInformation>();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (typeInformationStub);
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (typeInformationStub);
      Assert.That (memberInfoStub.IsOriginalDeclaration(), Is.True);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeNotEqualToOrignalDeclaringType_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (MockRepository.GenerateStub<ITypeInformation>());
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (MockRepository.GenerateStub<ITypeInformation>());

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeIsNull_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (null);
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType()).Return (MockRepository.GenerateStub<ITypeInformation>());

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_OrignalDeclaringTypeIsNull_False ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (MockRepository.GenerateStub<ITypeInformation>());
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (null);

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.False);
    }

    [Test]
    public void IsOriginalDeclaration_DeclaringTypeIsNullAndOrignalDeclaringTypeIsNull_True ()
    {
      var memberInfoStub = MockRepository.GenerateStub<IMemberInformation> ();
      memberInfoStub.Stub (stub => stub.DeclaringType).Return (null);
      memberInfoStub.Stub (stub => stub.GetOriginalDeclaringType ()).Return (null);

      Assert.That (memberInfoStub.IsOriginalDeclaration (), Is.True);
    }

    [Test]
    public void AsRuntimeType_WithTypeAdapter_ReturnsRuntimeType ()
    {
      var expectedType = typeof (TheType);
      ITypeInformation typeInformation = TypeAdapter.Create (expectedType);

      Assert.That (typeInformation.AsRuntimeType(), Is.SameAs (expectedType));
    }

    [Test]
    public void AsRuntimeType_WithOtherITypeInformation_ReturnsNull ()
    {
      var typeInformation = MockRepository.GenerateStub<ITypeInformation>();

      Assert.That (typeInformation.AsRuntimeType(), Is.Null);
    }

    [Test]
    public void ConvertToRuntimeType_WithTypeAdapter_ReturnsRuntimeType ()
    {
      var expectedType = typeof (TheType);
      ITypeInformation typeInformation = TypeAdapter.Create (expectedType);

      Assert.That (typeInformation.ConvertToRuntimeType(), Is.SameAs (expectedType));
    }

    [Test]
    public void ConvertToRuntimeType_WithOtherITypeInformation_ThrowsInvalidOperationException ()
    {
      var typeInformation = MockRepository.GenerateStub<ITypeInformation>();
      typeInformation.Stub (_ => _.Name).Return ("TheName");

      Assert.That (
          () => typeInformation.ConvertToRuntimeType(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              string.Format (
                  "The type 'TheName' cannot be converted to a runtime type because no conversion is registered for '{0}'.",
                  typeInformation.GetType().Name)));
    }

    [Test]
    public void AsRuntimeProperty_WithPropertyAdapter_ReturnsRuntimeType ()
    {
      var expectedProperty = MemberInfoFromExpressionUtility.GetProperty ((TheType t) => t.TheProperty);
      IPropertyInformation propertyInformation = PropertyInfoAdapter.Create (expectedProperty);

      Assert.That (propertyInformation.AsRuntimePropertyInfo(), Is.SameAs (expectedProperty));
    }

    [Test]
    public void AsRuntimeProperty_WithOtherIPropertyInformation_ReturnsNull ()
    {
      var propertyInformation = MockRepository.GenerateStub<IPropertyInformation>();

      Assert.That (propertyInformation.AsRuntimePropertyInfo(), Is.Null);
    }

    [Test]
    public void ConvertToRuntimeProperty_WithPropertyAdapter_ReturnsRuntimeType ()
    {
      var expectedProperty = MemberInfoFromExpressionUtility.GetProperty ((TheType t) => t.TheProperty);
      IPropertyInformation propertyInformation = PropertyInfoAdapter.Create (expectedProperty);

      Assert.That (propertyInformation.ConvertToRuntimePropertyInfo(), Is.SameAs (expectedProperty));
    }

    [Test]
    public void ConvertToRuntimeProperty_WithOtherIPropertyInformation_ThrowsInvalidOperationException ()
    {
      var propertyInformation = MockRepository.GenerateStub<IPropertyInformation>();
      propertyInformation.Stub (_ => _.Name).Return ("TheName");

      Assert.That (
          () => propertyInformation.ConvertToRuntimePropertyInfo(),
          Throws.InvalidOperationException.And.Message.EqualTo (
              string.Format (
                  "The property 'TheName' cannot be converted to a runtime property because no conversion is registered for '{0}'.",
                  propertyInformation.GetType().Name)));
    }
  }
}