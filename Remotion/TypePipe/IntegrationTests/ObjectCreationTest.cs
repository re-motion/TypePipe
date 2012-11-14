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

namespace TypePipe.IntegrationTests
{
  [TestFixture]
  public class ObjectCreationTest : ObjectFactoryIntegrationTestBase
  {
    [Test]
    public void ConstructorArguments ()
    {
      var pipeline = CreateObjectFactory();

      var instance = pipeline.CreateInstance<DomainType>();
      Assert.That (instance.String, Is.EqualTo ("default .ctor"));

      var obj = new object();
      instance = pipeline.CreateInstance<DomainType> (ParamList.Create (obj));
      Assert.That (instance.Obj, Is.SameAs (obj));

      instance = pipeline.CreateInstance<DomainType> (ParamList.Create ("abc"));
      Assert.That (instance.String, Is.EqualTo ("abc"));

      instance = pipeline.CreateInstance<DomainType> (ParamList.Create ((IEnumerable<char>) "abc"));
      Assert.That (instance.Enumerable, Is.EqualTo ("abc"));
      Assert.That (instance.String, Is.Null);

      instance = pipeline.CreateInstance<DomainType> (ParamList.Create (new[] { 'a', 'b', 'c' }));
      Assert.That (instance.CharArray, Is.EqualTo (new[] { 'a', 'b', 'c' }));
    }

    // TODO Review: Add integration test for non-generic overload

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

      // TODO Review: Test protected ctor
    }
  }
}