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
using System.Reflection;
using Remotion.Text;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Builds signature strings which can be used for debug messages.
  /// </summary>
  public static class SignatureDebugStringGenerator
  {
    public static string GetTypeSignature (Type type)
    {
      return GetShortTypeName (type);
    }

    public static string GetFieldSignature (FieldInfo field)
    {
      return GetShortTypeName (field.FieldType) + " " + field.Name;
    }

    public static string GetConstructorSignature (ConstructorInfo constructor)
    {
      return GetSignatureString (typeof (void), constructor.Name, Type.EmptyTypes, constructor.GetParameters());
    }

    public static string GetMethodSignature (MethodInfo method)
    {
      return GetSignatureString (method.ReturnType, method.Name, method.GetGenericArguments(), method.GetParameters());
    }

    public static string GetParameterSignature (ParameterInfo parameter)
    {
      return GetShortTypeName (parameter.ParameterType) + " " + parameter.Name;
    }

    public static string GetPropertySignature (PropertyInfo property)
    {
      return GetShortTypeName (property.PropertyType) + " " + property.Name;
    }

    public static string GetEventSignature (EventInfo event_)
    {
      return GetShortTypeName (event_.EventHandlerType) + " " + event_.Name;
    }

    private static string GetShortTypeName (Type type)
    {
      return type.Name + GetGenericArgumentSignature (type.GetGenericArguments());
    }

    private static string GetSignatureString (Type returnType, string name, Type[] genericArguments, ParameterInfo[] parameters)
    {
      var returnTypeName = GetShortTypeName (returnType);
      var genericArgumentSignature = GetGenericArgumentSignature (genericArguments);
      var parameterTypeNames = string.Join (", ", parameters.Select (p => p.ParameterType).Select (GetShortTypeName));

      return string.Format ("{0} {1}{2}({3})", returnTypeName, name, genericArgumentSignature, parameterTypeNames);
    }

    private static string GetGenericArgumentSignature (Type[] genericArguments)
    {
      if (genericArguments.Length == 0)
        return string.Empty;

      return "[" + string.Join (",", genericArguments.Select (GetShortTypeName)) + "]";
    }
  }
}