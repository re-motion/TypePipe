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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.Caching;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Creates a <see cref="ProxyType"/> model for the given base type.
  /// </summary>
  /// <remarks>
  /// This class is used behind the <see cref="TypeCache"/>, therefore the incrementation of the <see cref="_counter"/> field does not need to be
  /// guarded.
  /// </remarks>
  public class ProxyTypeModelFactory : IProxyTypeModelFactory
  {
    private int _counter;

    public ProxyType CreateProxyType (Type baseType)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var relatedMethodFinder = new RelatedMethodFinder();
      var interfaceMappingComputer = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (memberSelector, relatedMethodFinder);

      _counter++;
      var name = string.Format ("{0}_Proxy{1}", baseType.Name, _counter);
      var fullname = string.IsNullOrEmpty (baseType.Namespace) ? name : string.Format ("{0}.{1}", baseType.Namespace, name);
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit | (baseType.IsSerializable ? TypeAttributes.Serializable : 0);

      var proxyType = new ProxyType (
          memberSelector, baseType, name, baseType.Namespace, fullname, attributes, interfaceMappingComputer, mutableMemberFactory);

      CopyConstructors (baseType, proxyType);

      return proxyType;
    }

    private void CopyConstructors (Type baseType, ProxyType proxyType)
    {
      // TODO yyy: out and ref parameters?!

      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var accessibleInstanceCtors = baseType.GetConstructors (bindingFlags).Where (SubclassFilterUtility.IsVisibleFromSubclass);
      foreach (var ctor in accessibleInstanceCtors)
      {
        proxyType.AddConstructor (
            ctor.Attributes.AdjustVisibilityForAssemblyBoundaries(),
            ParameterDeclaration.CreateForEquivalentSignature (ctor),
            ctx => ctx.CallBaseConstructor (ctx.Parameters.Cast<Expression>()));
      }
    }
  }
}