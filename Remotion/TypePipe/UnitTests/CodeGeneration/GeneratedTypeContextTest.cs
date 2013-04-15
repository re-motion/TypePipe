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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  [TestFixture]
  public class GeneratedTypeContextTest
  {
    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private MutableType _mutableType;
    private Type _generatedType;

    private GeneratedTypeContext _context;


    [SetUp]
    public void SetUp ()
    {
      _mutableType = MutableTypeObjectMother.Create();
      _generatedType = typeof (GeneratedType);

      _context = new GeneratedTypeContext (new[] { new KeyValuePair<MutableType, Type> (_mutableType, _generatedType) });
    }

    [Test]
    public void GetGeneratedType ()
    {
      var result = _context.GetGeneratedType (_mutableType);

      Assert.That (result, Is.SameAs (_generatedType));
    }

    [Test]
    public void GetGeneratedXXX_Members ()
    {
      // TODO 5461: MutableGenericParameters on MutableType.

      var addedTypeInitializer = _mutableType.AddTypeInitializer (ctx => Expression.Empty());
      var addedField = _mutableType.AddField ("_field");
      var addedConstructor = _mutableType.AddConstructor();
      var addedMethod = MutableTypeTestExtensions.AddMethod (_mutableType, "Method");
      var addedProperty = _mutableType.AddProperty ("Property");
      var addedEvent = _mutableType.AddEvent ("Event");

      var typeInitializer = _generatedType.TypeInitializer;
      var field = _generatedType.GetFields (c_all).Single (f => f.Name == "_field");
      var constructor = _generatedType.GetConstructors().Single();
      var method = _generatedType.GetMethods (c_all).Single (m => m.Name == "Method");
      var property = _generatedType.GetProperties (c_all).Single (p => p.Name == "Property");
      var event_ = _generatedType.GetEvents (c_all).Single (e => e.Name == "Event");

      Assert.That (_context.GetGeneratedConstructor (addedTypeInitializer), Is.SameAs (typeInitializer));
      Assert.That (_context.GetGeneratedField (addedField), Is.SameAs (field));
      Assert.That (_context.GetGeneratedConstructor (addedConstructor), Is.SameAs (constructor));
      Assert.That (_context.GetGeneratedMethod (addedMethod), Is.SameAs (method));
      Assert.That (_context.GetGeneratedProperty (addedProperty), Is.SameAs (property));
      Assert.That (_context.GetGeneratedEvent (addedEvent), Is.SameAs (event_));
    }

    [Test]
    public void GetGeneratedXXX_Members_NoTypeInitializer ()
    {
      var addedField = _mutableType.AddField ("_field");
      var field = _generatedType.GetField ("_field", c_all);

      Assert.That (_context.GetGeneratedMember (addedField), Is.SameAs (field));
    }

    private class GeneratedType
    {
      static GeneratedType() {}
      private int _field;
      public static void Method () {}
      internal int Property { get; set; }
      public event Action Event;
    }
  }
}