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
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class MemberEmitterContextTest
  {
    private MemberEmitterContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = MemberEmitterContextObjectMother.GetSomeContext ();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_context.PostDeclarationsActionManager.Actions, Is.Empty);
      Assert.That (_context.TrampolineMethods, Is.Empty);
      Assert.That (_context.ConstructorRunCounter, Is.Null);
      Assert.That (_context.InitializationMethod, Is.Null);
    }

    [Test]
    public void TrampolineMethods_MemberInfoEqualityComparer ()
    {
      var method1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((object obj) => obj.ToString());
      var method2 = typeof (MemberEmitterContextTest).GetMethod ("ToString");
      Assert.That (method1, Is.Not.SameAs (method2));

      _context.TrampolineMethods.Add (method1, null);
      Assert.That (_context.TrampolineMethods.ContainsKey (method2), Is.True);
    }
  }
}