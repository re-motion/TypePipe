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
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.CodeGeneration;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class TypeAssemblyContextExtensionsTest
  {
    private TypeAssemblyContextBase _context;

    [SetUp]
    public void SetUp ()
    {
      _context = TypeAssemblyContextBaseObjectMother.Create();
    }

    [Test]
    public void CreateClass ()
    {
      var name = "MyName";
      var @namespace = "MyNamespace";
      var baseType = ReflectionObjectMother.GetSomeSubclassableType ();

      var result = _context.CreateClass (name, @namespace, baseType);

      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { result }));
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Namespace, Is.EqualTo (@namespace));
      Assert.That (result.BaseType, Is.SameAs (baseType));
      Assert.That (result.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.Class));
    }

    [Test]
    public void CreateInterface ()
    {
      var result = _context.CreateInterface ("IAbc", "MyNs");

      Assert.That (_context.AdditionalTypes, Is.EqualTo (new[] { result }));
      Assert.That (result.IsInterface, Is.True);
      Assert.That (result.BaseType, Is.Null);
      Assert.That (result.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract));
      Assert.That (result.Name, Is.EqualTo ("IAbc"));
      Assert.That (result.Namespace, Is.EqualTo ("MyNs"));
    }
  }
}