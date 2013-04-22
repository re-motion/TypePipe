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
using Remotion.Reflection;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class ObjectCreationTest : IntegrationTestBase
  {
    [Test]
    public void ConstructorArguments_Generic ()
    {
      var pipeline = CreatePipeline();

      var instance = pipeline.Create<DomainType>();
      Assert.That (instance.String, Is.EqualTo ("default .ctor"));

      var obj = new object();
      instance = pipeline.Create<DomainType> (ParamList.Create (obj));
      Assert.That (instance.Obj, Is.SameAs (obj));

      instance = pipeline.Create<DomainType> (ParamList.Create ("abc"));
      Assert.That (instance.String, Is.EqualTo ("abc"));

      instance = pipeline.Create<DomainType> (ParamList.Create ((IEnumerable<char>) "abc"));
      Assert.That (instance.Enumerable, Is.EqualTo ("abc"));
      Assert.That (instance.String, Is.Null);

      instance = pipeline.Create<DomainType> (ParamList.Create (new[] { 'a', 'b', 'c' }));
      Assert.That (instance.CharArray, Is.EqualTo (new[] { 'a', 'b', 'c' }));
    }

    [Test]
    public void NonGeneric ()
    {
      var pipeline = CreatePipeline();

      var instance = (DomainType) pipeline.Create (typeof (DomainType));
      Assert.That (instance.String, Is.EqualTo ("default .ctor"));

      instance = (DomainType) pipeline.Create (typeof (DomainType), ParamList.Create ("abc"));
      Assert.That (instance.String, Is.EqualTo ("abc"));
    }

    [Test]
    public void AllowNonPublic ()
    {
      var pipeline = CreatePipeline();

      var instance1 = pipeline.Create<DomainType> (ParamList.Create (7), allowNonPublicConstructor: true);
      var instance2 = (DomainType) pipeline.Create (typeof (DomainType), ParamList.Create (8), allowNonPublicConstructor: true);

      Assert.That (instance1.String, Is.EqualTo ("7"));
      Assert.That (instance2.String, Is.EqualTo ("8"));
    }

    public class DomainType
    {
      public readonly object Obj;      
      public readonly string String;
      public readonly IEnumerable<char> Enumerable;
      public readonly char[] CharArray;

      public DomainType () { String = "default .ctor"; }
      public DomainType (object obj) { Obj = obj; }
      public DomainType (string @string) { String = @string; }
      public DomainType (IEnumerable<char> enumerable) { Enumerable = enumerable; }
      public DomainType (char[] charArray) { CharArray = charArray; }

      protected DomainType (int i) { String = "" + i; }
    }
  }
}