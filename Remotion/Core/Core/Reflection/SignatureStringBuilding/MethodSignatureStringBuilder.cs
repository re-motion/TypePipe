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
using System.Text;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Reflection.SignatureStringBuilding
{
  /// <summary>
  /// Builds a string representing the signature of a given <see cref="MethodInfo"/> object. This is similar to the string returned by 
  /// <see cref="object.ToString"/> as it contains a textual representation of the method's return type, parameter types (without parameter names),
  /// and generic parameter types. It's different from <see cref="object.ToString"/>, though, because it does not contain the method's name or 
  /// generic parameter names.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The signature string does currently not hold custom modifiers.
  /// </para>
  /// <para>
  /// For simplicity, this class assumes that type or namespace names cannot contain the character "[". It encloses the parameter list with "(" 
  /// and ")" and  separates the parameters by ",". It also assumes that the full name of a type (namespace, enclosing type (if any), and simple 
  /// type name) is enough to identify a type - assembly information is not encoded. The 1:1 mapping of signature strings to method signatures is
  /// only guaranteed for methods that adhere to these assumptions.
  /// </para>
  /// <para>
  /// This class does not support closed generic methods, only generic method definitions are supported.
  /// </para>
  /// </remarks>
  public class MethodSignatureStringBuilder : IMemberSignatureStringBuilder
  {
    private readonly MemberSignatureStringBuilderHelper _helper = new MemberSignatureStringBuilderHelper ();

    public string BuildSignatureString (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);
      if (methodBase.IsGenericMethod && !methodBase.IsGenericMethodDefinition)
        throw new ArgumentException ("Closed generic methods are not supported.", "methodBase");

      var sb = new StringBuilder ();

      if (methodBase is MethodInfo)
        _helper.AppendTypeString (sb, ((MethodInfo) methodBase).ReturnType);
      else
      {
        Assertion.IsTrue (methodBase is ConstructorInfo);
        _helper.AppendTypeString (sb, typeof (void));
      }

      sb.Append ("(");
      _helper.AppendSeparatedTypeStrings (sb, methodBase.GetParameters ().Select (p => p.ParameterType));
      sb.Append (")");
      
      if (methodBase.IsGenericMethod)
        sb.Append ("`").Append (methodBase.GetGenericArguments ().Length);

      return sb.ToString ();
    }

    string IMemberSignatureStringBuilder.BuildSignatureString (MemberInfo memberInfo)
    {
      return BuildSignatureString ((MethodBase) memberInfo);
    }
  }
}
