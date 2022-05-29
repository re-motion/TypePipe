﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration;
using Moq;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class AssemblyContextTest
  {
    private AssemblyContext _assemblyContext;
    private Mock<IMutableTypeBatchCodeGenerator> _mutableTypeBatchCodeGenerator;
    private Mock<IGeneratedCodeFlusher> _generatedCodeFlusher;

    [SetUp]
    public void SetUp ()
    {
      _mutableTypeBatchCodeGenerator = new Mock<IMutableTypeBatchCodeGenerator> (MockBehavior.Strict);
      _generatedCodeFlusher = new Mock<IGeneratedCodeFlusher> (MockBehavior.Strict);

      _assemblyContext = new AssemblyContext (_mutableTypeBatchCodeGenerator.Object, _generatedCodeFlusher.Object);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_assemblyContext.ParticipantState, Is.Not.Null);
      Assert.That (_assemblyContext.ParticipantState, Is.SameAs (_assemblyContext.ParticipantState));

      Assert.That (_assemblyContext.MutableTypeBatchCodeGenerator, Is.SameAs (_mutableTypeBatchCodeGenerator.Object));
      Assert.That (_assemblyContext.GeneratedCodeFlusher, Is.SameAs (_generatedCodeFlusher.Object));
    }

    [Test]
    public void ResetParticipantState ()
    {
      var participantState = _assemblyContext.ParticipantState;
      _assemblyContext.ResetParticipantState();
      Assert.That (_assemblyContext.ParticipantState, Is.Not.SameAs (participantState));
    }
  }
}