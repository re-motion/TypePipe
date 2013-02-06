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
      return GetSignatureString (typeof (void), constructor.Name, constructor.GetParameters());
    }

    public static string GetMethodSignature (MethodInfo method)
    {
      return GetSignatureString (method.ReturnType, method.Name, method.GetParameters());
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
      if (type.IsGenericType)
        return type.Name + "[" + SeparatedStringBuilder.Build (",", type.GetGenericArguments(), GetShortTypeName) + "]";
      return type.Name;
    }

    private static string GetSignatureString (Type returnType, string name, IEnumerable<ParameterInfo> parameters)
    {
      var parameterTypes = SeparatedStringBuilder.Build (", ", parameters.Select (p => GetShortTypeName (p.ParameterType)));
      return GetShortTypeName (returnType) + " " + name + "(" + parameterTypes + ")";
    }
  }
}