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
using Moq;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class TypeAnalyzerTest
  {
    private Mock<IAssemblyAnalyzer> _assemblyAnalyzerMock;

    private TypeAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _assemblyAnalyzerMock = new Mock<IAssemblyAnalyzer> (MockBehavior.Strict);

      _analyzer = new TypeAnalyzer (_assemblyAnalyzerMock.Object);
    }

    [Test]
    public void IsStrongNamed ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (type.Assembly)).Returns (fakeResult).Verifiable();

      var result1 = _analyzer.IsStrongNamed (type);
      var result2 = _analyzer.IsStrongNamed (type);

      _assemblyAnalyzerMock.Verify();
      Assert.That (result1, Is.EqualTo (fakeResult));
      Assert.That (result2, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_Generic ()
    {
      var type = typeof (IList<Lazy<IParticipant>>);
      var fakeResult = BooleanObjectMother.GetRandomBoolean();
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (typeof (IList<>).Assembly)).Returns (true).Verifiable();
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (typeof (Lazy<>).Assembly)).Returns (true).Verifiable();
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (typeof (IParticipant).Assembly)).Returns (fakeResult).Verifiable();

      var result = _analyzer.IsStrongNamed (type);

      _assemblyAnalyzerMock.Verify();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void IsStrongNamed_CacheUsesReferenceEquality ()
    {
      var type = typeof (object);
      var proxyType = MutableTypeObjectMother.Create (baseType: type, memberSelector: null);
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (type.Assembly)).Returns (true).Verifiable();
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (proxyType.Assembly)).Returns (true).Verifiable();

      _analyzer.IsStrongNamed (type);
      _analyzer.IsStrongNamed (proxyType);

      _assemblyAnalyzerMock.Verify();
    }

    [Test]
    public void IsStrongNamed_TypeBuilder ()
    {
      var typeBuilder = ReflectionEmitObjectMother.CreateTypeBuilder();
      Assert.That (typeBuilder.GetGenericArguments(), Is.Null);
      _assemblyAnalyzerMock.Setup (x => x.IsStrongNamed (typeBuilder.Assembly)).Returns (true);

      Assert.That (_analyzer.IsStrongNamed (typeBuilder), Is.True);
    }
  }
}