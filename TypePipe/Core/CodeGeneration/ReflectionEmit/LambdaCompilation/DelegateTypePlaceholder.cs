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
using Remotion.TypePipe.Dlr.Ast.Compiler;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Acts as a placeholder for delegate <see cref="Type"/>s in the <see cref="LambdaCompiler"/> and must be replaced during code generation.
  /// </summary>
  public class DelegateTypePlaceholder : CustomType
  {
    private const TypeAttributes c_delegateTypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;

    private readonly Type _returnType;
    private readonly IReadOnlyList<Type> _parameterTypes;
    private readonly IReadOnlyCollection<MethodInfo> _methods;

    public DelegateTypePlaceholder (Type returnType, IEnumerable<Type> parameterTypes)
        : base ("DelegateTypePlaceholder", null, c_delegateTypeAttributes, null, EmptyTypes)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterTypes", parameterTypes);

      SetBaseType (typeof (MulticastDelegate));

      _returnType = returnType;
      _parameterTypes = parameterTypes.ToList().AsReadOnly();
      _methods = CreateMethods (returnType, _parameterTypes).ToList().AsReadOnly();
    }

    public Type ReturnType
    {
      get { return _returnType; }
    }

    public IReadOnlyList<Type> ParameterTypes
    {
      get { return _parameterTypes; }
    }

    private IEnumerable<MethodInfo> CreateMethods (Type returnType, IEnumerable<Type> parameterTypes)
    {
      // The following implementation is not complete. We skip 'BeginInvoke', 'EndInvoke' and methods from 'MulticastDelegate' base type.
      // This is OK because the only purpose of this class is to be a placeholder within the LambdaCompiler, which only queries for the 'Invoke' method.

      var attributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig;
      var parameters = parameterTypes.Select (t => new ParameterDeclaration (t));

      yield return new MethodOnCustomType (this, "Invoke", attributes, EmptyTypes, returnType, parameters);
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new NotSupportedException ("Method GetCustomAttributeData is not supported.");
    }

    public override IEnumerable<Type> GetAllNestedTypes ()
    {
      throw new NotSupportedException ("Method GetAllNestedTypes is not supported.");
    }

    public override IEnumerable<Type> GetAllInterfaces ()
    {
      throw new NotSupportedException ("Method GetAllInterfaces is not supported.");
    }

    public override IEnumerable<FieldInfo> GetAllFields ()
    {
      throw new NotSupportedException ("Method GetAllFields is not supported.");
    }

    public override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      throw new NotSupportedException ("Method GetAllConstructors is not supported.");
    }

    public override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return _methods;
    }

    public override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      throw new NotSupportedException ("Method GetAllProperties is not supported.");
    }

    public override IEnumerable<EventInfo> GetAllEvents ()
    {
      throw new NotSupportedException ("Method GetAllEvents is not supported.");
    }
  }
}