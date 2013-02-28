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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class TypeSubstitutionUtilityTest
  {
    private Type _parameter;
    private Type _argument;
    private Dictionary<Type, Type> _parametersToArguments;
    private Dictionary<TypeInstantiationInfo, TypeInstantiation> _instantiations;

    [SetUp]
    public void SetUp ()
    {
      _parameter = typeof (GenericType<>).GetGenericArguments().Single();
      _argument = ReflectionObjectMother.GetSomeType();
      _parametersToArguments = new Dictionary<Type, Type> { {_parameter, _argument} };
      _instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation> ();
    }

    [Test]
    public void SubstituteGenericParameters_GenericParameter ()
    {
      var genericParameter = typeof (GenericType<>).GetField ("Field").FieldType;

      var result = TypeSubstitutionUtility.SubstituteGenericParameters (_parametersToArguments, _instantiations, genericParameter);

      Assert.That (result, Is.SameAs (_argument));
    }

    [Test]
    public void SubstituteGenericParameters_GenericParameter_NoMatch ()
    {
      var parametersToArguments = new Dictionary<Type, Type>();
      var genericParameter = typeof (GenericType<>).GetField ("Field").FieldType;

      var result = TypeSubstitutionUtility.SubstituteGenericParameters (parametersToArguments, _instantiations, genericParameter);

      Assert.That (result, Is.SameAs (genericParameter));
    }

    [Test]
    public void SubstituteGenericParameters_RecursiveGenericType ()
    {
      var recursiveGeneric = typeof(GenericType<>).GetField ("RecursiveGeneric").FieldType;

      var list = TypeSubstitutionUtility.SubstituteGenericParameters (_parametersToArguments, _instantiations, recursiveGeneric);

      Assert.That (list.GetGenericTypeDefinition(), Is.SameAs (typeof (List<>)));
      var func = list.GetGenericArguments().Single();
      Assert.That (func.GetGenericTypeDefinition(), Is.SameAs (typeof (Func<>)));
      var typeArgument = func.GetGenericArguments().Single();
      Assert.That (typeArgument, Is.SameAs (_argument));
    }

    [Test]
    public void SubstituteGenericParameters_NonGenericType ()
    {
      var nonGeneric = ReflectionObjectMother.GetSomeNonGenericType ();
      var result = TypeSubstitutionUtility.SubstituteGenericParameters (_parametersToArguments, _instantiations, nonGeneric);

      Assert.That (result, Is.SameAs (nonGeneric));
    }

    class GenericType<T>
    {
      public T Field;
      public List<Func<T>> RecursiveGeneric;
    }
  }
}