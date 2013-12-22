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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A factory for creating <see cref="MutablePropertyInfo"/> instances.
  /// </summary>
  public class PropertyFactory
  {
    private readonly IMethodFactory _methodFactory;

    public PropertyFactory (IMethodFactory methodFactory)
    {
      ArgumentUtility.CheckNotNull ("methodFactory", methodFactory);

      _methodFactory = methodFactory;
    }

    public MutablePropertyInfo CreateProperty (
        MutableType declaringType,
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("indexParameters", indexParameters);
      // Get body provider may be null.
      // Set body provider may be null.

      MemberAttributesUtility.ValidateAttributes (
          "property accessor methods", MemberAttributesUtility.InvalidMethodAttributes, accessorAttributes, "accessorAttributes");

      if (getBodyProvider == null && setBodyProvider == null)
        throw new ArgumentException ("At least one accessor body provider must be specified.", "getBodyProvider");

      var indexParams = indexParameters.ToList();
      var signature = new PropertySignature (type, indexParams.Select (pd => pd.Type));
      if (declaringType.AddedProperties.Any (p => p.Name == name && PropertySignature.Create (p).Equals (signature)))
        throw new InvalidOperationException ("Property with equal name and signature already exists.");

      var attributes = accessorAttributes | MethodAttributes.SpecialName;
      MutableMethodInfo getMethod = null, setMethod = null;
      if (getBodyProvider != null)
        getMethod = CreateAccessor (declaringType, "get_" + name, attributes, type, indexParams, getBodyProvider);
      if (setBodyProvider != null)
      {
        var setterParams = indexParams.Concat (new[] { new ParameterDeclaration (type, "value") });
        setMethod = CreateAccessor (declaringType, "set_" + name, attributes, typeof (void), setterParams, setBodyProvider);
      }

      return new MutablePropertyInfo (declaringType, name, PropertyAttributes.None, getMethod, setMethod);
    }

    public MutablePropertyInfo CreateProperty (
        MutableType declaringType, string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Get method may be null.
      // Set method may be null.

      MemberAttributesUtility.ValidateAttributes ("properties", MemberAttributesUtility.InvalidPropertyAttributes, attributes, "attributes");

      if (getMethod == null && setMethod == null)
        throw new ArgumentException ("Property must have at least one accessor.", "getMethod");

      var readWriteProperty = getMethod != null && setMethod != null;
      if (readWriteProperty && getMethod.IsStatic != setMethod.IsStatic)
        throw new ArgumentException ("Accessor methods must be both either static or non-static.", "getMethod");

      if (getMethod != null && !ReferenceEquals (getMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Get method is not declared on the current type.", "getMethod");
      if (setMethod != null && !ReferenceEquals (setMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Set method is not declared on the current type.", "setMethod");

      if (getMethod != null && getMethod.ReturnType == typeof (void))
        throw new ArgumentException ("Get accessor must be a non-void method.", "getMethod");
      if (setMethod != null && setMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Set accessor must have return type void.", "setMethod");

      var getSignature = getMethod != null ? new PropertySignature (getMethod.ReturnType, getMethod.GetParameters ().Select (p => p.ParameterType)) : null;
      var setParameters = setMethod != null ? setMethod.GetParameters ().Select (p => p.ParameterType).ToList () : null;
      var setSignature = setMethod != null ? new PropertySignature (setParameters.Last (), setParameters.Take (setParameters.Count - 1)) : null;

      if (readWriteProperty && !getSignature.Equals (setSignature))
        throw new ArgumentException ("Get and set accessor methods must have a matching signature.", "setMethod");

      var signature = getSignature ?? setSignature;
      if (declaringType.AddedProperties.Any (p => p.Name == name && PropertySignature.Create (p).Equals (signature)))
        throw new InvalidOperationException ("Property with equal name and signature already exists.");

      return new MutablePropertyInfo (declaringType, name, attributes, getMethod, setMethod);
    }

    private MutableMethodInfo CreateAccessor (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _methodFactory.CreateMethod (
          declaringType, name, attributes, GenericParameterDeclaration.None, ctx => returnType, ctx => parameters, bodyProvider);
    }
  }
}