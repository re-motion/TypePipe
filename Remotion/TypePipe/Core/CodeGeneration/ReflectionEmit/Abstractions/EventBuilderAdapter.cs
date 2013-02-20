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
using System.Reflection.Emit;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// Adapts <see cref="EventBuilder"/> with the <see cref="IEventBuilder"/> interface.
  /// </summary>
  public class EventBuilderAdapter : BuilderAdapterBase, IEventBuilder
  {
    private readonly EventBuilder _eventBuilder;
    private readonly ReadOnlyDictionary<IMethodBuilder, MethodBuilder> _methodMapping;

    [CLSCompliant (false)]
    public EventBuilderAdapter (EventBuilder eventBuilder, ReadOnlyDictionary<IMethodBuilder, MethodBuilder> methodMapping)
        : base (ArgumentUtility.CheckNotNull ("eventBuilder", eventBuilder).SetCustomAttribute)
    {
      ArgumentUtility.CheckNotNull ("methodMapping", methodMapping);

      _eventBuilder = eventBuilder;
      _methodMapping = methodMapping;
    }

    [CLSCompliant (false)]
    public void SetAddOnMethod (IMethodBuilder addMethodBuilder)
    {
      ArgumentUtility.CheckNotNull ("addMethodBuilder", addMethodBuilder);

      _eventBuilder.SetAddOnMethod (_methodMapping[addMethodBuilder]);
    }

    [CLSCompliant (false)]
    public void SetRemoveOnMethod (IMethodBuilder removeMethodBuilder)
    {
      ArgumentUtility.CheckNotNull ("removeMethodBuilder", removeMethodBuilder);

      _eventBuilder.SetRemoveOnMethod (_methodMapping[removeMethodBuilder]);
    }

    [CLSCompliant (false)]
    public void SetRaiseMethod (IMethodBuilder raiseMethodBuilder)
    {
      ArgumentUtility.CheckNotNull ("raiseMethodBuilder", raiseMethodBuilder);

      _eventBuilder.SetRaiseMethod (_methodMapping[raiseMethodBuilder]);
    }
  }
}