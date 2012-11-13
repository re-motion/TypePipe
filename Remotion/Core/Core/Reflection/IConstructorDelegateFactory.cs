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
using Remotion.Collections;

namespace Remotion.Reflection
{
  /// <summary>
  /// Extracts constructor signatures from delegate types and delegates for their efficient invocation.
  /// </summary>
  public interface IConstructorDelegateFactory
  {
    Tuple<Type[], Type> GetSignature (Type delegateType);

    Delegate CreateConstructorCall (ConstructorInfo constructor, Type delegateType);

    Delegate CreateDefaultConstructorCall (Type constructedType, Type delegateType);
  }
}