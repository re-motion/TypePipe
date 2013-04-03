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
using NUnit.Framework;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class CodeManagerTest
  {
    private ICodeGenerator _codeGeneratorMock;
    private ITypeCache _typeCacheMock;

    private CodeManager _manager;

    [SetUp]
    public void SetUp ()
    {
      _codeGeneratorMock = MockRepository.GenerateStrictMock<ICodeGenerator>();
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();

      _manager = new CodeManager (_codeGeneratorMock, _typeCacheMock);
    }

    [Test]
    public void FlushCodeToDisk ()
    {
      var configID = "config";
      var fakeResult = "assembly path";
      _typeCacheMock.Expect (mock => mock.ParticipantConfigurationID).Return (configID);
      _codeGeneratorMock.Expect (mock => mock.FlushCodeToDisk (configID)).Return (fakeResult);

      var result = _manager.FlushCodeToDisk();

      _typeCacheMock.VerifyAllExpectations();
      _codeGeneratorMock.VerifyAllExpectations();
      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void DelegatingMembers ()
    {
      _codeGeneratorMock.Expect (mock => mock.AssemblyDirectory).Return ("get dir");
      Assert.That (_manager.AssemblyDirectory, Is.EqualTo ("get dir"));
      _codeGeneratorMock.Expect (mock => mock.AssemblyName).Return ("get name");
      Assert.That (_manager.AssemblyName, Is.EqualTo ("get name"));

      _codeGeneratorMock.Expect (mock => mock.SetAssemblyDirectory ("set dir"));
      _manager.SetAssemblyDirectory ("set dir");
      _codeGeneratorMock.Expect (mock => mock.SetAssemblyName ("set name"));
      _manager.SetAssemblyName ("set name");

      var assembly = GetType().Assembly;
      _typeCacheMock.Expect (mock => mock.LoadFlushedCode (assembly));
      _manager.LoadFlushedCode (assembly);

      _codeGeneratorMock.VerifyAllExpectations();
      _typeCacheMock.VerifyAllExpectations();
    }
  }
}