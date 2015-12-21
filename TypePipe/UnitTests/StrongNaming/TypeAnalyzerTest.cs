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
using NUnit.Framework;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class TypeAnalyzerTest
  {
    private IAssemblyAnalyzer _assemblyAnalyzerMock;

    private TypeAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _assemblyAnalyzerMock = MockRepository.GenerateStrictMock<IAssemblyAnalyzer>();

      _analyzer = new TypeAnalyzer (_assemblyAnalyzerMock);
    }

    [Test]
    public void IsStrongNamed ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (type.Assembly)).Return (fakeResult);

      var result1 = _analyzer.IsStrongNamed (type);
      var result2 = _analyzer.IsStrongNamed (type);

      _assemblyAnalyzerMock.VerifyAllExpectations();
      Assert.That (result1, Is.EqualTo (fakeResult));
      Assert.That (result2, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_Generic ()
    {
      var type = typeof (IList<Lazy<IParticipant>>);
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (typeof (IList<>).Assembly)).Return (true);
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (typeof (Lazy<>).Assembly)).Return (true);
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (typeof (IParticipant).Assembly)).Return (fakeResult);

      var result = _analyzer.IsStrongNamed (type);

      _assemblyAnalyzerMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_CacheUsesReferenceEquality ()
    {
      var type = typeof (object);
      var proxyType = MutableTypeObjectMother.Create (baseType: type, memberSelector: null);
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (type.Assembly)).Return (true);
      _assemblyAnalyzerMock.Expect (x => x.IsStrongNamed (proxyType.Assembly)).Return (true);

      _analyzer.IsStrongNamed (type);
      _analyzer.IsStrongNamed (proxyType);

      _assemblyAnalyzerMock.VerifyAllExpectations();
    }

    [Test]
    public void IsStrongNamed_TypeBuilder ()
    {
      var typeBuilder = ReflectionEmitObjectMother.CreateTypeBuilder();
      Assert.That (typeBuilder.GetGenericArguments(), Is.Null);
      _assemblyAnalyzerMock.Stub (x => x.IsStrongNamed (typeBuilder.Assembly)).Return (true);

      Assert.That (_analyzer.IsStrongNamed (typeBuilder), Is.True);
    }
  }
}