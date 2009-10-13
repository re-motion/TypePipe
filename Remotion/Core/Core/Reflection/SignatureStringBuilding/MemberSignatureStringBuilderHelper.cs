using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Reflection.SignatureStringBuilding
{
  /// <summary>
  /// Provides a common utility class for classes building strings representing the signature of a given <see cref="MemberInfo"/> object.
  /// </summary>
  /// <remarks>
  /// <para>
  /// For simplicity, this class assumes that type or namespace names cannot contain the character "[". It also assumes that the full name of a type 
  /// (namespace, enclosing type (if any), and simple type name) is enough to identify a type - assembly information is not encoded. The 1:1 mapping 
  /// of signature strings to member signatures is only guaranteed for members that adhere to these assumptions.
  /// </para>
  /// </remarks>
  public class MemberSignatureStringBuilderHelper
  {
    public void AppendTypeString (StringBuilder sb, Type type)
    {
      ArgumentUtility.CheckNotNull ("sb", sb);
      ArgumentUtility.CheckNotNull ("type", type);

      if (type.IsGenericParameter)
      {
        if (type.DeclaringMethod != null)
        {
          sb.Append ("[").Append (type.GenericParameterPosition).Append ("]");
        }
        else
        {
          sb.Append ("[").Append (type.GenericParameterPosition);
          sb.Append ("/");
          AppendTypeString (sb, type.DeclaringType);
          sb.Append ("]");
        }
      }
      else if (type.IsGenericTypeDefinition)
      {
        sb.Append (type.FullName); // Namespace.Type`Count
      }
      else if (type.IsGenericType)
      {
        AppendTypeString (sb, type.GetGenericTypeDefinition ());
        sb.Append ("[");
        AppendSeparatedTypeStrings (sb, type.GetGenericArguments ());
        sb.Append ("]");
      }
      else
      {
        sb.Append (type.FullName);
      }
    }

    public void AppendSeparatedTypeStrings (StringBuilder sb, IEnumerable<Type> types)
    {
      ArgumentUtility.CheckNotNull ("sb", sb);
      ArgumentUtility.CheckNotNull ("types", types);

      bool first = true;
      foreach (var type in types)
      {
        if (!first)
          sb.Append (",");
        AppendTypeString (sb, type);
        first = false;
      }
    }
  }
}