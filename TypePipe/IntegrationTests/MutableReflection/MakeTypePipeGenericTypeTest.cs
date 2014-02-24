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
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class MakeTypePipeGenericTypeTest
  {
    private Type _genericTypeDefinition;
    private MutableType _typeArg1;
    private MutableType _typeArg2;

    private Type _instantiation;

    [SetUp]
    public void SetUp ()
    {
      _genericTypeDefinition = typeof (GenericType<,>);
      _typeArg1 = MutableTypeObjectMother.Create (typeof (object));
      _typeArg2 = MutableTypeObjectMother.Create (typeof (MakeTypePipeGenericTypeTest));

      _instantiation = _genericTypeDefinition.MakeTypePipeGenericType (_typeArg1, _typeArg2);
    }

    [Test]
    public void TypeInstantiation ()
    {
      Assert.That (_instantiation, Is.TypeOf<TypeInstantiation>());
      Assert.That (_instantiation.GetGenericTypeDefinition(), Is.SameAs (_genericTypeDefinition));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void Names ()
    {
      Assert.That (_instantiation.Name, Is.EqualTo ("GenericType`2"));
      Assert.That (_instantiation.FullName, Is.EqualTo (
          "Remotion.TypePipe.IntegrationTests.MutableReflection.MakeTypePipeGenericTypeTest+GenericType`2[[System.Object_AssembledTypeProxy_1, TypePipe_GeneratedAssembly],[Remotion.TypePipe.IntegrationTests.MutableReflection.MakeTypePipeGenericTypeTest_AssembledTypeProxy_1, TypePipe_GeneratedAssembly]]"));
      Assert.That (_instantiation.ToString (), Is.EqualTo ("GenericType`2[Object_AssembledTypeProxy_1,MakeTypePipeGenericTypeTest_AssembledTypeProxy_1]"));
    }

    [Test]
    public void BaseType ()
    {
      var genericBaseType = typeof (GenericBase<>);
      var baseType = _instantiation.BaseType;

      Assertion.IsNotNull (baseType);
      Assert.That (baseType.Name, Is.EqualTo ("GenericBase`1"));
      Assert.That (baseType, Is.TypeOf<TypeInstantiation>());
      Assert.That (baseType.GetGenericTypeDefinition(), Is.SameAs (genericBaseType));
      Assert.That (baseType.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1 }));
    }

    [Test]
    public void Interfaces ()
    {
      var genericIfc = typeof (IMyInterface<>);
      var ifc = _instantiation.GetInterfaces().Single();

      Assert.That (ifc.Name, Is.EqualTo ("IMyInterface`1"));
      Assert.That (ifc, Is.TypeOf<TypeInstantiation>());
      Assert.That (ifc.GetGenericTypeDefinition(), Is.SameAs (genericIfc));
      Assert.That (ifc.GetGenericArguments(), Is.EqualTo (new[] { _typeArg2 }));
    }

    [Test]
    public void Fields ()
    {
      var genericField = _genericTypeDefinition.GetField ("Field");
      var field = _instantiation.GetField ("Field");

      Assert.That (field, Is.TypeOf<FieldOnTypeInstantiation>());
      Assert.That (field.As<FieldOnTypeInstantiation>().FieldOnGenericType, Is.EqualTo (genericField));
      Assert.That (field.FieldType, Is.SameAs (_typeArg1));
    }

    [Test]
    public void Constructors ()
    {
      var genericCtor = _genericTypeDefinition.GetConstructors().Single();
      var ctor = _instantiation.GetConstructors().Single();

      Assert.That (ctor, Is.TypeOf<ConstructorOnTypeInstantiation>());
      Assert.That (ctor.As<ConstructorOnTypeInstantiation>().ConstructorOnGenericType, Is.EqualTo (genericCtor));
      Assert.That (ctor.GetParameters().Select (p => p.ParameterType), Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void Methods ()
    {
      var genericMethod = _genericTypeDefinition.GetMethod ("Method");
      var method = _instantiation.GetMethod ("Method");

      Assert.That (method, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (method.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (genericMethod));
      Assert.That (method.ReturnType, Is.SameAs (_typeArg1));
      Assert.That (method.GetParameters().Single().ParameterType, Is.SameAs (_typeArg2));
    }

    [Test]
    public void Properties ()
    {
      var genericProperty = _genericTypeDefinition.GetProperties().Single();
      var genericGetter = genericProperty.GetGetMethod (true);
      var genericSetter = genericProperty.GetSetMethod (true);
      var property = _instantiation.GetProperties().Single();
      var getter = property.GetGetMethod (true);
      var setter = property.GetSetMethod (true);

      Assert.That (property, Is.TypeOf<PropertyOnTypeInstantiation>());
      Assert.That (property.As<PropertyOnTypeInstantiation>().PropertyOnGenericType, Is.EqualTo (genericProperty));
      Assert.That (property.PropertyType, Is.SameAs (_typeArg1));

      Assert.That (getter, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (getter.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (genericGetter));
      Assert.That (setter, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (setter.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (genericSetter));
    }

    [Test]
    public void Events ()
    {
      var genericEvt = _genericTypeDefinition.GetEvents().Single();
      var genericAdder = genericEvt.GetAddMethod (true);
      var genericRemover = genericEvt.GetRemoveMethod (true);
      Assert.That (genericEvt.GetRaiseMethod (true), Is.Null);
      var evt = _instantiation.GetEvents().Single();
      var adder = evt.GetAddMethod (true);
      var remover = evt.GetRemoveMethod (true);

      Assert.That (evt, Is.TypeOf<EventOnTypeInstantiation>());
      Assert.That (evt.As<EventOnTypeInstantiation>().EventOnGenericType, Is.EqualTo (genericEvt));
      var evtType = evt.EventHandlerType;
      Assert.That (evtType.GetGenericTypeDefinition(), Is.SameAs (typeof (Func<>)));
      Assert.That (evtType.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1 }));

      Assert.That (adder, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (adder.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (genericAdder));
      Assert.That (remover, Is.TypeOf<MethodOnTypeInstantiation>());
      Assert.That (remover.As<MethodOnTypeInstantiation>().MethodOnGenericType, Is.EqualTo (genericRemover));
    }

    // TODO 5816: Nested generic types

    [Test]
    public void GenericType_WrappedInSignature_TypeInstantiation ()
    {
      var enumerable = _instantiation.GetField ("SubstituteTypeInstantiation").FieldType;
      Assert.That (enumerable, Is.TypeOf<TypeInstantiation>());
      Assert.That (enumerable.Name, Is.EqualTo ("IEnumerable`1"));
      var func = enumerable.GetGenericArguments().Single();
      Assert.That (func.Name, Is.EqualTo ("Func`2"));
      var typeArgs = func.GetGenericArguments();
      Assert.That (typeArgs, Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void GenericType_WrappedInSignature_RuntimeType ()
    {
      var enumerable = _instantiation.GetField ("SubstituteRuntimeType").FieldType;
      Assert.That (enumerable.IsRuntimeType(), Is.True);
      Assert.That (enumerable.Name, Is.EqualTo ("IEnumerable`1"));
      var func = enumerable.GetGenericArguments().Single();
      Assert.That (func.Name, Is.EqualTo ("Func`2"));
      var typeArgs = func.GetGenericArguments();
      Assert.That (typeArgs, Is.EqualTo (new[] { typeof (int), typeof (string) }));
    }

    [Test]
    public void RecursiveTypeInstantiation ()
    {
      // This would cause a stack overflow if we wouldn't use an instantiation context.
      var result = _instantiation.GetField ("RecursiveGenericType").FieldType;
      Assert.That (result, Is.SameAs (_instantiation));
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

#pragma warning disable 169 // Unsued fields
      public IEnumerable<Func<T1, T2>> SubstituteTypeInstantiation;
      public IEnumerable<Func<int, string>> SubstituteRuntimeType;
      public GenericType<T1, T2> RecursiveGenericType;
#pragma warning restore 169
    }
  }
}