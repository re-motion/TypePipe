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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom <see cref="EventInfo"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the 
  /// implementation. Note that the equality members <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomEventInfo : EventInfo, ICustomAttributeDataProvider
  {
    private readonly CustomType _declaringType;
    private readonly string _name;
    private readonly EventAttributes _attributes;
    private readonly MethodInfo _addMethod;
    private readonly MethodInfo _removeMethod;
    private readonly MethodInfo _raiseMethod;

    protected CustomEventInfo (
        CustomType declaringType,
        string name,
        EventAttributes attributes,
        MethodInfo addMethod,
        MethodInfo removeMethod,
        MethodInfo raiseMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("addMethod", addMethod);
      ArgumentUtility.CheckNotNull ("removeMethod", removeMethod);
      // Raise method may be null.
      var delegateType = addMethod.GetParameters().Single().ParameterType;
      Assertion.IsTrue (typeof (Delegate).IsTypePipeAssignableFrom (delegateType));
      Assertion.IsTrue (removeMethod.GetParameters().Single().ParameterType == delegateType);
      var parameterTypes = delegateType.GetMethod ("Invoke").GetParameters().Select (p => p.ParameterType);
      Assertion.IsTrue (raiseMethod == null || raiseMethod.GetParameters().Select (p => p.ParameterType).SequenceEqual (parameterTypes));

      _declaringType = declaringType;
      _name = name;
      _attributes = attributes;
      _addMethod = addMethod;
      _removeMethod = removeMethod;
      _raiseMethod = raiseMethod;
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override EventAttributes Attributes
    {
      get { return _attributes; }
    }

    public override MethodInfo GetAddMethod (bool nonPublic)
    {
      return _addMethod.IsPublic || nonPublic ? _addMethod : null;
    }

    public override MethodInfo GetRemoveMethod (bool nonPublic)
    {
      return _removeMethod.IsPublic || nonPublic ? _removeMethod : null;
    }

    public override MethodInfo GetRaiseMethod (bool nonPublic)
    {
      return _raiseMethod != null && (_raiseMethod.IsPublic || nonPublic) ? _raiseMethod : null;
    }

    public override MethodInfo[] GetOtherMethods (bool nonPublic)
    {
      return new MethodInfo[0];
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
      return SignatureDebugStringGenerator.GetEventSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\", DeclaringType = \"{2}\"", GetType().Name.Replace ("Info", ""), ToString(), DeclaringType);
    }

    #region Unsupported Members

    public override Type ReflectedType
    {
      get { throw new NotSupportedException ("Property ReflectedType is not supported."); }
    }

    #endregion
  }
}