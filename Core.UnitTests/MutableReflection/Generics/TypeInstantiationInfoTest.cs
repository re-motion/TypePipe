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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeInstantiationInfoTest
  {
    private Type _genericTypeDefinition;
    private Type _typeArg;

    private TypeInstantiationInfo _info1;
    private TypeInstantiationInfo _info2;
    private TypeInstantiationInfo _info3;
    private TypeInstantiationInfo _info4;

    [SetUp]
    public void SetUp ()
    {
      _genericTypeDefinition = typeof (List<>);
      _typeArg = ReflectionObjectMother.GetSomeType();

      _info1 = new TypeInstantiationInfo (_genericTypeDefinition, new[] { _typeArg }.AsOneTime());
      _info2 = new TypeInstantiationInfo (typeof (Func<>), new[] { _typeArg });
      _info3 = new TypeInstantiationInfo (_genericTypeDefinition, new[] { ReflectionObjectMother.GetSomeOtherType() });
      _info4 = new TypeInstantiationInfo (_genericTypeDefinition, new[] { _typeArg });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_info1.GenericTypeDefinition, Is.SameAs (_genericTypeDefinition));
      Assert.That (_info1.TypeArguments, Is.EqualTo (new[] { _typeArg }));
    }

    [Test]
    public void Initialization_NoGenericTypeDefinition ()
    {
      Assert.That (
          () => Dev.Null = new TypeInstantiationInfo (typeof (List<int>), Type.EmptyTypes),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Specified type must be a generic type definition.", "genericTypeDefinition"));
    }

    [Test]
    public void Initialization_NonMatchingGenericArgumentCount ()
    {
      Assert.That (
          () => Dev.Null = new TypeInstantiationInfo (typeof (List<>), Type.EmptyTypes),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Generic parameter count of the generic type definition does not match the number of supplied type arguments.", "typeArguments"));
    }

    [Test]
    public void Equals ()
    {
      Assert.That (_info1.Equals (null), Is.False);
      Assert.That (_info1.Equals (new object()), Is.False);
      Assert.That (_info1.Equals (_info2), Is.False);
      Assert.That (_info1.Equals (_info3), Is.False);
      Assert.That (_info1.Equals (_info4), Is.True);
    }

    [Test]
    public new void GetHashCode ()
    {
      Assert.That (_info1.GetHashCode(), Is.EqualTo (_info4.GetHashCode()));
    }
  }
}