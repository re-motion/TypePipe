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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddCustomAttributeTest : TypeAssemblerIntegrationTestBase
  {
    [Ignore("TODO 5280")]
    [Test]
    public void NewMembers ()
    {
      var type = AssembleType<DomainType> (
          mutableType =>
          {
            var field = mutableType.AddField ("Field", typeof (int), FieldAttributes.Public);
            var constructor = mutableType.AllMutableConstructors.Single();
            var parameters = new[] { new ParameterDeclaration (typeof (int), "p") };
            var method = mutableType.AddMethod ("Method", MethodAttributes.Public, typeof (int), parameters, ctx => Expression.Constant (7));
            var parameter = method.MutableParameters.Single ();
            var returnParameter = method.MutableReturnParameter;

            AddCustomAttributes (mutableType);
            AddCustomAttributes (field);
            AddCustomAttributes (constructor);
            AddCustomAttributes (method);
            AddCustomAttributes (parameter);
            AddCustomAttributes (returnParameter);
            // TODO 4791
            //AddCustomAttributes (property);
            //AddCustomAttributes (@event);
          });

      var methodInfo = type.GetMethod ("Method");
      CheckAddedCustomAttributes (type);
      CheckAddedCustomAttributes (type.GetField ("Field"));
      CheckAddedCustomAttributes (type.GetConstructor (Type.EmptyTypes));
      CheckAddedCustomAttributes (methodInfo);
      CheckAddedCustomAttributes (methodInfo.GetParameters().Single(), typeof (InAttribute));
      CheckAddedCustomAttributes (methodInfo.ReturnParameter);
      // TODO 4791
      //CheckAddedAttributes (type.GetProperty("Property"));
      //CheckAddedAttributes (type.GetEvent("Event"));
      // nested types
      // setter value parameter, Adder (+ parameter), Remover (+ parameter), generic type, Invoker?
    }

    private void AddCustomAttributes (IMutableInfo mutableInfo)
    {
      Assert.That (mutableInfo.CanAddCustomAttributes, Is.True);

      mutableInfo.AddCustomAttribute (CreateSingleAttribute());
      Assert.That (
          () => mutableInfo.AddCustomAttribute (CreateSingleAttribute ()),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Attribute of type 'SingleAttribute' (with AllowMultiple = false) is already present."));

      mutableInfo.AddCustomAttribute (CreateMultipleAttribute ("abc"));
      mutableInfo.AddCustomAttribute (CreateMultipleAttribute ("def"));
    }

    private void CheckAddedCustomAttributes (ICustomAttributeProvider attributeProvider, params Type[] additionalAttributeTypes)
    {
      var attributes = attributeProvider.GetCustomAttributes (false);

      var expectedAttributeTypes = new[] { typeof (SingleAttribute), typeof (MultipleAttribute), typeof (MultipleAttribute) }
          .Concat (additionalAttributeTypes);
      Assert.That (attributes.Select (a => a.GetType()), Is.EquivalentTo (expectedAttributeTypes));
      Assert.That (attributes.OfType<MultipleAttribute>().Select (a => a.String), Is.EquivalentTo (new[] { "abc", "def" }));
    }

    private CustomAttributeDeclaration CreateSingleAttribute ()
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new SingleAttribute());
      return new CustomAttributeDeclaration (constructor, new object[0]);
    }

    private CustomAttributeDeclaration CreateMultipleAttribute (string value)
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new MultipleAttribute());
      var field = NormalizingMemberInfoFromExpressionUtility.GetField ((MultipleAttribute obj) => obj.String);
      return new CustomAttributeDeclaration (constructor, new object[0], new NamedArgumentDeclaration (field, value));
    }

    public class DomainType { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = false)]
    public class SingleAttribute : Attribute { }

    [AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
    public class MultipleAttribute : Attribute
    {
      public string String;
    }
  }
}