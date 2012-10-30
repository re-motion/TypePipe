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
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ReflectionEmitObjectMother
  {
    public static TypeBuilder GetSomeTypeBuilder ()
    {
      return (TypeBuilder) FormatterServices.GetUninitializedObject (typeof (TypeBuilder));
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
      return (LocalBuilder) FormatterServices.GetUninitializedObject (typeof (LocalBuilder));
    }

    public static Type GetSomeTypeBuilderInstantiation ()
    {
      var type = typeof (UnspecifiedType<>).MakeGenericType (MutableTypeObjectMother.CreateForExistingType());
      Assertion.IsTrue (type.GetType().FullName == "System.Reflection.Emit.TypeBuilderInstantiation");
      return type;
    }

    private class UnspecifiedType<T> { }
  }
}