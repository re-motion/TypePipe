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
using System.Reflection;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Provides useful methods for investigating event overrides.
  /// </summary>
  public class RelatedEventFinder : IRelatedEventFinder
  {
    public EventInfo GetBaseEvent (EventInfo @event)
    {
      ArgumentUtility.CheckNotNull ("event", @event);

      Assertion.IsNotNull (@event.DeclaringType);
      var baseTypeSequence = EnumerableExtensions.CreateSequence (@event.DeclaringType.BaseType, t => t.BaseType);
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var baseEvents = from t in baseTypeSequence
                       from e in t.GetEvents (bindingFlags)
                       where IsBaseEvent (e, @event)
                       select e;
      return baseEvents.FirstOrDefault();
    }

    private bool IsBaseEvent (EventInfo baseCandidateEvent, EventInfo @event)
    {
      var adder = @event.GetAddMethod (true);
      var remover = @event.GetRemoveMethod (true);
      Assertion.IsNotNull (adder, "Events must always have an add method.");
      Assertion.IsNotNull (remover, "Events must always have a remove method.");

      var baseCandidateAdder = baseCandidateEvent.GetAddMethod (true);
      var baseCandidateRemover = baseCandidateEvent.GetRemoveMethod (true);
      Assertion.IsNotNull (baseCandidateAdder, "Events must always have an add method.");
      Assertion.IsNotNull (baseCandidateRemover, "Events must always have a remove method.");

      return HasSameBaseDefinition (adder, baseCandidateAdder) || HasSameBaseDefinition (remover, baseCandidateRemover);
    }

    private bool HasSameBaseDefinition (MethodInfo a, MethodInfo b)
    {
      return MethodBaseDefinitionCache.GetBaseDefinition (a) == MethodBaseDefinitionCache.GetBaseDefinition (b);
    }
  }
}