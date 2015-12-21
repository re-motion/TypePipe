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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.TypePipe.StrongNaming;

namespace Remotion.TypePipe.UnitTests.StrongNaming
{
  [TestFixture]
  public class AssemblyAnalyzerTest
  {
    private AssemblyAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _analyzer = new AssemblyAnalyzer();
    }

    [Test]
    public void IsStrongNamed ()
    {
      var assembly1 = typeof (AssemblyAnalyzerTest).Assembly;
      var assembly2 = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("test1"), AssemblyBuilderAccess.Run);

      Assert.That (_analyzer.IsStrongNamed (assembly1), Is.True);
      Assert.That (_analyzer.IsStrongNamed (assembly2), Is.False);
    }
  }
}