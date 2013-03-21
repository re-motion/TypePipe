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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.Collections;

namespace Remotion.TypePipe.UnitTests.MutableReflection
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

      _context = new GeneratedTypeContext (new[] { Tuple.Create (_mutableType, _generatedType) });
    }

    [Test]
    public void GetGeneratedMember_Type ()
    {
      var result = _context.GetGeneratedMember (_mutableType);

      Assert.That (result, Is.SameAs (_generatedType));
    }

    [Test]
    public void GetGeneratedMember_Member ()
    {
      // TODO 5461: MutableGenericParameters on MutableType.

      var addedTypeInitializer = _mutableType.AddTypeInitializer (ctx => Expression.Empty());
      var addedField = _mutableType.AddField ("_field");
      var addedConstructor = _mutableType.AddConstructor();
      var addedMethod = _mutableType.AddMethod ("Method");
      var addedProperty = _mutableType.AddProperty ("Property");
      var addedEvent = _mutableType.AddEvent ("Event");

      var typeInitializer = _generatedType.TypeInitializer;
      var field = _generatedType.GetField ("_field", c_all);
      var constructor = _generatedType.GetConstructors (c_all).Single(c => !c.IsStatic);
      var method = _generatedType.GetMethod ("Method", c_all);
      var property = _generatedType.GetProperty ("Property", c_all);
      var event_ = _generatedType.GetEvent ("Event", c_all);

      Assert.That (_context.GetGeneratedMember (addedTypeInitializer), Is.SameAs (typeInitializer));
      Assert.That (_context.GetGeneratedMember (addedField), Is.SameAs (field));
      Assert.That (_context.GetGeneratedMember (addedConstructor), Is.SameAs (constructor));
      Assert.That (_context.GetGeneratedMember (addedMethod), Is.SameAs (method));
      Assert.That (_context.GetGeneratedMember (addedProperty), Is.SameAs (property));
      Assert.That (_context.GetGeneratedMember (addedEvent), Is.SameAs (event_));
    }

    [Test]
    public void GetGeneratedMember_Member_NoTypeInitializer ()
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