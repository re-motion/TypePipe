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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a parameter.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableParameterInfo"/> to represent the original parameter, before any mutations.
  /// </remarks>
  public class UnderlyingParameterInfoDescriptor
  {
    private readonly ParameterInfo _underlyingSystemParameterInfo;
    private readonly Type _type;
    private readonly string _name;
    private readonly ParameterAttributes _attributes;

    public static UnderlyingParameterInfoDescriptor Create (ParameterInfo originalParameter)
    {
      ArgumentUtility.CheckNotNull ("originalParameter", originalParameter);
      
      return new UnderlyingParameterInfoDescriptor (
          originalParameter, originalParameter.ParameterType, originalParameter.Name, originalParameter.Attributes);
    }

    public static UnderlyingParameterInfoDescriptor Create (Type parameterType, string parameterName, ParameterAttributes patameterAttributes)
    {
      ArgumentUtility.CheckNotNull ("parameterType", parameterType);
      ArgumentUtility.CheckNotNullOrEmpty ("parameterName", parameterName);

      return new UnderlyingParameterInfoDescriptor (null, parameterType, parameterName, patameterAttributes);
    }

    private UnderlyingParameterInfoDescriptor (ParameterInfo underlyingSystemParameterInfo, Type type, string name, ParameterAttributes attributes)
    {
      Assertion.IsNotNull (type, "type");
      Assertion.IsNotNull (name, "name");

      _underlyingSystemParameterInfo = underlyingSystemParameterInfo;
      _type = type;
      _name = name;
      _attributes = attributes;
    }

    public ParameterInfo UnderlyingSystemParameterInfo
    {
      get { return _underlyingSystemParameterInfo; }
    }

    public Type Type
    {
      get { return _type; }
    }

    public string Name
    {
      get { return _name; }
    }

    public ParameterAttributes Attributes
    {
      get { return _attributes; }
    }
  }
}