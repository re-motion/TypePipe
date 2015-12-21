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
using Remotion.TypePipe.Dlr.Ast.Compiler;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Creates an instance of <see cref="OffsetTrackingILGenerator"/>, adapted to implement <see cref="IILGenerator"/>.
  /// </summary>
  [CLSCompliant (false)]
  public class OffsetTrackingILGeneratorFactory : IILGeneratorFactory
  {
    public IILGenerator CreateAdaptedILGenerator (ILGenerator realILGenerator)
    {
      ArgumentUtility.CheckNotNull ("realILGenerator", realILGenerator);
      
      // The OffsetTrackingILGenerator is defined by the DLR to add an ILOffsetProperty to ILGenerator under the CLR version 2 or Silverlight.
      // With .NET 4, the ILGenerator already has this property. When upgrading, we _could_ implement different ILGeneratorProvider and 
      // ILGeneratorAdapter classes that directly use ILGenerator.
      var offsetTrackingILGenerator = new OffsetTrackingILGenerator (realILGenerator);
      return new OffsetTrackingILGeneratorAdapter (offsetTrackingILGenerator);
    }
  }
}