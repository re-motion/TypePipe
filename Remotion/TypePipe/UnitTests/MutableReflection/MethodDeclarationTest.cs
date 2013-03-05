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
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MethodDeclarationTest
  {
    [Ignore]
    [Test]
    public void CreateForEquivalentSignature ()
    {
      var method = GetType().GetMethod ("Method");

      var decl = MethodDeclaration.CreateForEquivalentSignature (method);

      var context = new GenericParameterContext (new[] { ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeOtherType() });
      Assert.That (decl.GenericParameters, Has.Count.EqualTo (2));
      GenericParameterDeclarationTest.CheckGenericParameter (
          decl.GenericParameters[0],
          context,
          "TRet",
          GenericParameterAttributes.NotNullableValueTypeConstraint,
          expectedBaseTypeConstraint: null,
          expectedInterfaceConstraints: typeof (IList<>).MakeGenericType (context.GenericParameters[0]));
      GenericParameterDeclarationTest.CheckGenericParameter (
          decl.GenericParameters[1],
          context,
          "TRet",
          GenericParameterAttributes.DefaultConstructorConstraint,
          typeof (MethodDeclarationTest),
          new[] { typeof (IDisposable) });

      var returnType = decl.ReturnTypeProvider (context);
      Assert.That (returnType, Is.SameAs (context.GenericParameters[0]));

      var parameters = decl.ParameterProvider (context).ToList();
      Assert.That (parameters, Has.Count.EqualTo (2));
      ParameterDeclarationTest.CheckParameter (parameters[0], typeof (int).MakeByRefType(), "i", ParameterAttributes.Out);
      ParameterDeclarationTest.CheckParameter (parameters[1], context.GenericParameters[1], "t", ParameterAttributes.None);

      Assert.That (decl.GenericParameters, Has.Count.EqualTo (2));
      Assert.That (decl.GenericParameters, Has.Count.EqualTo (2));
    }

    private TRet Method<TRet, TArg> (out int i, TArg t)
        where TRet : struct, IList<TRet>
        where TArg : MethodDeclarationTest, IDisposable, new()
    {
      i = 7;
      return default (TRet);
    }
  }
}