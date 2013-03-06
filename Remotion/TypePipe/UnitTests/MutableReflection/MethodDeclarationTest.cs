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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MethodDeclarationTest
  {
    [Test]
    public void CreateEquivalent ()
    {
      var method = GetType().GetMethod ("Method");

      var decl = MethodDeclaration.CreateEquivalent (method);

      var context = new GenericParameterContext (new[] { ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeOtherType() });
      Assert.That (decl.GenericParameters, Has.Count.EqualTo (2));
      GenericParameterDeclarationTest.CheckGenericParameter (
          decl.GenericParameters[0],
          context,
          "TRet",
          GenericParameterAttributes.DefaultConstructorConstraint,
          expectedConstraints: new[] { typeof (IDisposable) });
      GenericParameterDeclarationTest.CheckGenericParameter (
          decl.GenericParameters[1],
          context,
          "TArg",
          GenericParameterAttributes.None,
          typeof (List<>).MakeGenericType (context.GenericParameters[1]),
          context.GenericParameters[0],
          typeof (IList<>).MakeGenericType (context.GenericParameters[1]));

      var returnType = decl.ReturnTypeProvider (context);
      Assert.That (returnType, Is.SameAs (context.GenericParameters[0]));

      var parameters = decl.ParameterProvider (context).ToList();
      Assert.That (parameters, Has.Count.EqualTo (2));
      ParameterDeclarationTest.CheckParameter (parameters[0], typeof (int).MakeByRefType(), "i", ParameterAttributes.Out);
      ParameterDeclarationTest.CheckParameter (parameters[1], context.GenericParameters[1], "t", ParameterAttributes.None);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified method must be either a non-generic method or a generic method definition; it cannot be a method instantiation.\r\n"
        + "Parameter name: method")]
    public void CreateEquivalent_MethodInstantiation ()
    {
      MethodDeclaration.CreateEquivalent (ReflectionObjectMother.GetSomeMethodInstantiation());
    }

    public TRet Method<TRet, TArg> (out int i, TArg t)
        where TRet : IDisposable, new()
        where TArg : List<TArg>, TRet, IList<TArg>
    {
      i = 7;
      return default (TRet);
    }
  }
}