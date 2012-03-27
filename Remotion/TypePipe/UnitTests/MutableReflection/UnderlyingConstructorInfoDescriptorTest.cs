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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Text;
using Remotion.TypePipe.Expressions;
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
      var arguments = new ArgumentTestHelper ("string", 7).Expressions;
      var parameterDeclarations = new ParameterDeclaration[0];

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarations, arguments);

      Assert.That (descriptor.UnderlyingSystemConstructorInfo, Is.Null);
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.ParameterDeclarations, Is.SameAs (parameterDeclarations));
      Assert.That (descriptor.Arguments, Is.SameAs (arguments));
      Assert.That (descriptor.Body, Is.TypeOf<OriginalBodyExpression> ());
      var originalBodyExpression = (OriginalBodyExpression) descriptor.Body;
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (arguments));
    }

    [Test]
    public void Create_ForExisting ()
    {
      var originalCtor = ReflectionObjectMother.GetConstructor (() => new ExampleType ("string", 7));
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalCtor);

      Assert.That (descriptor.UnderlyingSystemConstructorInfo, Is.SameAs (originalCtor));
      Assert.That (descriptor.Attributes, Is.EqualTo (originalCtor.Attributes));

      var expectedParamterDecls =
          new[]
          {
              new { Type = typeof (string), Name = "s", Attributes = ParameterAttributes.None },
              new { Type = typeof (int), Name = "i", Attributes = ParameterAttributes.None }
          };
      var actualParameterDecls = descriptor.ParameterDeclarations.Select (pd => new { pd.Type, pd.Name, pd.Attributes });
      Assert.That (actualParameterDecls, Is.EqualTo (expectedParamterDecls));

      var arguments = descriptor.Arguments;
      Assert.That (arguments, Has.All.InstanceOf<ParameterExpression>());
      var actualArgumentSignature = GetSignature (arguments.Cast<ParameterExpression>());
      Assert.That (actualArgumentSignature, Is.EqualTo ("System.String s, System.Int32 i"));

      Assert.That (descriptor.Body, Is.TypeOf<OriginalBodyExpression>());
      var originalBodyExpression = (OriginalBodyExpression) descriptor.Body;
      var actualBodyArgumentSignature = GetSignature (originalBodyExpression.Arguments.Cast<ParameterExpression>());
      Assert.That (actualBodyArgumentSignature, Is.EqualTo ("System.String s, System.Int32 i"));
    }

    private string GetSignature(IEnumerable<ParameterExpression> arguments)
    {
      return SeparatedStringBuilder.Build (", ", arguments, exp => exp.Type + " " + exp.Name);
    }

    private class ExampleType
    {
      public ExampleType (string s, int i) { }
    }
  }
}