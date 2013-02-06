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
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiatorTest
  {
    private Type _genericType;
    private Type _constructedType;
    private Type[] _typeArguments;

    private TypeInstantiator _typeInstantiator;

    [SetUp]
    public void SetUp ()
    {
      _genericType = typeof (MyGenericType<,>);
      _constructedType = typeof (MyGenericType<List<int>, string>);
      _typeArguments = new[] { typeof (List<int>), typeof (string) };

      _typeInstantiator = new TypeInstantiator (_typeArguments.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_typeInstantiator.TypeArguments, Is.EqualTo (_typeArguments));
    }

    [Test]
    public void GetFullName ()
    {
      var result = _typeInstantiator.GetFullName (_genericType);

      Assert.That (result,Is.EqualTo ("Remotion.TypePipe.UnitTests.MutableReflection.Generics.TypeInstantiatorTest+MyGenericType`2[[System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"));
      Assert.That (result, Is.EqualTo (_constructedType.FullName), "Should be equal to original reflection.");
    }


    class MyGenericType<T1, T2> { }
  }
}