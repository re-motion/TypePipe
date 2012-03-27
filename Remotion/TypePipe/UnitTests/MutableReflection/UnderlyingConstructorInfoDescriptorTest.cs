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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingConstructorInfoDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var attributes = MethodAttributes.Abstract;
      var parameterDeclarations = new ParameterDeclaration[0];

      var ctorInfoStrategy = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarations);

      Assert.That (ctorInfoStrategy.UnderlyingSystemConstructorInfo, Is.Null);
      Assert.That (ctorInfoStrategy.Attributes, Is.EqualTo (attributes));
      Assert.That (ctorInfoStrategy.ParameterDeclarations, Is.SameAs (parameterDeclarations));
    }

    [Test]
    public void Create_ForExisting ()
    {
      var originalCtor = ReflectionObjectMother.GetConstructor (() => new ExampleType ("string", 7));
      var ctorInfoDescriptor = UnderlyingConstructorInfoDescriptor.Create (originalCtor);

      Assert.That (ctorInfoDescriptor.UnderlyingSystemConstructorInfo, Is.SameAs (originalCtor));
      Assert.That (ctorInfoDescriptor.Attributes, Is.EqualTo (originalCtor.Attributes));

      var result = ctorInfoDescriptor.ParameterDeclarations;

      var expectedParamterDecls =
          new[]
          {
              new { Type = typeof (string), Name = "s", Attributes = ParameterAttributes.None },
              new { Type = typeof (int), Name = "i", Attributes = ParameterAttributes.None }
          };
      var actualParameterDecls = result.Select (pd => new { pd.Type, pd.Name, pd.Attributes }).ToArray ();
      Assert.That (actualParameterDecls, Is.EqualTo (expectedParamterDecls));
    }

    private class ExampleType
    {
      public ExampleType (string s, int i) { }
    }
  }
}