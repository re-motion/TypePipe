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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class AddFieldTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    [Ignore("TODO: Type Pipe")]
    public void AddField ()
    {
      Assert.That (GetAllFieldNames (typeof (OriginalType)), Is.EquivalentTo (new[] { "OriginalField" }));

      var type = AssembleType<OriginalType> (mutableType => mutableType.AddField ("_newField", typeof (object), FieldAttributes.Private));

      Assert.That (GetAllFieldNames (type), Is.EqualTo (new[] { "OriginalField", "_newField" }));
      
      var newField = type.GetField ("_newField", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.That (newField, Is.Not.Null);
      Assert.That (newField.FieldType, Is.TypeOf<object>());
      Assert.That (newField.Attributes, Is.EqualTo (FieldAttributes.Private));
    }

    private string[] GetAllFieldNames (Type type)
    {
      return type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
          .Select (field => field.Name)
          .ToArray(); // better error message
    }

    public class OriginalType
    {
      // private fields cannot be accessed in sub class proxies
      protected string OriginalField;
    }
  }
}