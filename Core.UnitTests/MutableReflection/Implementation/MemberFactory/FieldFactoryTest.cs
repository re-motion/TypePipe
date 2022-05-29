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
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class FieldFactoryTest
  {
    private FieldFactory _factory;

    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _factory = new FieldFactory();

      _mutableType = MutableTypeObjectMother.Create();
    }

    [Test]
    public void CreateField ()
    {
      var field = _factory.CreateField (_mutableType, "_newField", typeof (string), FieldAttributes.FamANDAssem);

      Assert.That (field.DeclaringType, Is.SameAs (_mutableType));
      Assert.That (field.Name, Is.EqualTo ("_newField"));
      Assert.That (field.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (field.Attributes, Is.EqualTo (FieldAttributes.FamANDAssem));
    }

    [Test]
    public void CreateField_ThrowsForInvalidFieldAttributes ()
    {
      var message = "The following FieldAttributes are not supported for fields: " +
                    "Literal, HasFieldMarshal, HasDefault, HasFieldRVA.";
      var paramName = "attributes";
      Assert.That (() => CreateField (_mutableType, FieldAttributes.Literal), Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, paramName));
      Assert.That (() => CreateField (_mutableType, FieldAttributes.HasFieldMarshal), Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, paramName));
      Assert.That (() => CreateField (_mutableType, FieldAttributes.HasDefault), Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, paramName));
      Assert.That (() => CreateField (_mutableType, FieldAttributes.HasFieldRVA), Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (message, paramName));
    }

    [Test]
    public void CreateField_VoidType ()
    {
      Assert.That (
          () => _factory.CreateField (_mutableType, "NotImportant", typeof (void), FieldAttributes.ReservedMask),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Field cannot be of type void.", "type"));
    }

    [Test]
    public void CreateField_ThrowsIfAlreadyExist ()
    {
      var field = _mutableType.AddField ("Field", FieldAttributes.Private, typeof (int));

      Assert.That (
          () => _factory.CreateField (_mutableType, "OtherName", field.FieldType, 0),
          Throws.Nothing);

      Assert.That (
          () => _factory.CreateField (_mutableType, field.Name, typeof (string), 0),
          Throws.Nothing);

      Assert.That (
          () => _factory.CreateField (_mutableType, field.Name, field.FieldType, 0),
          Throws.InvalidOperationException.With.Message.EqualTo ("Field with equal name and signature already exists."));
    }

    private MutableFieldInfo CreateField (MutableType mutableType, FieldAttributes attributes)
    {
      return _factory.CreateField (mutableType, "dummy", typeof (int), attributes);
    }
  }
}