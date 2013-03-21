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
// using System;

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Collections;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class GeneratedTypeContextTest
  {
    private Dictionary<MutableType, Type> _mapping;

    private GeneratedTypeContext _context;

    [SetUp]
    public void SetUp ()
    {
      _mapping = new Dictionary<MutableType, Type>();

      _context = new GeneratedTypeContext (_mapping.AsReadOnly());
    }

    [Test]
    public void GetGeneratedMember ()
    {
      var mutableType = MutableTypeObjectMother.Create();
      var generatedType = ReflectionObjectMother.GetSomeType();
      _mapping.Add (mutableType, generatedType);

      var result = _context.GetGeneratedMember (mutableType);

      Assert.That (result, Is.SameAs (generatedType));
    }
  }
}