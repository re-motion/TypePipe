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
using System.Dynamic.Utils;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of an existing constructor.
  /// </summary>
  public class ExistingConstructorInfoStrategy : IUnderlyingConstructorInfoStrategy
  {
    private readonly ConstructorInfo _originalConstructorInfo;

    public ExistingConstructorInfoStrategy (ConstructorInfo originalConstructorInfo)
    {
      ArgumentUtility.CheckNotNull ("originalConstructorInfo", originalConstructorInfo);
      _originalConstructorInfo = originalConstructorInfo;
    }

    public ConstructorInfo GetUnderlyingSystemConstructorInfo ()
    {
      return _originalConstructorInfo;
    }

    public MethodAttributes GetAttributes ()
    {
      return _originalConstructorInfo.Attributes;
    }

    public IEnumerable<ParameterDeclaration> GetParameterDeclarations ()
    {
      return _originalConstructorInfo.GetParameters().Select (pi => new ParameterDeclaration (pi.ParameterType, pi.Name, pi.Attributes));
    }
  }
}