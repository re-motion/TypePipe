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
  /// Creates <see cref="MutableType"/> instances from the specified data or the given base type.
  /// </summary>
  public class MutableTypeFactory : IMutableTypeFactory
  {
    private const BindingFlags c_allInstanceMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private int _counter;

    public MutableType CreateType (string name, string @namespace, TypeAttributes attributes, Type baseType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      if (attributes.IsSet (TypeAttributes.Interface))
        throw new ArgumentException ("Cannot create interface type. Use CreateInterface instead.", "attributes");

      // TODO 4744: check that baseType.IsVisible
      if (CanNotBeSubclassed (baseType))
      {
        throw new ArgumentException (
            "Base type must not be sealed, an interface, an array, a byref type, a pointer, "
            + "a generic parameter, contain generic parameters and must have an accessible constructor.",
            "baseType");
      }

      return CreateMutableType (name, @namespace, attributes, baseType);
    }

    public MutableType CreateInterface (string name, string @namespace)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Name space may be null.

      var attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
      return CreateMutableType (name, @namespace, attributes, null);
    }

    public MutableType CreateProxy (Type baseType)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);

      _counter++;
      var name = string.Format ("{0}_Proxy{1}", baseType.Name, _counter);
      var attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit | (baseType.IsTypePipeSerializable() ? TypeAttributes.Serializable : 0);

      var proxyType = CreateType (name, baseType.Namespace, attributes, baseType);
      CopyConstructors (baseType, proxyType);

      return proxyType;
    }

    private static MutableType CreateMutableType (string name, string @namespace, TypeAttributes attributes, Type baseType)
    {
      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var interfaceMappingComputer = new InterfaceMappingComputer();
      var mutableMemberFactory = new MutableMemberFactory (new RelatedMethodFinder());

      return new MutableType (memberSelector, baseType, name, @namespace, attributes, interfaceMappingComputer, mutableMemberFactory);
    }

    private void CopyConstructors (Type baseType, MutableType proxyType)
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

    private static bool CanNotBeSubclassed (Type type)
    {
      return type.IsSealed
             || type.IsInterface
             || type.ContainsGenericParameters
             || !HasAccessibleConstructor (type);
    }

    private static bool HasAccessibleConstructor (Type type)
    {
      return type.GetConstructors (c_allInstanceMembers).Where (SubclassFilterUtility.IsVisibleFromSubclass).Any ();
    }
  }
}