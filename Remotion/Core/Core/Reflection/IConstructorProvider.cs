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
using System.Reflection;

namespace Remotion.Reflection
{
  /// <summary>
  /// Retrieves constructors of generated types and creates delegates to enable their efficient invocation.
  /// </summary>
  /// <remarks>
  /// The <see cref="GetConstructor"/> method includes the original type and signature to allow building useful exception messages.
  /// </remarks>
  public interface IConstructorProvider
  {
    // TODO 5172: Move this to TypePipe specific IConstructorFinder interface
    ConstructorInfo GetConstructor (
        Type generatedType, Type[] generatedParamterTypes, bool allowNonPublic, Type originalType, Type[] originalParameterTypes);

    // TODO 5172: Move these to interface IConstructorDelegateFactory, add GetSignature method (from ConstructorLookupInfo)

    // TODO 5172: Remove returnType, can be extracted from delegateType
    Delegate CreateConstructorCall (ConstructorInfo constructor, Type delegateType, Type returnType);

    // TODO 5172: Add, use from ConstructorLookupInfo
    // Delegate CreateDefaultConstructorCall (Type constructedType, Type delegateType);
  }
}