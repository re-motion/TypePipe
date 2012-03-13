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
    public void AddField ()
    {
      Assert.That (GetAllFieldNames (typeof (OriginalType)), Is.EquivalentTo (new[] { "OriginalField" }));

      var type = AssembleType<OriginalType> (
          mutableType =>
          {
            mutableType.AddField ("_privateInstanceField", typeof (string), FieldAttributes.Private);
            mutableType.AddField ("PublicStaticField", typeof (int), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);
          });

      Assert.That (GetAllFieldNames (type), Is.EquivalentTo (new[] { "OriginalField", "_privateInstanceField", "PublicStaticField" }));

      var field1 = type.GetField ("_privateInstanceField", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.That (field1, Is.Not.Null);
      Assert.That (field1.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (field1.Attributes, Is.EqualTo (FieldAttributes.Private));

      var field2 = type.GetField ("PublicStaticField");
      Assert.That (field2, Is.Not.Null);
      Assert.That (field2.FieldType, Is.EqualTo (typeof (int)));
      Assert.That (field2.Attributes, Is.EqualTo (FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly));
    }

    private string[] GetAllFieldNames (Type type)
    {
      return type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
          .Select (field => field.Name)
          .ToArray(); // better error message
    }

    public class OriginalType
    {
      // protected so that Reflection on the subclass proxy will return the field
      protected object OriginalField;
    }
  }
}