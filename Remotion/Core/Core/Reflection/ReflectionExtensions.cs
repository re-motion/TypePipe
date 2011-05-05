// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Utilities;

namespace Remotion.Reflection
{
  /// <summary>
  /// Extension methods for <see cref="IMemberInformation"/>.
  /// </summary>
  public static class ReflectionExtensions
  {
    /// <summary>
    /// Evaluates whether the <see cref="IMemberInformation"/> instance represents to orignal declaration of the member in the type hierarchy.
    /// </summary>
    /// <returns>
    ///   <see langword="true" /> if <see cref="IMemberInformation.DeclaringType"/> and <see cref="IMemberInformation.GetOriginalDeclaringType"/> are equal.
    /// </returns>
    public static bool IsOriginalDeclaration (this IMemberInformation memberInfo)
    {
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var declaringType = memberInfo.DeclaringType;
      var originalDeclaringType = memberInfo.GetOriginalDeclaringType ();
      if (declaringType == null && originalDeclaringType == null)
        return true;
      if (declaringType == null)
        return false;
      if (originalDeclaringType == null)
        return false;
      return memberInfo.DeclaringType.Equals (originalDeclaringType);
    }
  }
}