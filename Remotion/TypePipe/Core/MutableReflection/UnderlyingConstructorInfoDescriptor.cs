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
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a constructor.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableConstructorInfo"/> to represent a constructor, before any mutations.
  /// </remarks>
  public class UnderlyingConstructorInfoDescriptor : UnderlyingMethodBaseDescriptor<ConstructorInfo>
  {
    public static UnderlyingConstructorInfoDescriptor Create (
        MethodAttributes attributes, IEnumerable<UnderlyingParameterInfoDescriptor> parameterDescriptors, Expression body)
    {
      ArgumentUtility.CheckNotNull ("parameterDescriptors", parameterDescriptors);
      ArgumentUtility.CheckNotNull ("body", body);

      if (body.Type != typeof(void))
        throw new ArgumentException ("Constructor bodies must have void return type.", "body");

      var readonlyParameterDeclarations = parameterDescriptors.ToList().AsReadOnly();

      return new UnderlyingConstructorInfoDescriptor (null, attributes, readonlyParameterDeclarations, EmptyCustomAttributeDataProvider, body);
    }

    public static UnderlyingConstructorInfoDescriptor Create (ConstructorInfo underlyingConstructor)
    {
      ArgumentUtility.CheckNotNull ("underlyingConstructor", underlyingConstructor);

      // TODO 4695
      // If ctor visibility is FamilyOrAssembly, change it to Family because the mutated type will be put into a different assembly.
      var attributes = underlyingConstructor.Attributes.AdjustVisibilityForAssemblyBoundaries();
      var parameterDescriptors = UnderlyingParameterInfoDescriptor.CreateFromMethodBase (underlyingConstructor);
      var customAttributeDataProvider = GetCustomAttributeProvider (underlyingConstructor);
      var body = CreateOriginalBodyExpression (underlyingConstructor, typeof (void), parameterDescriptors);

      return new UnderlyingConstructorInfoDescriptor (underlyingConstructor, attributes, parameterDescriptors, customAttributeDataProvider, body);
    }

    private static string GetConstructorName (MethodAttributes attributes)
    {
      return attributes.IsSet (MethodAttributes.Static) ? ConstructorInfo.TypeConstructorName : ConstructorInfo.ConstructorName;
    }

    private UnderlyingConstructorInfoDescriptor (
        ConstructorInfo underlyingConstructor,
        MethodAttributes attributes,
        ReadOnlyCollection<UnderlyingParameterInfoDescriptor> parameterDescriptors,
        Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider,
        Expression body)
        : base (underlyingConstructor, GetConstructorName (attributes), attributes, parameterDescriptors, customAttributeDataProvider, body)
    {
      Assertion.IsTrue (body.Type == typeof (void));
    }
  }
}