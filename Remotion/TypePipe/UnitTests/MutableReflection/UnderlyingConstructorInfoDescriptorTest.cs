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
      var parameterDeclarations = new[] { new ParameterDeclaration (typeof (object), "xxx") };

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarations);

      Assert.That (descriptor.UnderlyingSystemConstructorInfo, Is.Null);
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.ParameterDeclarations, Is.EqualTo (parameterDeclarations));
     
      var actualParameterExpressionData = descriptor.ParameterExpressions.Select (pe => new { pe.Name, pe.Type, pe.IsByRef });
      var expectedParameterExpressionData = new[] { new { Name = "xxx", Type = typeof (object), IsByRef = false } };
      Assert.That (actualParameterExpressionData, Is.EqualTo (expectedParameterExpressionData));
    }

    [Test]
    public void Create_ForNew_WithByRefParameterType ()
    {
      var attributes = MethodAttributes.Abstract;
      var parameterDeclarations = new[] { new ParameterDeclaration (typeof (object).MakeByRefType(), "xxx") };

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarations);

      Assert.That (descriptor.ParameterDeclarations, Is.EqualTo (parameterDeclarations));

      var actualParameterExpressionData = descriptor.ParameterExpressions.Select (pe => new { pe.Name, pe.Type, pe.IsByRef });
      var expectedParameterExpressionData = new[] { new { Name = "xxx", Type = typeof (object), IsByRef = true } };
      Assert.That (actualParameterExpressionData, Is.EqualTo (expectedParameterExpressionData));
    }

    [Test]
    public void Create_ForExisting ()
    {
      int v;
      var originalCtor = ReflectionObjectMother.GetConstructor (() => new ExampleType ("string", out v, 1.0, null));
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalCtor);

      Assert.That (descriptor.UnderlyingSystemConstructorInfo, Is.SameAs (originalCtor));
      Assert.That (descriptor.Attributes, Is.EqualTo (originalCtor.Attributes));

      var expectedParamterDecls =
          new[]
          {
              new { Type = typeof (string), Name = "s", Attributes = ParameterAttributes.None },
              new { Type = typeof (int).MakeByRefType(), Name = "i", Attributes = ParameterAttributes.Out },
              new { Type = typeof (double), Name = "d", Attributes = ParameterAttributes.In },
              new { Type = typeof (object), Name = "o", Attributes = ParameterAttributes.In | ParameterAttributes.Out },
          };
      var actualParameterDecls = descriptor.ParameterDeclarations.Select (pd => new { pd.Type, pd.Name, pd.Attributes });
      Assert.That (actualParameterDecls, Is.EqualTo (expectedParamterDecls));

      var actualParameterExpressionData = descriptor.ParameterExpressions.Select (pe => new { pe.Name, pe.Type, pe.IsByRef });
      var expectedParameterExpressionData = new[]
                                            {
                                                new { Name = "s", Type = typeof (string), IsByRef = false },
                                                new { Name = "i", Type = typeof (int), IsByRef = true },
                                                new { Name = "d", Type = typeof (double), IsByRef = false },
                                                new { Name = "o", Type = typeof (object), IsByRef = false },
                                            };
      Assert.That (actualParameterExpressionData, Is.EqualTo (expectedParameterExpressionData));
    }

    private class ExampleType
    {
      public ExampleType (string s, out int i, [In] double d, [In, Out] object o)
      {
        i = 5;
      }
    }
  }
}