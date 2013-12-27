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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.MemberSignatures;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// A factory for creating <see cref="MutableEventInfo"/> instances.
  /// </summary>
  public class EventFactory
  {
    private readonly IMethodFactory _methodFactory;

    public EventFactory (IMethodFactory methodFactory)
    {
      ArgumentUtility.CheckNotNull ("methodFactory", methodFactory);

      _methodFactory = methodFactory;
    }

    public MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        Type handlerType,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> addBodyProvider,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNullAndTypeIsAssignableFrom ("handlerType", handlerType, typeof (Delegate));
      ArgumentUtility.CheckNotNull ("addBodyProvider", addBodyProvider);
      ArgumentUtility.CheckNotNull ("removeBodyProvider", removeBodyProvider);
      // Raise body provider may be null.

      MemberAttributesUtility.ValidateAttributes (
          "event accessor methods", MemberAttributesUtility.InvalidMethodAttributes, accessorAttributes, "accessorAttributes");

      var signature = new EventSignature (handlerType);
      if (declaringType.AddedEvents.Any (e => e.Name == name && EventSignature.Create (e).Equals (signature)))
        throw new InvalidOperationException ("Event with equal name and signature already exists.");

      var attributes = accessorAttributes | MethodAttributes.SpecialName;
      var addRemoveParameters = new[] { new ParameterDeclaration (handlerType, "handler") };

      var addMethod = CreateAccessor (declaringType, "add_" + name, attributes, typeof (void), addRemoveParameters, addBodyProvider);
      var removeMethod = CreateAccessor (declaringType, "remove_" + name, attributes, typeof (void), addRemoveParameters, removeBodyProvider);

      MutableMethodInfo raiseMethod = null;
      if (raiseBodyProvider != null)
      {
        var invokeMethod = GetInvokeMethod (handlerType);
        var raiseParameters = invokeMethod.GetParameters().Select (p => new ParameterDeclaration (p.ParameterType, p.Name, p.Attributes));
        raiseMethod = CreateAccessor (declaringType, "raise_" + name, attributes, invokeMethod.ReturnType, raiseParameters, raiseBodyProvider);
      }

      return new MutableEventInfo (declaringType, name, EventAttributes.None, addMethod, removeMethod, raiseMethod);
    }

    public MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        EventAttributes attributes,
        MutableMethodInfo addMethod,
        MutableMethodInfo removeMethod,
        MutableMethodInfo raiseMethod)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("addMethod", addMethod);
      ArgumentUtility.CheckNotNull ("removeMethod", removeMethod);
      // Raise method may be null.

      MemberAttributesUtility.ValidateAttributes ("events", MemberAttributesUtility.InvalidEventAttributes, attributes, "attributes");

      if (addMethod.IsStatic != removeMethod.IsStatic || (raiseMethod != null && raiseMethod.IsStatic != addMethod.IsStatic))
        throw new ArgumentException ("Accessor methods must be all either static or non-static.", "addMethod");

      if (!ReferenceEquals (addMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Add method is not declared on the current type.", "addMethod");
      if (!ReferenceEquals (removeMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Remove method is not declared on the current type.", "removeMethod");
      if (raiseMethod != null && !ReferenceEquals (raiseMethod.DeclaringType, declaringType))
        throw new ArgumentException ("Raise method is not declared on the current type.", "raiseMethod");

      if (addMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Add method must have return type void.", "addMethod");
      if (removeMethod.ReturnType != typeof (void))
        throw new ArgumentException ("Remove method must have return type void.", "removeMethod");

      var addMethodParameterTypes = addMethod.GetParameters ().Select (p => p.ParameterType).ToList ();
      var removeMethodParameterTypes = removeMethod.GetParameters ().Select (p => p.ParameterType).ToList ();

      if (addMethodParameterTypes.Count != 1 || !addMethodParameterTypes[0].IsSubclassOf (typeof (Delegate)))
        throw new ArgumentException ("Add method must have a single parameter that is assignable to 'System.Delegate'.", "addMethod");
      if (removeMethodParameterTypes.Count != 1 || !removeMethodParameterTypes[0].IsSubclassOf (typeof (Delegate)))
        throw new ArgumentException ("Remove method must have a single parameter that is assignable to 'System.Delegate'.", "removeMethod");

      if (addMethodParameterTypes.Single () != removeMethodParameterTypes.Single ())
        throw new ArgumentException ("The type of the handler parameter is different for the add and remove method.", "removeMethod");

      var handlerType = addMethodParameterTypes.Single ();
      var invokeMethod = GetInvokeMethod (handlerType);
      if (raiseMethod != null && !MethodSignature.AreEqual (raiseMethod, invokeMethod))
        throw new ArgumentException ("The signature of the raise method does not match the handler type.", "raiseMethod");

      var signature = new EventSignature (handlerType);
      if (declaringType.AddedEvents.Any (e => e.Name == name && EventSignature.Create (e).Equals (signature)))
        throw new InvalidOperationException ("Event with equal name and signature already exists.");

      return new MutableEventInfo (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }

    private MethodInfo GetInvokeMethod (Type delegateType)
    {
      Assertion.IsTrue (delegateType.IsSubclassOf (typeof (Delegate)));
      return delegateType.GetMethod ("Invoke", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private MutableMethodInfo CreateAccessor (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _methodFactory.CreateMethod (
          declaringType, name, attributes, GenericParameterDeclaration.None, ctx => returnType, ctx => parameters, bodyProvider);
    }
  }
}