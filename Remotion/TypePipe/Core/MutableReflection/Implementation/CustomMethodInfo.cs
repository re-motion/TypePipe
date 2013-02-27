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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom <see cref="MethodInfo"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the 
  /// implementation. Note that the equality members <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomMethodInfo : MethodInfo, ICustomAttributeDataProvider
  {
    private readonly Type _declaringType;
    private readonly string _name;
    private readonly MethodAttributes _attributes;
    private readonly bool _isGenericMethod;
    private readonly MethodInfo _genericMethodDefinition;
    private readonly ReadOnlyCollection<Type> _typeArguments;

    protected CustomMethodInfo (
        Type declaringType,
        string name,
        MethodAttributes attributes,
        bool isGenericMethod,
        MethodInfo genericMethodDefinition,
        IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Generic method definition may be null (for non-generic methods and generic method definitions).
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);
      //Assertion.IsTrue (typeArguments == null || (typeArguments != null && genericMethodDefinition != null));

      _declaringType = declaringType;
      _name = name;
      _attributes = attributes;
      _isGenericMethod = isGenericMethod;
      _genericMethodDefinition = genericMethodDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();
    }

    public abstract override ParameterInfo ReturnParameter { get; }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();
    public abstract override ParameterInfo[] GetParameters ();

    public abstract override MethodInfo GetBaseDefinition ();

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override MethodAttributes Attributes
    {
      get { return _attributes; }
    }

    public override CallingConventions CallingConvention
    {
      get
      {
        if (IsStatic)
          return CallingConventions.Standard;
        else
          return CallingConventions.Standard | CallingConventions.HasThis;
      }
    }

    public override Type ReturnType
    {
      get { return Assertion.IsNotNull (ReturnParameter).ParameterType; }
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes
    {
      get { return Assertion.IsNotNull (ReturnParameter); }
    }

    public override bool IsGenericMethod
    {
      get { return _isGenericMethod; }
    }

    public override bool IsGenericMethodDefinition
    {
      get { return _isGenericMethod && _genericMethodDefinition == null; }
    }

    public override bool ContainsGenericParameters
    {
      get { return _typeArguments.Any (g => g.IsGenericParameter); }
    }

    public override MethodInfo GetGenericMethodDefinition ()
    {
      if (!_isGenericMethod)
        throw new InvalidOperationException ("GetGenericMethodDefinition can only be called on generic methods (IsGenericMethod must be true).");

      return _genericMethodDefinition ?? this;
    }

    public override Type[] GetGenericArguments ()
    {
      return _typeArguments.ToArray();
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (bool inherit)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return CustomAttributeFinder.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.GetCustomAttributes (this, attributeType, inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.IsDefined (this, attributeType, inherit);
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetMethodSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\", DeclaringType = \"{2}\"", GetType().Name.Replace ("Info", ""), ToString(), DeclaringType);
    }

    #region Not YET Implemented from MethodInfo interface

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      throw new NotImplementedException ();
    }

    #endregion

    #region Unsupported Members

    public override Type ReflectedType
    {
      get { throw new NotSupportedException ("Property ReflectedType is not supported."); }
    }

    public override int MetadataToken
    {
      get { throw new NotSupportedException ("Property MetadataToken is not supported."); }
    }

    public override Module Module
    {
      get { throw new NotSupportedException ("Property Module is not supported."); }
    }

    public override RuntimeMethodHandle MethodHandle
    {
      get { throw new NotSupportedException ("Property MethodHandle is not supported."); }
    }

    public override MethodInfo MakeGenericMethod (params Type[] typeArguments)
    {
      throw new NotSupportedException ("Method MakeGenericMethod is not supported.");
    }

    public override MethodBody GetMethodBody ()
    {
      throw new NotSupportedException ("Method GetMethodBody is not supported.");
    }

    public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotSupportedException ("Method Invoke is not supported.");
    }

    #endregion
  }
}