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
    private CustomType _customTypePartialMock;

    [SetUp]
    public void SetUp ()
    {
      _customTypePartialMock = MockRepository.GeneratePartialMock<CustomType>();
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