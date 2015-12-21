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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddPropertyTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void ReadWrite_WithNewField ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var field = proxyType.AddField ("_field", FieldAttributes.Private, typeof (string));
            proxyType.AddProperty (
                "Property",
                typeof (string),
                ParameterDeclaration.None,
                MethodAttributes.Public, 
                getBodyProvider: ctx => Expression.Field (ctx.This, field),
                setBodyProvider: ctx => Expression.Assign (Expression.Field (ctx.This, field), ctx.Parameters[0]));
          });

      var property = type.GetProperty ("Property");
      var backingField = type.GetField ("_field", BindingFlags.NonPublic | BindingFlags.Instance);
      Assertion.IsNotNull (backingField);
      var instance = Activator.CreateInstance (type);

      Assert.That (backingField.GetValue (instance), Is.Null);
      Assert.That (property.GetValue (instance, null), Is.Null);
      property.SetValue (instance, "Test", null);
      Assert.That (backingField.GetValue (instance), Is.EqualTo ("Test"));
      Assert.That (property.GetValue (instance, null), Is.EqualTo ("Test"));
    }

    [Test]
    public void AccessorAttributes ()
    {
      var accessorAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var prop = proxyType.AddProperty (
                "Property",
                typeof (int),
                ParameterDeclaration.None,
                accessorAttributes: accessorAttributes,
                getBodyProvider: ctx => Expression.Constant (7),
                setBodyProvider: ctx => Expression.Empty());

            Assert.That (prop.GetGetMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName));
            Assert.That (prop.GetSetMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName));
          });

      var property = type.GetProperty ("Property");
      Assert.That (property.GetGetMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName));
      Assert.That (property.GetSetMethod().Attributes, Is.EqualTo (accessorAttributes | MethodAttributes.SpecialName));
    }

    [Test]
    public void ReadOnly_WriteOnly ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.AddProperty (
                "ReadOnly", typeof (int), ParameterDeclaration.None, MethodAttributes.Public, ctx => Expression.Default (typeof (int)), null);
            proxyType.AddProperty ("WriteOnly", typeof (int), ParameterDeclaration.None, MethodAttributes.Public, null, ctx => Expression.Empty());
          });

      var readOnlyProperty = type.GetProperty ("ReadOnly");
      var writeOnlyProperty = type.GetProperty ("WriteOnly");

      Assert.That (readOnlyProperty.CanWrite, Is.False);
      Assert.That (readOnlyProperty.GetSetMethod (true), Is.Null);
      Assert.That (writeOnlyProperty.CanRead, Is.False);
      Assert.That (writeOnlyProperty.GetGetMethod (true), Is.Null);
    }

    [Test]
    public void IndexParameters ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType o) => o.PublicField);
            proxyType.AddProperty (
                "Property",
                typeof (string),
                new[] { new ParameterDeclaration (typeof (string), "index0"), new ParameterDeclaration (typeof (string), "index1") },
                MethodAttributes.Public,
                getBodyProvider: ctx =>
                {
                  Assert.That (ctx.ReturnType, Is.SameAs (typeof (string)));
                  Assert.That (ctx.Parameters.Count, Is.EqualTo (2));
                  return ExpressionHelper.StringConcat (ctx.Parameters[0], ctx.Parameters[1]);
                },
                setBodyProvider: ctx =>
                {
                  Assert.That (ctx.ReturnType, Is.SameAs (typeof (void)));
                  Assert.That (ctx.Parameters.Count, Is.EqualTo (3));
                  var value = ExpressionHelper.StringConcat (ExpressionHelper.StringConcat (ctx.Parameters[0], ctx.Parameters[1]), ctx.Parameters[2]);
                  return Expression.Assign (Expression.Field (ctx.This, field), value);
                });
          });

      var property = type.GetProperty ("Property");
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (property.GetValue (instance, new object[] { "a ", "b" }), Is.EqualTo ("a b"));
      property.SetValue (instance, "value", new object[] { "a ", "b " });
      Assert.That (instance.PublicField, Is.EqualTo ("a b value"));
    }

    [Test]
    public void OverrideExisting_AddCustomAttribute ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var existingProperty = NormalizingMemberInfoFromExpressionUtility.GetProperty ((DomainType obj) => obj.ExistingProperty);
            var getMethod = proxyType.GetOrAddOverride (existingProperty.GetGetMethod());
            var setMethod = proxyType.GetOrAddOverride (existingProperty.GetSetMethod());
            var property = proxyType.AddProperty (existingProperty.Name, PropertyAttributes.None, getMethod, setMethod);

            var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (""));
            var customAttributes = new CustomAttributeDeclaration (attributeCtor, new object[] { "derived" });
            property.AddCustomAttribute (customAttributes);
          });

      var newProperty = type.GetProperty ("ExistingProperty");
      var instance = (DomainType) Activator.CreateInstance (type);

      Assert.That (instance.ExistingProperty, Is.Null);
      Assert.That (newProperty.GetValue (instance, null), Is.Null);
      newProperty.SetValue (instance, "Test", null);
      Assert.That (instance.ExistingProperty, Is.EqualTo ("Test"));
      Assert.That (newProperty.GetValue (instance, null), Is.EqualTo ("Test"));

      var attributeArgs = Attribute.GetCustomAttributes (newProperty, inherit: true).Cast<AbcAttribute>().Select (a => a.Arg);
      Assert.That (attributeArgs, Is.EquivalentTo (new[] { "base", "derived" }));
    }

    [Test]
    public void ReadOnly_NonPublic_Static ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var attributes = MethodAttributes.Private | MethodAttributes.Static;
            var getMethod = proxyType.AddMethod (
                "StaticGetMethod", attributes, typeof (int), ParameterDeclaration.None, ctx => Expression.Constant (7));
            proxyType.AddProperty ("StaticProperty", PropertyAttributes.SpecialName, getMethod, null);
          });

      var nonExistingInstanceProperty = type.GetProperty ("StaticProperty", BindingFlags.NonPublic | BindingFlags.Instance);
      Assert.That (nonExistingInstanceProperty, Is.Null);

      var property = type.GetProperty ("StaticProperty", BindingFlags.NonPublic | BindingFlags.Static);
      CheckSignature (property, CallingConventions.Standard);

      Assert.That (property.Attributes, Is.EqualTo (PropertyAttributes.SpecialName));
      Assert.That (property.GetValue (null, null), Is.EqualTo (7));
    }

    [Test]
    public void WriteOnly_Public_Instance ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var field = NormalizingMemberInfoFromExpressionUtility.GetField ((DomainType o) => o.PublicField);
            var setMethod = proxyType.AddMethod (
                "InstanceSetMethod",
                MethodAttributes.Public,
                typeof (void),
                new[] { new ParameterDeclaration (typeof (string), "value") },
                ctx => Expression.Assign (Expression.Field (ctx.This, field), ctx.Parameters[0]));
            proxyType.AddProperty ("InstanceProperty", PropertyAttributes.None, getMethod: null, setMethod: setMethod);
          });

      var nonExistingStaticProperty = type.GetProperty ("InstanceProperty", BindingFlags.Public | BindingFlags.Static);
      Assert.That (nonExistingStaticProperty, Is.Null);

      var property = type.GetProperty ("InstanceProperty", BindingFlags.Public | BindingFlags.Instance);
      CheckSignature (property, CallingConventions.HasThis | CallingConventions.Standard);

      var instance = (DomainType) Activator.CreateInstance (type);
      property.SetValue (instance, "test", null);
      Assert.That (instance.PublicField, Is.EqualTo ("test"));
    }

    private static void CheckSignature (PropertyInfo property, CallingConventions expectedCallingConvention)
    {
      // Unfortunately there is no other way to observe that we indeed correctly generate an 'instance property', i.e., a property with a
      // signature that in turn has the CallingConventions.HasThis. This forces us to use the correct (more complex) overload of
      // TypeBuilder.DefineProperty during code generation (TypeBuilderAdapter.DefineProperty).

      var signature = PrivateInvoke.GetNonPublicProperty (property, "Signature");
      var callingConvention = (CallingConventions) PrivateInvoke.GetNonPublicProperty (signature, "CallingConvention");
      Assert.That (callingConvention, Is.EqualTo (expectedCallingConvention));
    }

    public class DomainType
    {
      [UsedImplicitly] public string PublicField;

      [Abc ("base")]
      public virtual string ExistingProperty { get; set; }
    }

    [AttributeUsage (AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class AbcAttribute : Attribute
    {
      public AbcAttribute (string arg) { Arg = arg; }
      public string Arg { get; set; }
    }
  }
}