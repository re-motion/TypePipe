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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableFieldInfoTest
  {
    private MutableFieldInfo _fieldInfo;

    [SetUp]
    public void SetUp ()
    {
      _fieldInfo = MutableFieldInfoObjectMother.Create ();
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = ReflectionObjectMother.GetSomeType();
      var fieldType = ReflectionObjectMother.GetSomeType ();
      var name = "_fieldName";
      var attributes = FieldAttributes.InitOnly;

      var fieldInfo = new MutableFieldInfo (declaringType, fieldType, name, attributes);
      
      Assert.That (fieldInfo.DeclaringType, Is.SameAs (declaringType));
      Assert.That (fieldInfo.FieldType, Is.SameAs (fieldType));
      Assert.That (fieldInfo.Name, Is.EqualTo (name));
      Assert.That (fieldInfo.Attributes, Is.EqualTo (attributes));
      Assert.That (fieldInfo.AddedCustomAttributeDeclarations, Is.Empty);
    }

    [Test]
    public void AddCustomAttribute ()
    {
      var customAttribute = CustomAttributeDeclarationObjectMother.Create();

      _fieldInfo.AddCustomAttribute (customAttribute);

      Assert.That (_fieldInfo.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { customAttribute }));
    }

    // TODO More checks for AddCustomAttribute errors?

    [Ignore ("TODO 4672")]
    [Test]
    public void GetCustomAttribute ()
    {
    }
  }
}