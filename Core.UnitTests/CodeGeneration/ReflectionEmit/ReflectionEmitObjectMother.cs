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
using System.Runtime.Serialization;
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
      return (TypeBuilder) FormatterServices.GetUninitializedObject (c_typeBuilderType.Value);
    }

    public static GenericTypeParameterBuilder GetSomeGenericTypeParameterBuilder ()
    {
      return (GenericTypeParameterBuilder) FormatterServices.GetUninitializedObject (typeof (GenericTypeParameterBuilder));
    }

    public static FieldBuilder GetSomeFieldBuilder ()
    {
      return (FieldBuilder) FormatterServices.GetUninitializedObject (typeof (FieldBuilder));
    }

    public static ConstructorBuilder GetSomeConstructorBuilder ()
    {
      return (ConstructorBuilder) FormatterServices.GetUninitializedObject (typeof (ConstructorBuilder));
    }

    public static MethodBuilder GetSomeMethodBuilder ()
    {
      return (MethodBuilder) FormatterServices.GetUninitializedObject (typeof (MethodBuilder));
    }

    public static PropertyBuilder GetSomePropertyBuilder ()
    {
      return (PropertyBuilder) FormatterServices.GetUninitializedObject (typeof (PropertyBuilder));
    }

    public static EventBuilder GetSomeEventBuilder ()
    {
      return (EventBuilder) FormatterServices.GetUninitializedObject (typeof (EventBuilder));
    }

    public static LocalBuilder GetSomeLocalBuilder ()
    {
      return (LocalBuilder) FormatterServices.GetUninitializedObject (c_localBuilderType.Value);
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