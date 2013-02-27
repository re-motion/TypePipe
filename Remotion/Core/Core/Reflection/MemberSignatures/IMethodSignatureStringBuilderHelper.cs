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
using System.Reflection;
using System.Text;

namespace Remotion.Reflection.MemberSignatures
{
  /// <summary>
  /// Defines an interface for classes building strings representing the signature of a given <see cref="MemberInfo"/> object.
  /// </summary>
  /// <remarks>
  /// This merely defines an interface for <see cref="MethodSignatureStringBuilderHelper"/> to allow for more flexibility
  /// in <see cref="MethodSignature"/>.
  /// </remarks>
  public interface IMethodSignatureStringBuilderHelper
  {
    void AppendTypeString (StringBuilder sb, Type type);
    void AppendSeparatedTypeStrings (StringBuilder sb, IEnumerable<Type> types);
  }
}