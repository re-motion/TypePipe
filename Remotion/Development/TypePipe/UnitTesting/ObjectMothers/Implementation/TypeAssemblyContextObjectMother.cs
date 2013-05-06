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
using System.Collections.Generic;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.Development.TypePipe.UnitTesting.ObjectMothers.Implementation
{
  public static class TypeAssemblyContextObjectMother
  {
    public static ProxyTypeAssemblyContext Create (
        Type requestedType = null,
        MutableType proxyType = null,
        IMutableTypeFactory mutableTypeFactory = null,
        IDictionary<string, object> state = null)
    {
      requestedType = requestedType ?? typeof (UnspecifiedRequestedType);
      proxyType = proxyType ?? MutableTypeObjectMother.Create (requestedType);
      mutableTypeFactory = mutableTypeFactory ?? new MutableTypeFactory();
      state = state ?? new Dictionary<string, object>();

      return new ProxyTypeAssemblyContext (requestedType, proxyType, mutableTypeFactory, state);
    }

    public class UnspecifiedRequestedType {}
  }
}