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
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class MakeTypePipeGenericTypeTest
  {
    private ProxyType _typeArg1;
    private ProxyType _typeArg2;

    private Type _instantiation;

    [SetUp]
    public void SetUp ()
    {
      _typeArg1 = ProxyTypeObjectMother.Create (typeof (object));
      _typeArg2 = ProxyTypeObjectMother.Create (typeof (MakeTypePipeGenericTypeTest));

      _instantiation = typeof (GenericType<,>).MakeTypePipeGenericType (_typeArg1, _typeArg2);
    }

    [Test]
    public void TypeInstantiation ()
    {
      Assert.That (_instantiation, Is.TypeOf<TypeInstantiation>());
      Assert.That (_instantiation.GetGenericTypeDefinition(), Is.SameAs (typeof (GenericType<,>)));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void Names ()
    {
      Assert.That (_instantiation.Name, Is.EqualTo ("GenericType`2"));
      Assert.That (_instantiation.FullName, Is.EqualTo (
          "Remotion.TypePipe.IntegrationTests.MutableReflection.MakeTypePipeGenericTypeTest+GenericType`2[[System.Object_Proxy1, TypePipe_GeneratedAssembly],[Remotion.TypePipe.IntegrationTests.MutableReflection.MakeTypePipeGenericTypeTest_Proxy1, TypePipe_GeneratedAssembly]]"));
      Assert.That (_instantiation.ToString (), Is.EqualTo ("GenericType`2[Object_Proxy1,MakeTypePipeGenericTypeTest_Proxy1]"));
    }

    [Test]
    public void BaseType ()
    {
      var baseType = _instantiation.BaseType;
      Assertion.IsNotNull (baseType);
      Assert.That (baseType.Name, Is.EqualTo ("GenericBase`1"));
      Assert.That (baseType.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1 }));
    }

    [Test]
    public void Interfaces ()
    {
      var ifc = _instantiation.GetInterfaces().Single();
      Assert.That (ifc.Name, Is.EqualTo ("IMyInterface`1"));
      Assert.That (ifc.GetGenericArguments(), Is.EqualTo (new[] { _typeArg2 }));
    }

    [Test]
    public void Fields ()
    {
      var field = _instantiation.GetFields().Single();
      Assert.That (field.Name, Is.EqualTo ("Field"));
      Assert.That (field.FieldType, Is.SameAs (_typeArg1));
    }

    [Test]
    public void Constructors ()
    {
      var ctor = _instantiation.GetConstructors().Single();
      Assert.That (ctor.Name, Is.EqualTo (".ctor"));
      Assert.That (ctor.GetParameters().Select (p => p.ParameterType), Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void Methods ()
    {
      var method = _instantiation.GetMethods().Single (m => m.Name == "Method");
      Assert.That (method.ReturnType, Is.SameAs (_typeArg1));
      Assert.That (method.GetParameters().Single().ParameterType, Is.SameAs (_typeArg2));
    }

    [Ignore ("TODO 5387")]
    [Test]
    public void Properties ()
    {
      var property = _instantiation.GetProperties().Single();
      Assert.That (property.Name, Is.EqualTo ("Property"));
      Assert.That (property.PropertyType, Is.SameAs (_typeArg1));
    }

    [Ignore("TODO 5387")]
    [Test]
    public void Events ()
    {
      var evt = _instantiation.GetEvents().Single();
      Assert.That (evt.Name, Is.EqualTo ("Event"));
      var evtType = evt.EventHandlerType;
      Assert.That (evtType.Name, Is.EqualTo ("Func`1"));
      Assert.That (evtType.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1 }));
    }

    [Test]
    public void RecursiveTypeInstantiations_Substitution ()
    {
      var enumerable = _instantiation.GetMethod ("ReturnTypeMethod_NeedsSubstitute").ReturnType;
      Assert.That (enumerable, Is.TypeOf<TypeInstantiation>());
      Assert.That (enumerable.Name, Is.EqualTo ("IEnumerable`1"));
      var func = enumerable.GetGenericArguments().Single();
      Assert.That (func.Name, Is.EqualTo ("Func`2"));
      var typeArgs = func.GetGenericArguments();
      Assert.That (typeArgs, Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void RecursiveTypeInstantiations_NoSubstitution ()
    {
      var enumerable = _instantiation.GetMethod ("ReturnTypeMethod_RuntimeType").ReturnType;
      Assert.That (enumerable.IsRuntimeType(), Is.True);
    }

    interface IMyInterface<T> { }
    class GenericBase<T> { }
    class GenericType<T1, T2> : GenericBase<T1>, IMyInterface<T2>
    {
      public T1 Field = default(T1);
      public GenericType (T1 t1, T2 t2) { }
      public T1 Method (T2 t) { return default (T1); }
      public T1 Property { get; set; }
      public event Func<T1> Event;

      public IEnumerable<Func<T1, T2>> ReturnTypeMethod_NeedsSubstitute () { return null; }
      public IEnumerable<Func<int, string>> ReturnTypeMethod_RuntimeType () { return null; }
    }
  }
}