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
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Defines an interface for <see cref="IILGenerator"/>.
  /// </summary>
  [CLSCompliant (false)]
  public interface IILGenerator
  {
    int ILOffset { get; }

    void BeginCatchBlock (Type exceptionType);
    void BeginExceptFilterBlock ();
    void BeginExceptionBlock ();
    void BeginFaultBlock ();
    void BeginFinallyBlock ();
    LocalBuilder DeclareLocal (Type localType);
    Label DefineLabel ();
    void Emit (OpCode opcode, Label[] labels);
    void Emit (OpCode opcode, Label label);
    void Emit (OpCode opcode, FieldInfo field);
    void Emit (OpCode opcode, LocalBuilder local);
    void Emit (OpCode opcode, string str);
    void Emit (OpCode opcode, MethodInfo meth);
    void Emit (OpCode opcode);
    void Emit (OpCode opcode, int arg);
    void Emit (OpCode opcode, byte arg);
    void Emit (OpCode opcode, sbyte arg);
    void Emit (OpCode opcode, long arg);
    void Emit (OpCode opcode, float arg);
    void Emit (OpCode opcode, double arg);
    void Emit (OpCode opcode, ConstructorInfo con);
    void Emit (OpCode opcode, Type cls);
    void EmitCall (OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes);
    void EndExceptionBlock ();
    void MarkLabel (Label loc);
    void MarkSequencePoint (ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn);
  }
}