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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A factory for creating <see cref="MutableConstructorInfo"/> instances.
  /// </summary>
  public class ConstructorFactory
  {
    public MutableConstructorInfo CreateConstructor (
        MutableType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameters,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      MemberAttributesUtility.ValidateAttributes ("constructors", MemberAttributesUtility.InvalidConstructorAttributes, attributes, "attributes");

      var isStatic = attributes.IsSet (MethodAttributes.Static);
      var paras = parameters.ToList();
      if (isStatic && paras.Count != 0)
        throw new ArgumentException ("A type initializer (static constructor) cannot have parameters.", "parameters");

      var signature = new MethodSignature (typeof (void), paras.Select (p => p.Type), 0);
      if (declaringType.AddedConstructors.Any (ctor => ctor.IsStatic == isStatic && MethodSignature.Create (ctor).Equals (signature)))
        throw new InvalidOperationException ("Constructor with equal signature already exists.");

      var parameterExpressions = paras.Select (p => p.Expression);
      var context = new ConstructorBodyCreationContext (declaringType, isStatic, parameterExpressions);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);

      var attr = attributes.Set (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
      return new MutableConstructorInfo (declaringType, attr, paras, body);
    }
  }
}