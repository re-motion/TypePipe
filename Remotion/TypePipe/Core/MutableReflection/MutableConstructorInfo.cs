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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.ReflectionEmit;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="ConstructorInfo"/> that can be modified.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableConstructorInfo : ConstructorInfo, IMutableMethodBase
  {
    private readonly MutableType _declaringType;
    private readonly UnderlyingConstructorInfoDescriptor _underlyingConstructorInfoDescriptor;
    private readonly ReadOnlyCollection<MutableParameterInfo> _parameters;
    // TODO 5057 (Use Lazy<T>)
    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDatas;

    private Expression _body;

    public MutableConstructorInfo (MutableType declaringType, UnderlyingConstructorInfoDescriptor underlyingConstructorInfoDescriptor)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("underlyingConstructorInfoDescriptor", underlyingConstructorInfoDescriptor);
      Assertion.IsFalse (underlyingConstructorInfoDescriptor.Attributes.IsSet (MethodAttributes.Static));

      _declaringType = declaringType;
      _underlyingConstructorInfoDescriptor = underlyingConstructorInfoDescriptor;

      _parameters = _underlyingConstructorInfoDescriptor.ParameterDescriptors
          .Select (pd => new MutableParameterInfo (this, pd))
          .ToList().AsReadOnly();

      _customAttributeDatas = new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (
          underlyingConstructorInfoDescriptor.CustomAttributeDataProvider);

      _body = _underlyingConstructorInfoDescriptor.Body;
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public ConstructorInfo UnderlyingSystemConstructorInfo
    {
      get { return _underlyingConstructorInfoDescriptor.UnderlyingSystemInfo ?? this; }
    }

    public bool IsNew
    {
      get { return _underlyingConstructorInfoDescriptor.UnderlyingSystemInfo == null; }
    }

    public bool IsModified
    {
      get { return _body != _underlyingConstructorInfoDescriptor.Body; }
    }

    public override string Name
    {
      get { return _underlyingConstructorInfoDescriptor.Name; }
    }

    public override MethodAttributes Attributes
    {
      get { return _underlyingConstructorInfoDescriptor.Attributes; }
    }

    public override CallingConventions CallingConvention
    {
      get
      {
        Assertion.IsFalse (IsStatic);
        return CallingConventions.HasThis;
      }
    }

    public ReadOnlyCollection<ParameterExpression> ParameterExpressions
    {
      get { return _underlyingConstructorInfoDescriptor.ParameterDescriptors.Select (pd => pd.Expression).ToList().AsReadOnly(); }
    }

    public Expression Body
    {
      get { return _body; }
    }

    public bool CanSetBody
    {
      // TODO 4695
      get { return IsNew || SubclassFilterUtility.IsVisibleFromSubclass (this); }
    }

    public void SetBody (Func<ConstructorBodyModificationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      if (!CanSetBody)
      {
        // TODO 4695
        var message = string.Format ("The body of the existing inaccessible constructor '{0}' cannot be replaced.", this);
        throw new NotSupportedException (message);
      }

      var memberSelector = new MemberSelector (new BindingFlagsEvaluator());
      var context = new ConstructorBodyModificationContext (_declaringType, ParameterExpressions, _body, memberSelector);
      _body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetConstructorSignature (this);
    }

    public string ToDebugString()
    {
      return string.Format ("MutableConstructor = \"{0}\", DeclaringType = \"{1}\"", ToString(), DeclaringType);
    }

    public override ParameterInfo[] GetParameters ()
    {
      return _parameters.ToArray();
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeDatas.Value;
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.GetCustomAttributes (this, attributeType, inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return TypePipeCustomAttributeImplementationUtility.IsDefined (this, attributeType, inherit);
    }

    #region Not Implemented from ConstructorInfo interface

    public override MethodImplAttributes GetMethodImplementationFlags ()
    {
      throw new NotImplementedException();
    }

    public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public override Type ReflectedType
    {
      get { throw new NotImplementedException(); }
    }

    public override RuntimeMethodHandle MethodHandle
    {
      get { throw new NotImplementedException(); }
    }

    public override object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}