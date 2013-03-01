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
using System.Text;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.UnitTests.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class GenericParameterCompatibleMethodSignatureStringBuilderHelperTest
  {
    private MethodSignatureStringBuilderHelper _originalBuilder;
    private GenericParameterCompatibleMethodSignatureStringBuilderHelper _builder;

    private StringBuilder _sb1;
    private StringBuilder _sb2;

    [SetUp]
    public void SetUp ()
    {
      _originalBuilder = new MethodSignatureStringBuilderHelper();
      _builder = new GenericParameterCompatibleMethodSignatureStringBuilderHelper();

      _sb1 = new StringBuilder();
      _sb2 = new StringBuilder();
    }

    [Test]
    public void AppendTypeString_CallsBase_ForStandardType ()
    {
      var someType = ReflectionObjectMother.GetSomeType();

      _builder.AppendTypeString (_sb1, someType);
      _originalBuilder.AppendTypeString (_sb2, someType);

      Assert.That (_sb1.ToString(), Is.EqualTo (_sb2.ToString()));
    }

    [Test]
    public void AppendTypeString_CallsBase_ForInitializedGenericParam ()
    {
      var initializedGenericParam = GenericParameterObjectMother.Create();
      initializedGenericParam.InitializeDeclaringMember (MutableMethodInfoObjectMother.Create());

      _builder.AppendTypeString (_sb1, initializedGenericParam);
      _originalBuilder.AppendTypeString (_sb2, initializedGenericParam);

      Assert.That (_sb1.ToString(), Is.EqualTo (_sb2.ToString()));
    }

    [Test]
    public void AppendTypeString_CallsBase_ForUninitializedGenericParam ()
    {
      var uninitializedGenericParam = GenericParameterObjectMother.Create (position: 7);

      _builder.AppendTypeString (_sb1, uninitializedGenericParam);
      Assert.That (() => _originalBuilder.AppendTypeString (_sb2, uninitializedGenericParam), Throws.Exception);

      Assert.That (_sb1.ToString(), Is.EqualTo ("[7]"));
    }
  }
}