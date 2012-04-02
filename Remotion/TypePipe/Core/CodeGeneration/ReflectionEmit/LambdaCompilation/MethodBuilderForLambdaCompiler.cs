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
using System.Reflection.Emit;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Adapts <see cref="MethodBuilder"/> for the <see cref="IMethodBuilderForLambdaCompiler"/> interface.
  /// </summary>
  /// <remarks>
  /// This class is internal because it should only be used from <see cref="TypeModifier"/> and <see cref="TypeModificationHandler"/>.
  /// </remarks>
  internal class MethodBuilderForLambdaCompiler : IMethodBuilderForLambdaCompiler
  {
    private readonly MethodBuilder _methodBuilder;
    private readonly IILGeneratorFactory _ilGeneratorFactory;
    private readonly bool _ignoreSignatureModifications;

    public MethodBuilderForLambdaCompiler (MethodBuilder methodBuilder, IILGeneratorFactory ilGeneratorFactory, bool ignoreSignatureModifications)
    {
      ArgumentUtility.CheckNotNull ("methodBuilder", methodBuilder);
      ArgumentUtility.CheckNotNull ("ilGeneratorFactory", ilGeneratorFactory);

      _methodBuilder = methodBuilder;
      _ilGeneratorFactory = ilGeneratorFactory;
      _ignoreSignatureModifications = ignoreSignatureModifications;
    }

    public Type DeclaringType
    {
      get { return _methodBuilder.DeclaringType; }
    }

    public void SetReturnType (Type returnType)
    {
      // If method builder was created by the TypePipe, ignore because return type should have been correctly set prior to this call.
      if (!_ignoreSignatureModifications)
        _methodBuilder.SetReturnType (returnType);
      else
        Assertion.IsTrue (returnType == _methodBuilder.ReturnType);
    }

    public void SetParameters (Type[] parameterTypes)
    {
      // If method builder was created by the TypePipe, ignore because parameters should have been correctly set prior to this call.
      // We cannot assert correctness because _constructorBuilder.GetParameters() throws.
      if (!_ignoreSignatureModifications)
        _methodBuilder.SetParameters (parameterTypes);
    }

    public void DefineParameter (int position, ParameterAttributes attributes, string strParamName)
    {
      // If method builder was created by the TypePipe, ignore because parameters should have been correctly set prior to this call.
      // (Prevent duplicate ParamTokens)
      // We cannot assert correctness because _constructorBuilder.GetParameters() throws.
      if (!_ignoreSignatureModifications)
        _methodBuilder.DefineParameter (position, attributes, strParamName);
    }

    public IILGenerator GetILGenerator ()
    {
      return _ilGeneratorFactory.CreateAdaptedILGenerator (_methodBuilder.GetILGenerator ());
    }

    public MethodBase AsMethodBase ()
    {
      return _methodBuilder;
    }
  }
}