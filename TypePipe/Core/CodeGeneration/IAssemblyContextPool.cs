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

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>Represents a synchronized pool of <see cref="AssemblyContext"/> objects.</summary>
  /// <remarks>
  /// An <see cref="AssemblyContext"/> can be removed from the pool via the <see cref="Dequeue"/> operation 
  /// and returned to the pool via the <see cref="Enqueue"/> operation.
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
  public interface IAssemblyContextPool
  {
    /// <summary>
    /// Returns a registered <see cref="AssemblyContext"/> to the pool.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <paramref name="assemblyContext"/> is not registered with the pool 
    /// or if an attempt is made to enqueue the same <see cref="AssemblyContext"/> twice.
    /// </exception>
    void Enqueue (AssemblyContext assemblyContext);

    /// <summary>
    /// Removes an <see cref="AssemblyContext"/> from the pool. If the pool is empty, the operation blocks until an <see cref="AssemblyContext"/> is 
    /// returned to the pool via another thread.
    /// </summary>
    AssemblyContext Dequeue ();

    /// <summary>
    /// Removes all registered <see cref="AssemblyContext"/> objects from the pool.  The method blocks until all registered contexts 
    /// have been returned from their respective consuming threads and are once again available for dequeuing.
    /// </summary>
    AssemblyContext[] DequeueAll ();
  }
}