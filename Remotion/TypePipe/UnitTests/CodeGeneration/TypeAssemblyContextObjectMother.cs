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
using System.Runtime.Serialization;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.CodeGeneration
{
  public static class TypeAssemblyContextObjectMother
  {
    public static TypeAssemblyContext Create (Type requestedType = null, IMutableTypeFactory mutableTypeFactory = null, IDictionary<string, object> state = null)
    {
      requestedType = requestedType ?? typeof (UnspecifiedType);
      mutableTypeFactory = mutableTypeFactory ?? new MutableTypeFactory();
      state = state ?? new Dictionary<string, object>();

      return new TypeAssemblyContext (mutableTypeFactory, requestedType, state);
    }

    public static TypeAssemblyContext Create (MutableType proxyType)
    {
      var typeContext = (TypeAssemblyContext) FormatterServices.GetUninitializedObject (typeof (TypeAssemblyContext));
      PrivateInvoke.SetNonPublicField (typeContext, "_proxyType", proxyType);

      return typeContext;
    }

    public class UnspecifiedType {}
  }
}