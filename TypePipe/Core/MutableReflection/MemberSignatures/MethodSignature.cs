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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.MemberSignatures
{
  /// <summary>
  /// Represents a method signature and allows signatures to be compared to each other.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class does not support closed generic methods, only generic method definitions are supported.
  /// </para>
  /// <para>
  /// The signature currently does not include custom modifiers.
  /// </para>
  /// <para>
  /// For simplicity, this class assumes that type or namespace names cannot contain the character "[". It also assumes that the full name of a type 
  /// (namespace, enclosing type (if any), and simple  type name) is enough to identify a type - assembly information is not used when comparing 
  /// signatures.
  /// </para>
  /// </remarks>
  public class MethodSignature : IMemberSignature, IEquatable<MethodSignature>
  {
    private static readonly IMethodSignatureStringBuilderHelper s_helper = new MethodSignatureStringBuilderHelper();

    public static MethodSignature Create (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);

      if (methodBase.IsGenericMethod && !methodBase.IsGenericMethodDefinition)
        throw new ArgumentException ("Closed generic methods are not supported.", "methodBase");

      var returnType = GetReturnType (methodBase);
      var parameterTypes = methodBase.GetParameters ().Select (p => p.ParameterType);
      var genericParameterCount = methodBase.IsGenericMethod ? methodBase.GetGenericArguments ().Length : 0;
      return new MethodSignature (returnType, parameterTypes, genericParameterCount);
    }

    public static bool AreEqual (MethodBase method1, MethodBase method2)
    {
      ArgumentUtility.CheckNotNull ("method1", method1);
      ArgumentUtility.CheckNotNull ("method2", method2);

      return Create (method1).Equals (Create (method2));
    }

    private static Type GetReturnType (MethodBase methodBase)
    {
      var methodInfo = methodBase as MethodInfo;
      if (methodInfo == null)
      {
        Assertion.IsTrue (methodBase is ConstructorInfo);
        return typeof (void);
      }

      return methodInfo.ReturnType;
    }

    private readonly Type _returnType;
    private readonly IEnumerable<Type> _parameterTypes;
    private readonly int _genericParameterCount;
    private readonly IMethodSignatureStringBuilderHelper _signatureBuilder;

    public MethodSignature (
        Type returnType,
        IEnumerable<Type> parameterTypes,
        int genericParameterCount,
        IMethodSignatureStringBuilderHelper methodSignatureStringBuilderHelper)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);
      ArgumentUtility.CheckNotNull ("methodSignatureStringBuilderHelper", methodSignatureStringBuilderHelper);

      _returnType = returnType;
      _parameterTypes = parameterTypes;
      _genericParameterCount = genericParameterCount;
      _signatureBuilder = methodSignatureStringBuilderHelper;
    }

    public MethodSignature (Type returnType, IEnumerable<Type> parameterTypes, int genericParameterCount)
        : this (returnType, parameterTypes, genericParameterCount, s_helper)
    {
    }

    public Type ReturnType
    {
      get { return _returnType; }
    }

    public IEnumerable<Type> ParameterTypes
    {
      get { return _parameterTypes; }
    }

    public int GenericParameterCount
    {
      get { return _genericParameterCount; }
    }

    public override string ToString ()
    {
      var sb = new StringBuilder ();

      _signatureBuilder.AppendTypeString (sb, ReturnType);

      sb.Append ("(");
      _signatureBuilder.AppendSeparatedTypeStrings (sb, ParameterTypes);
      sb.Append (")");

      if (GenericParameterCount > 0)
        sb.Append ("`").Append (GenericParameterCount);

      return sb.ToString ();
    }

    public virtual bool Equals (MethodSignature other)
    {
      return !ReferenceEquals (other, null) 
          && ToString() == other.ToString();
    }

    public sealed override bool Equals (object obj)
    {
      if (obj == null || obj.GetType () != GetType ())
        return false;

      var other = (MethodSignature) obj;
      return Equals (other);
    }

    bool IEquatable<IMemberSignature>.Equals (IMemberSignature other)
    {
      return Equals (other);
    }

    public override int GetHashCode ()
    {
      return ToString().GetHashCode();
    }
  }
}