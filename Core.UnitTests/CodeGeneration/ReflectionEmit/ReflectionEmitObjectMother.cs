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
using System.Reflection;
using System.Reflection.Emit;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#else
using System.Runtime.CompilerServices;
#endif
using System.Threading;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  public static class ReflectionEmitObjectMother
  {
    private static readonly Lazy<Type> c_typeBuilderType = new Lazy<Type> (GetTypeBuilderType, LazyThreadSafetyMode.None);
    private static readonly Lazy<Type> c_localBuilderType = new Lazy<Type> (GetLocalBuilderType, LazyThreadSafetyMode.None);

    private static Type GetTypeBuilderType ()
    {
#if NETFRAMEWORK || NET6_0 || NET7_0
      return typeof(TypeBuilder);
#else
      return Type.GetType ("System.Reflection.Emit.TypeBuilderImpl, System.Reflection.Emit", throwOnError: true, ignoreCase: false);
#endif
    }

    private static Type GetLocalBuilderType ()
    {
#if NET9_0_OR_GREATER
      return Type.GetType ("System.Reflection.Emit.LocalBuilderImpl, System.Reflection.Emit", throwOnError: true, ignoreCase: false);
#else
      return typeof(LocalBuilder);
#endif
    }

    public static ModuleBuilder CreateModuleBuilder ()
    {
      var assemblyName = "test";
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");

      return moduleBuilder;
    }

    public static TypeBuilder CreateTypeBuilder ()
    {
      return CreateModuleBuilder().DefineType ("myType");
    }

    public static MethodBuilder CreateMethodBuilder ()
    {
      return CreateTypeBuilder().DefineMethod ("method", MethodAttributes.Public);
    }

    public static TypeBuilder GetSomeTypeBuilder ()
    {
#if NETFRAMEWORK
      return (TypeBuilder) FormatterServices.GetUninitializedObject (c_typeBuilderType.Value);
#else
      return (TypeBuilder) RuntimeHelpers.GetUninitializedObject (c_typeBuilderType.Value);
#endif
    }

    public static GenericTypeParameterBuilder GetSomeGenericTypeParameterBuilder ()
    {
#if NETFRAMEWORK
      return (GenericTypeParameterBuilder) FormatterServices.GetUninitializedObject (typeof (GenericTypeParameterBuilder));
#else
      return (GenericTypeParameterBuilder) RuntimeHelpers.GetUninitializedObject (typeof (GenericTypeParameterBuilder));
#endif
    }

    public static FieldBuilder GetSomeFieldBuilder ()
    {
#if NETFRAMEWORK
      return (FieldBuilder) FormatterServices.GetUninitializedObject (typeof (FieldBuilder));
#else
      return (FieldBuilder) RuntimeHelpers.GetUninitializedObject (typeof (FieldBuilder));
#endif
    }

    public static ConstructorBuilder GetSomeConstructorBuilder ()
    {
#if NETFRAMEWORK
      return (ConstructorBuilder) FormatterServices.GetUninitializedObject (typeof (ConstructorBuilder));
#else
      return (ConstructorBuilder) RuntimeHelpers.GetUninitializedObject (typeof (ConstructorBuilder));
#endif
    }

    public static MethodBuilder GetSomeMethodBuilder ()
    {
#if NETFRAMEWORK
      return (MethodBuilder) FormatterServices.GetUninitializedObject (typeof (MethodBuilder));
#else
      return (MethodBuilder) RuntimeHelpers.GetUninitializedObject (typeof (MethodBuilder));
#endif
    }

    public static PropertyBuilder GetSomePropertyBuilder ()
    {
#if NETFRAMEWORK
      return (PropertyBuilder) FormatterServices.GetUninitializedObject (typeof (PropertyBuilder));
#else
      return (PropertyBuilder) RuntimeHelpers.GetUninitializedObject (typeof (PropertyBuilder));
#endif
    }

    public static EventBuilder GetSomeEventBuilder ()
    {
#if NETFRAMEWORK
      return (EventBuilder) FormatterServices.GetUninitializedObject (typeof (EventBuilder));
#else
      return (EventBuilder) RuntimeHelpers.GetUninitializedObject (typeof (EventBuilder));
#endif
    }

    public static LocalBuilder GetSomeLocalBuilder ()
    {
#if NETFRAMEWORK
      return (LocalBuilder) FormatterServices.GetUninitializedObject (c_localBuilderType.Value);
#else
      return (LocalBuilder) RuntimeHelpers.GetUninitializedObject (c_localBuilderType.Value);
#endif
    }

    public static Type GetSomeTypeBuilderInstantiation ()
    {
      var type = typeof (UnspecifiedType<>).MakeGenericType (MutableTypeObjectMother.Create (baseType: null, memberSelector: null));
      Assertion.IsTrue (type.GetType().FullName == "System.Reflection.Emit.TypeBuilderInstantiation");
      return type;
    }

    private class UnspecifiedType<T> { }
  }
}