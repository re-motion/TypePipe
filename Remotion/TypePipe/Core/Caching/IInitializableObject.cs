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
namespace Remotion.TypePipe.Caching
{
  /// <summary>
  /// <b>DO NOT USE THIS INTERFACE</b>; it is an infastructure interface.
  /// If a generated type implements this interface, the pipeline will invoke the <see cref="Initialize"/> method when creating instances of it.
  /// Note that the <see cref="Initialize"/> is called exactly once, independently of which API was used to retrieve the instance.
  /// </summary>
  /// <seealso cref="IObjectFactory.CreateInstance"/>
  /// <seealso cref="IObjectFactory.GetUninitializedObject"/>
  public interface IInitializableObject
  {
    void Initialize ();
  }
}