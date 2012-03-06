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

namespace Remotion.TypePipe.UnitTests.FutureReflection
{
  [TestFixture]
  public class FutureFieldInfoTest
  {
    [Test]
    public void FutureFieldInfo_IsAFieldInfo ()
    {
      Assert.That (New.FutureFieldInfo(), Is.InstanceOf<FieldInfo>());
    }

    [Test]
    public void DeclaringType ()
    {
      var declaringType = New.FutureType();
      var futureFieldInfo = New.FutureFieldInfo (declaringType: declaringType);
      Assert.That (futureFieldInfo.DeclaringType, Is.SameAs (declaringType));
    }

    [Test]
    public void Attributes ()
    {
      var futureFieldInfo = New.FutureFieldInfo (fieldAttributes: FieldAttributes.InitOnly);
      Assert.That (futureFieldInfo.Attributes, Is.EqualTo (FieldAttributes.InitOnly));
    }

    [Test]
    public void FieldType ()
    {
      var fieldType = typeof (string);
      var futureFieldInfo = New.FutureFieldInfo (fieldType: fieldType);
      Assert.That (futureFieldInfo.FieldType, Is.SameAs (fieldType));
    }
  }
}