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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddFieldTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void PrivateInstance ()
    {
      Assert.That (GetAllFieldNames (typeof (OriginalType)), Is.EquivalentTo (new[] { "OriginalField" }));

      var type = AssembleType<OriginalType> (proxyType => proxyType.AddField ("_privateInstanceField", FieldAttributes.Private, typeof (string)));

      Assert.That (GetAllFieldNames (type), Is.EquivalentTo (new[] { "OriginalField", "_privateInstanceField" }));

      var fieldInfo = type.GetField ("_privateInstanceField", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.That (fieldInfo, Is.Not.Null);
      Assert.That (fieldInfo.FieldType, Is.EqualTo (typeof (string)));
      Assert.That (fieldInfo.Attributes, Is.EqualTo (FieldAttributes.Private));
    }

    [Test]
    public void PublicStatic_FieldAttributes ()
    {
      Assert.That (GetAllFieldNames (typeof (OriginalType)), Is.EquivalentTo (new[] { "OriginalField" }));

      var fieldAttributes = FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.NotSerialized;
      var type = AssembleType<OriginalType> (proxyType => proxyType.AddField ("PublicStaticField", fieldAttributes, typeof (int)));

      Assert.That (GetAllFieldNames (type), Is.EquivalentTo (new[] { "OriginalField", "PublicStaticField" }));

      var fieldInfo = type.GetField ("PublicStaticField");
      Assert.That (fieldInfo, Is.Not.Null);
      Assert.That (fieldInfo.FieldType, Is.EqualTo (typeof (int)));
      Assert.That (fieldInfo.Attributes, Is.EqualTo (fieldAttributes));
      Assert.That (fieldInfo.IsNotSerialized, Is.True);
    }

    [Test]
    public void ShadowingField ()
    {
      var nonPublicInstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;
      var existingField = typeof (OriginalType).GetField ("OriginalField", nonPublicInstanceFlags);
      Assert.That (existingField, Is.Not.Null);

      var type = AssembleType<DerivedType> (proxyType => 
      { 
        var addedField = proxyType.AddField (existingField.Name, FieldAttributes.Family, existingField.FieldType);

        Assert.That (
            proxyType.GetFields (nonPublicInstanceFlags),
            Is.EquivalentTo (new[] { addedField, typeof (DerivedType).GetField ("OriginalField", nonPublicInstanceFlags) }));
      });

      Assert.That (GetAllFieldNames (type), Is.EquivalentTo (new[] { "OriginalField", "OriginalField" }));
    }

    [Test]
    public void WithCustomAttribute ()
    {
      var type = AssembleType<OriginalType> (
          proxyType =>
          {
            var mutableFieldInfo = proxyType.AddField ("_fieldWithCustomAttributes", FieldAttributes.Private, typeof (int));

            var attributeCtor = typeof (AddedAttribute).GetConstructor (new[] { typeof (string) });
            var namedProperty = typeof (AddedAttribute).GetProperty ("NamedPropertyArg");
            var namedField = typeof (AddedAttribute).GetField ("NamedFieldArg");
            var customAttributeDeclaration = new CustomAttributeDeclaration (
                attributeCtor,
                new object[] { "ctorArg" },
                new NamedArgumentDeclaration (namedProperty, 7),
                new NamedArgumentDeclaration (namedField, new[] { MyEnum.Other, MyEnum.Default }));
            mutableFieldInfo.AddCustomAttribute (customAttributeDeclaration);
          });

      var field = type.GetField ("_fieldWithCustomAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
      var customAttributes = field.GetCustomAttributes (false);
      Assert.That (customAttributes, Has.Length.EqualTo (1));

      var customAttribute = (AddedAttribute) customAttributes.Single();
      Assert.That(customAttribute.CtorArg, Is.EqualTo("ctorArg"));
      Assert.That (customAttribute.NamedPropertyArg, Is.EqualTo (7));
      Assert.That (customAttribute.NamedFieldArg, Is.EqualTo (new[] { MyEnum.Other, MyEnum.Default }));
    }

    [Test]
    public void MutableField_UsedByMethodBodies ()
    {
      var type = AssembleType<OriginalType> (
          proxyType =>
          {
            var fieldInfo = proxyType.AddField ("_privateInstanceField", FieldAttributes.Private, typeof (string));
            proxyType.AddConstructor (
                MethodAttributes.Public,
                new[] { new ParameterDeclaration (typeof (string), "arg") },
                ctx =>
                Expression.Block (ctx.CallThisConstructor (), Expression.Assign (Expression.Field (ctx.This, fieldInfo), ctx.Parameters[0]))
                );
          },
          proxyType =>
          proxyType.AddMethod (
              "MethodUsingField",
              MethodAttributes.Public,
              typeof (string),
              ParameterDeclaration.None,
              ctx => Expression.Field (ctx.This, proxyType.GetField ("_privateInstanceField", BindingFlags.Instance | BindingFlags.NonPublic))));

      var instance = Activator.CreateInstance (type, "test value");

      var method = type.GetMethod ("MethodUsingField");
      var result = method.Invoke (instance, null);

      Assert.That (result, Is.EqualTo ("test value"));
    }

    private string[] GetAllFieldNames (Type type)
    {
      return type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
          .Select (field => field.Name)
          .Where(n => n != "__typeID") // Exclude '__typeID' field.
          .ToArray (); // Better error message.
    }

    public class OriginalType
    {
      // protected so that Reflection on the subclass proxy will return the field
      protected object OriginalField;
    }

    public class DerivedType : OriginalType
    {
    }

    public class AddedAttribute : Attribute
    {
      public MyEnum[] NamedFieldArg;

      private readonly string _ctorArg;

      public AddedAttribute (string ctorArg)
      {
        _ctorArg = ctorArg;
      }

      public string CtorArg
      {
        get { return _ctorArg; }
      }

      public int NamedPropertyArg { get; set; }
    }

    public enum MyEnum
    {
      Default,
      Other
    }
  }
}