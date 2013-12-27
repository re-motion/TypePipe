// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection
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