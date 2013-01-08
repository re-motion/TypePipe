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

using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  [Ignore ("TODO 4778")]
  public class MutableTypeInSignaturesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void Field ()
    {
      var type = AssembleType<DomainType> (mt => mt.AddField ("Field", mt, FieldAttributes.Public));

      var field = type.GetField ("Field");
      Assert.That (field.FieldType, Is.SameAs (type));
    }

    [Test]
    public void Constructor ()
    {
      var type = AssembleType<DomainType> (
          mt => mt.AddConstructor (MethodAttributes.Public, new[] { new ParameterDeclaration (mt, "p") }, ctx => Expression.Empty()));

      var constructor = type.GetConstructor (new[] { type });
      Assert.That (constructor, Is.Not.Null);
      Assert.That (constructor.GetParameters().Single().ParameterType, Is.SameAs (type));
    }

    [Test]
    public void Method ()
    {
      var type = AssembleType<DomainType> (
          mt => mt.AddMethod ("Method", MethodAttributes.Public, mt, new[] { new ParameterDeclaration (mt, "p") }, ctx => Expression.Default (mt)));

      var method = type.GetMethod ("Method");
      Assert.That (method.ReturnType, Is.SameAs (type));
      Assert.That (method.GetParameters().Single().ParameterType, Is.SameAs (type));
    }
  }

  public class DomainType { }
}