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
using Remotion.TypePipe.Serialization.Implementation;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  public class TestableDeserializationSurrogateBase : DeserializationSurrogateBase
  {
    private readonly Func<IObjectFactory, Type, StreamingContext, object> _createRealObjectAssertions;

    public TestableDeserializationSurrogateBase (
        SerializationInfo serializationInfo,
        StreamingContext streamingContext,
        Func<IObjectFactory, Type, StreamingContext, object> createRealObjectAssertions)
        : base (serializationInfo, streamingContext)
    {
      _createRealObjectAssertions = createRealObjectAssertions;
    }

    protected override object CreateRealObject (IObjectFactory objectFactory, Type underlyingType, StreamingContext context)
    {
      return _createRealObjectAssertions (objectFactory, underlyingType, context);
    }
  }
}