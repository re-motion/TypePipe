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
using System.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration
{
  /// <summary>
  /// Prepares an assembled type for code generation.
  /// </summary>
  public class AssembledTypePreparer : IAssembledTypePreparer
  {
    private const string c_typeIDFieldName = "__typeID";

    public void AddTypeID (MutableType proxyType, IEnumerable<Expression> typeID)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);
      ArgumentUtility.CheckNotNull ("typeID", typeID);

      var typeIDField = proxyType.AddField (c_typeIDFieldName, FieldAttributes.Private | FieldAttributes.Static, typeof (object[]));

      proxyType.AddTypeInitialization (
          ctx => Expression.Assign (
              Expression.Field (null, typeIDField),
              Expression.NewArrayInit (typeof (object), typeID)));
    }

    public object[] ExtractTypeID (Type assembledType)
    {
      ArgumentUtility.CheckNotNull ("assembledType", assembledType);

      var typeIDField = assembledType.GetField (c_typeIDFieldName, BindingFlags.NonPublic | BindingFlags.Static);
      Assertion.IsNotNull (typeIDField);

      return (object[]) typeIDField.GetValue (null);
    }
  }
}