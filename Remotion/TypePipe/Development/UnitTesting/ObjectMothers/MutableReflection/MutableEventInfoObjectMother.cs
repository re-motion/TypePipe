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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection
{
  public static class MutableEventInfoObjectMother
  {
    public static MutableEventInfo Create (
        MutableType declaringType = null,
        string name = "UnspecifiedEvent",
        EventAttributes attributes = EventAttributes.None,
        MutableMethodInfo addMethod = null,
        MutableMethodInfo removeMethod = null,
        MutableMethodInfo raiseMethod = null)
    {
      Assertion.IsTrue (addMethod != null && removeMethod != null);
      declaringType = declaringType ?? MutableTypeObjectMother.Create();

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }

    public static MutableEventInfo CreateWithAccessors (
        MutableType declaringType = null,
        string name = "UnspecifiedEvent",
        EventAttributes attributes = EventAttributes.None,
        Type handlerType = null,
        bool createRaiseMethod = false)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      handlerType = handlerType ?? typeof (Func<,>).MakeGenericType (ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeOtherType());
      Assertion.IsTrue (handlerType.IsSubclassOf (typeof (Delegate)));

      var invokeMethod = handlerType.GetMethod ("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var raiseParameterTypes = invokeMethod.GetParameters().Select (p => p.ParameterType).ToArray();
      var addRemoveParameterTypes = new[] { handlerType };

      var addMethod = CreateMethod (declaringType, "Adder", addRemoveParameterTypes);
      var removeMethod = CreateMethod (declaringType, "Remover", addRemoveParameterTypes);
      var raiseMethod = createRaiseMethod ? CreateMethod (declaringType, "Raiser", raiseParameterTypes, invokeMethod.ReturnType) : null;

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }

    private static MutableMethodInfo CreateMethod (MutableType declaringType, string name, Type[] parameterTypes, Type returnType = null)
    {
      var parameters = parameterTypes.Select ((t, i) => ParameterDeclarationObjectMother.Create (t, i.ToString (CultureInfo.InvariantCulture)));
      return MutableMethodInfoObjectMother.Create (declaringType, name, returnType: returnType, parameters: parameters);
    }
  }
}