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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  public abstract class TypeAssemblerIntegrationTestBase : IntegrationTestBase
  {
    private const BindingFlags c_allDeclared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private bool _allowUnderlyingSystemTypeAccess;

    public override void SetUp ()
    {
      base.SetUp ();

      _allowUnderlyingSystemTypeAccess = false;
    }

    protected void AllowUnderlyingSystemTypeAccess ()
    {
      _allowUnderlyingSystemTypeAccess = true;
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected Type AssembleType<T> (params Action<ProxyType>[] participantActions)
    {
      return AssembleType (typeof (T), participantActions, 1);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected Type AssembleType (Type requestedType, params Action<ProxyType>[] participantActions)
    {
      return AssembleType (requestedType, participantActions, 1);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected Type AssembleType (Type requestedType, IEnumerable<Action<ProxyType>> participantActions, int stackFramesToSkip)
    {
      var testName = GetNameForThisTest (stackFramesToSkip + 1);
      return AssembleType (testName, requestedType, participantActions);
    }

    protected MethodInfo GetDeclaredMethod (Type type, string name)
    {
      var method = type.GetMethod (name, c_allDeclared);
      Assert.That (method, Is.Not.Null);
      return method;
    }

    protected MethodInfo GetDeclaredExplicitOverrideMethod (Type type, MethodInfo overriddenMethod)
    {
      var method = type.GetMethod (MethodOverrideUtility.GetNameForExplicitOverride (overriddenMethod), c_allDeclared);
      Assert.That (method, Is.Not.Null);
      return method;
    }

    protected MutableMethodInfo AddEquivalentMethod (
        ProxyType proxyType,
        MethodInfo template,
        MethodAttributes adjustedAttributes,
        Func<MethodBodyCreationContext, Expression> bodyProvider = null)
    {
      return proxyType.AddMethod (
          template.Name,
          adjustedAttributes,
          template.ReturnType,
          ParameterDeclaration.CreateForEquivalentSignature (template),
          bodyProvider ?? (ctx => Expression.Default (template.ReturnType)));
    }

    private Type AssembleType (string testName, Type requestedType, IEnumerable<Action<ProxyType>> participantActions)
    {
      var participants = participantActions.Select (CreateParticipant).AsOneTime();
      var underlyingTypeFactory =
          _allowUnderlyingSystemTypeAccess ? (IUnderlyingTypeFactory) new UnderlyingTypeFactory() : new ThrowingUnderlyingTypeFactory();
      var proxyTypeModelFactory = new ProxyTypeModelFactory (underlyingTypeFactory);
      var subclassProxyBuilder = CreateSubclassProxyBuilder (testName);
      var typeAssembler = new TypeAssembler (participants, proxyTypeModelFactory, subclassProxyBuilder);

      return typeAssembler.AssembleType (requestedType);
    }
  }
}