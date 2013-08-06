// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 

using System;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.Development.TypePipe.UnitTesting
{
  /// <summary>
  /// Adds a custom attribute to the proxy type. Use this participant to avoid the pipelines no-modification optimization in tests.
  /// </summary>
  public class ModifyingParticipant : SimpleParticipantBase
  {
    [AttributeUsage (AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ModifiedAssembledTypeAttribute : Attribute { }

    public override void Participate (object id, IProxyTypeAssemblyContext proxyTypeAssemblyContext)
    {
      var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new ModifiedAssembledTypeAttribute());
      var attribute = new CustomAttributeDeclaration (constructor, new object[0]);

      proxyTypeAssemblyContext.ProxyType.AddCustomAttribute (attribute);
    }
  }
}