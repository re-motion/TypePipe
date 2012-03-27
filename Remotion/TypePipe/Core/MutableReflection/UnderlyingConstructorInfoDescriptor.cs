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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of a constructor.
  /// </summary>
  /// <remarks>
  /// This is used by <see cref="MutableConstructorInfo"/> to represent the original constructor, before any mutations.
  /// </remarks>
  public class UnderlyingConstructorInfoDescriptor
  {
    public static UnderlyingConstructorInfoDescriptor Create (
        MethodAttributes attributes, 
        IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);

      return new UnderlyingConstructorInfoDescriptor (null, attributes, parameterDeclarations);
    }

    public static UnderlyingConstructorInfoDescriptor Create (ConstructorInfo originalConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("originalConstructorInfo", originalConstructorInfo);

      var attributes = originalConstructorInfo.Attributes;
      var parameterDeclarations =
          originalConstructorInfo.GetParameters ().Select (pi => new ParameterDeclaration (pi.ParameterType, pi.Name, pi.Attributes));
      return new UnderlyingConstructorInfoDescriptor (originalConstructorInfo, attributes, parameterDeclarations);
    }
    
    private readonly ConstructorInfo _underlyingSystemConstructorInfo;
    private readonly MethodAttributes _attributes;
    private readonly IEnumerable<ParameterDeclaration> _parameterDeclarations;

    private UnderlyingConstructorInfoDescriptor (
        ConstructorInfo underlyingSystemConstructorInfo, 
        MethodAttributes attributes, 
        IEnumerable<ParameterDeclaration> parameterDeclarations)
    {
      _underlyingSystemConstructorInfo = underlyingSystemConstructorInfo;
      _attributes = attributes;
      _parameterDeclarations = parameterDeclarations;
    }

    public ConstructorInfo UnderlyingSystemConstructorInfo
    {
      get { return _underlyingSystemConstructorInfo; }
    }

    public MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public IEnumerable<ParameterDeclaration> ParameterDeclarations
    {
      get { return _parameterDeclarations; }
    }
 }
}