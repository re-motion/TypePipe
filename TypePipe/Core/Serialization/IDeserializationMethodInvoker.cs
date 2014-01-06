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
using System.Runtime.Serialization;

namespace Remotion.TypePipe.Serialization
{
  /// <summary>
  /// An abstraction for calling <see cref="IDeserializationCallback.OnDeserialization"/> and methods annotated with
  /// <see cref="OnDeserializingAttribute"/> or <see cref="OnDeserializedAttribute"/> on an deserialized object.
  /// </summary>
  public interface IDeserializationMethodInvoker
  {
    /// <summary>
    /// Invokes methods with the <see cref="OnDeserializingAttribute"/>.
    /// </summary>
    void InvokeOnDeserializing (object instance, StreamingContext context);

    /// <summary>
    /// Invokes methods with the <see cref="OnDeserializedAttribute"/>.
    /// </summary>
    void InvokeOnDeserialized (object instance, StreamingContext context);

    /// <summary>
    /// Invokes <see cref="IDeserializationCallback.OnDeserialization"/> if the <paramref name="instance"/> implements <see cref="IDeserializationCallback"/>.
    /// </summary>
    void InvokeOnDeserialization (object instance, object sender);
  }
}