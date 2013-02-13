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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;
using Remotion.Collections;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class InstantiationInfoTest
  {
    private Type _customType;
    private Type _runtimeType;

    private InstantiationInfo _info1;
    private InstantiationInfo _info2;
    private InstantiationInfo _info3;
    private InstantiationInfo _info4;

    private Dictionary<InstantiationInfo, TypeInstantiation> _instantiations;

    [SetUp]
    public void SetUp ()
    {
      var genericTypeDef1 = typeof (List<>);
      var genericTypeDef2 = typeof (Func<>);

      _customType = CustomTypeObjectMother.Create();
      _runtimeType = ReflectionObjectMother.GetSomeType();

      _info1 = new InstantiationInfo (genericTypeDef1, new[] { _customType }.AsReadOnly());
      _info2 = new InstantiationInfo (genericTypeDef2, new[] { _customType });
      _info3 = new InstantiationInfo (genericTypeDef1, new[] { _runtimeType });
      _info4 = new InstantiationInfo (genericTypeDef1, new[] { _customType });

      _instantiations = new Dictionary<InstantiationInfo, TypeInstantiation>();
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_info1.GenericTypeDefinition, Is.SameAs (typeof (List<>)));
      Assert.That (_info1.TypeArguments, Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Instantiate_CustomTypeArgument ()
    {
      var result = _info1.Instantiate (_instantiations);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.GetGenericTypeDefinition(), Is.EqualTo (_info1.GenericTypeDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (_info1.TypeArguments));
    }

    [Test]
    public void Instantiate_RuntimeTypeArgument ()
    {
      var result = _info2.Instantiate (_instantiations);

      Assert.That (result, Is.TypeOf<TypeInstantiation>());
      Assert.That (result.GetGenericTypeDefinition(), Is.EqualTo (_info2.GenericTypeDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (_info2.TypeArguments));
    }

    [Test]
    public void Instantiate_AlreadyInContext ()
    {
      var result1 = _info1.Instantiate (_instantiations);
      Assert.That (_instantiations[_info1], Is.SameAs (result1));
      var count = _instantiations.Count;

      var result2 = _info1.Instantiate (_instantiations);
      var result3 = _info1.Instantiate (new Dictionary<InstantiationInfo, TypeInstantiation>());

      Assert.That (_instantiations, Has.Count.EqualTo (count));
      Assert.That (result2, Is.SameAs (result1));
      Assert.That (result3, Is.Not.SameAs (result1));
    }

    [Test]
    public void Equals ()
    {
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