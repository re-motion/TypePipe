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
  public class AddNestedTypesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void AddSimpleType ()
    {
      var type = AssembleType<DomainType> (p => p.AddNestedType ("NestedType", TypeAttributes.NestedPublic, typeof (BaseType)));
    
      var addedNestedType = type.GetNestedTypes().Single();
      Assert.That (addedNestedType.Name, Is.EqualTo ("NestedType"));
      Assert.That (addedNestedType.FullName, Is.EqualTo ("Remotion.TypePipe.IntegrationTests.TypeAssembly.DomainType_Proxy_1+NestedType"));
      Assert.That (addedNestedType.Attributes, Is.EqualTo (TypeAttributes.NestedPublic));
      Assert.That (addedNestedType.BaseType, Is.SameAs (typeof (BaseType)));
      Assert.That (addedNestedType.DeclaringType, Is.SameAs (type));
    }

    [Test]
    public void AddInterface ()
    {
      var attributes = TypeAttributes.NestedPublic | TypeAttributes.Interface | TypeAttributes.Abstract;
      var type = AssembleType<DomainType> (p => p.AddNestedType ("NestedType", attributes, null));

      var addedNestedType = type.GetNestedTypes ().Single ();
      Assert.That (addedNestedType.Name, Is.EqualTo ("NestedType"));
      Assert.That (addedNestedType.FullName, Is.EqualTo ("Remotion.TypePipe.IntegrationTests.TypeAssembly.DomainType_Proxy_1+NestedType"));
      Assert.That (addedNestedType.IsInterface, Is.True);
      Assert.That (addedNestedType.BaseType, Is.Null);
      Assert.That (addedNestedType.DeclaringType, Is.SameAs (type));
    }

    [Test]
    public void NestedTypeUsedInSignature ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var nestedType = proxyType.AddNestedType ("NestedType", TypeAttributes.NestedPublic, typeof (object));
            var field = proxyType.AddField ("field", FieldAttributes.Public | FieldAttributes.Static, nestedType);
            nestedType.AddMethod (
                "Method",
                MethodAttributes.Public | MethodAttributes.Static,
                GenericParameterDeclaration.None,
                ctx => nestedType,
                ctx => ParameterDeclaration.None,
                ctx => Expression.Field (null, field));
          });

      var addedNestedType = type.GetNestedTypes().Single();
      var addedField = type.GetField ("field");
      var instance = Activator.CreateInstance (addedNestedType);
      addedField.SetValue (null, instance);
      var method = addedNestedType.GetMethod ("Method");

      var result = method.Invoke (null, new object[0]);

      Assert.That (result, Is.Not.Null.And.SameAs (instance));
    }

    [Ignore ("TODO 5550")]
    [Test]
    public void NestedTypeUsedAsBaseType ()
    {
      // Using nested types as type dependencies (i.e., base type, interfaces) requires us to handle them like other top level types and not like
      // members. This means that they need to be included in the MutableType ordering before code is generated.

      var type = AssembleType<DomainType> (
          assemblyContext =>
          {
            var nestedType = assemblyContext.ProxyType.AddNestedType ("NestedBaseType", TypeAttributes.NestedPublic, typeof (object));
            nestedType.AddConstructor (MethodAttributes.Public, ParameterDeclaration.None, ctx => Expression.Empty());
            assemblyContext.CreateType ("TypeWithNestedBaseType", "MyNs", TypeAttributes.Public, nestedType);
          });

      var nestedBaseType = type.GetNestedTypes().Single();
      var typeWithNestedBaseType = type.Assembly.GetType ("MyNs.TypeWithNestedBaseType", throwOnError: true);

      Assert.That (typeWithNestedBaseType.BaseType, Is.SameAs (nestedBaseType));
    }

    public class DomainType {}
    public class BaseType {}
  }
}