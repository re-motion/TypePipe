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
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Creates a <see cref="ProxyType"/> model for the given base type.
  /// </summary>
  public class ProxyTypeModelFactory : IProxyTypeModelFactory
  {
    private int _counter;

    public ProxyType CreateProxyType (Type baseType)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      _counter++;
      var name = string.Format ("{0}_Proxy{1}", baseType.Name, _counter);
      var nameSpace = baseType.Namespace;
      var fullname = string.IsNullOrEmpty (nameSpace) ? name : string.Format ("{0}.{1}", nameSpace, name);
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit | (baseType.IsSerializableFast() ? TypeAttributes.Serializable : 0);

      var proxyType = CreateProxyType (baseType, name, nameSpace, fullname, attributes);
      CopyConstructors (baseType, proxyType);

      return proxyType;
    }

    private ProxyType CreateProxyType (Type baseType, string name, string nameSpace, string fullname, TypeAttributes attributes)
    {
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var interfaceMappingComputer = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (new RelatedMethodFinder());

      return new ProxyType (memberSelector, baseType, name, nameSpace, fullname, attributes, interfaceMappingComputer, mutableMemberFactory);
    }

    private void CopyConstructors (Type baseType, ProxyType proxyType)
    {
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var accessibleInstanceCtors = baseType.GetConstructors (bindingFlags).Where (SubclassFilterUtility.IsVisibleFromSubclass);

      foreach (var ctor in accessibleInstanceCtors)
      {
        var attributes = ctor.Attributes.AdjustVisibilityForAssemblyBoundaries();
        var parameters = ctor.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));

        proxyType.AddConstructor (attributes, parameters, ctx => ctx.CallBaseConstructor (ctx.Parameters.Cast<Expression>()));
      }
    }
  }
}