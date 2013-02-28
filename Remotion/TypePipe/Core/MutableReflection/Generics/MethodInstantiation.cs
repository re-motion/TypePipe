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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;
using Remotion.Collections;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a constructed generic <see cref="MethodInfo"/>, i.e., a generic method definition that was instantiated with type arguments.
  /// This class is needed because the the original reflection classes do not work in combination with <see cref="CustomType"/> instances.
  /// </summary>
  // TODO <remarks>Instances of this class are returned by <see cref="MutableMethodInfo.MakeGenericMethod"/>.</remarks>
  public class MethodInstantiation : CustomMethodInfo
  {
    private readonly Dictionary<TypeInstantiationInfo, TypeInstantiation> _instantiations = new Dictionary<TypeInstantiationInfo, TypeInstantiation>();

    private readonly ParameterInfo _returnParameter;
    private readonly ReadOnlyCollection<ParameterInfo> _parameters;
    private readonly Dictionary<Type, Type> _parametersToArguments;

    public MethodInstantiation (MethodInstantiationInfo instantiationInfo)
        : base (
            ArgumentUtility.CheckNotNull ("instantiationInfo", instantiationInfo).GenericMethodDefinition.DeclaringType,
            instantiationInfo.GenericMethodDefinition.Name,
            instantiationInfo.GenericMethodDefinition.Attributes,
            true,
            instantiationInfo.GenericMethodDefinition,
            instantiationInfo.TypeArguments)
    {
      var genericMethodDefinition = instantiationInfo.GenericMethodDefinition;
      _parametersToArguments = genericMethodDefinition
          .GetGenericArguments().Zip (instantiationInfo.TypeArguments).ToDictionary (t => t.Item1, t => t.Item2);

      _returnParameter = new MemberParameterOnInstantiation (this, genericMethodDefinition.ReturnParameter);
      _parameters = genericMethodDefinition
          .GetParameters().Select (p => new MemberParameterOnInstantiation (this, p)).Cast<ParameterInfo>().ToList().AsReadOnly();
    }

    public Type SubstituteGenericParameters (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      var typeArgument = _parametersToArguments.GetValueOrDefault (type);
      if (typeArgument != null)
        return typeArgument;

      if (!type.IsGenericType)
        return type;

      Assertion.IsFalse (type.IsArray, "Not yet supported, TODO 5409");

      var oldTypeArguments = type.GetGenericArguments ();
      var newTypeArguments = oldTypeArguments.Select (SubstituteGenericParameters).ToList ();

      // No substitution necessary (this is an optimization only).
      if (oldTypeArguments.SequenceEqual (newTypeArguments))
        return type;

      var genericTypeDefinition = type.GetGenericTypeDefinition ();
      var instantiationInfo = new TypeInstantiationInfo (genericTypeDefinition, newTypeArguments);

      return instantiationInfo.Instantiate (_instantiations);
    }

    public override ParameterInfo ReturnParameter
    {
      get { return _returnParameter; }
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (GetGenericMethodDefinition());
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.ToArray();
    }

    public override MethodInfo GetBaseDefinition ()
    {
      throw new NotImplementedException();
    }
  }
}