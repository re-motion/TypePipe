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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.Development.UnitTesting.ObjectMothers;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableMethodInfoTest
  {
    private MutableType _declaringType;

    private UnderlyingMethodInfoDescriptor _descriptor;
    private MutableMethodInfo _mutableMethod;
    private MutableMethodInfo _newNonVirtualMethod;
    private MutableMethodInfo _newFinalMethod;
    private MutableMethodInfo _newVirtualMethod;

    private MutableMethodInfo _existingNonVirtualMethod;
    private MutableMethodInfo _existingFinalMethod;
    private MutableMethodInfo _existingVirtualMethod;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create(UnderlyingTypeDescriptorObjectMother.Create (typeof (DomainType)));

      _descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew ();
      _mutableMethod = Create(_descriptor);

      _newNonVirtualMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: 0));
      _newFinalMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Virtual | MethodAttributes.Final));
      _newVirtualMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Virtual));

      var nonVirtualUnderlyingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod ());
      _existingNonVirtualMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting (nonVirtualUnderlyingMethod));

      var finalUnderlyingMethod = typeof (DomainType).GetMethod ("FinalMethod");
      _existingFinalMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting (finalUnderlyingMethod));

      var virtualUnderlyingMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod ());
      _existingVirtualMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting (virtualUnderlyingMethod));
    }

    [Test]
    public void Initialization ()
    {
      var mutableMethodInfo = new MutableMethodInfo (_declaringType, _descriptor);

      Assert.That (mutableMethodInfo.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (mutableMethodInfo.Body, Is.SameAs (_descriptor.Body));
    }

    [Test]
    public void UnderlyingSystemMethodInfo ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Not.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.UnderlyingSystemMethodInfo, Is.SameAs (descriptor.UnderlyingSystemInfo));
    }

    [Test]
    public void UnderlyingSystemMethodInfo_ForNull ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.UnderlyingSystemMethodInfo, Is.SameAs (methodInfo));
    }

    [Test]
    public void IsNewMethod_True ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.IsNew, Is.True);
    }

    [Test]
    public void IsNewMethod_False ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (descriptor.UnderlyingSystemInfo, Is.Not.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.IsNew, Is.False);
    }

    [Test]
    public void IsModified_False ()
    {
      Assert.That (_mutableMethod.IsModified, Is.False);
    }

    [Test]
    public void IsModified_True_Body ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (_descriptor.ReturnType);
      _mutableMethod.SetBody (ctx => fakeBody);

      Assert.That (_mutableMethod.IsModified, Is.True);
    }

    [Test]
    public void IsModified_True_ExplicitBaseDefinition ()
    {
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod());
      _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition);

      Assert.That (_existingVirtualMethod.IsModified, Is.True);
    }

    [Test]
    public void Name ()
    {
      Assert.That (_mutableMethod.Name, Is.EqualTo (_descriptor.Name));
      }

    [Test]
    public void Attributes ()
    {
      Assert.That (_mutableMethod.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void CallingConvention ()
    {
      var instanceDescriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: 0);
      var staticDescriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Static);

      var instanceMethod = new MutableMethodInfo (_declaringType, instanceDescriptor);
      var staticMethod = new MutableMethodInfo (_declaringType, staticDescriptor);

      Assert.That (instanceMethod.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (staticMethod.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void ReturnType ()
    {
      Assert.That (_descriptor.ReturnType, Is.Not.Null);

      Assert.That (_mutableMethod.ReturnType, Is.SameAs (_descriptor.ReturnType));
    }

    [Test]
    public void BaseMethod ()
    {
      var baseMethod = ReflectionObjectMother.GetSomeMethod();
      var mutableMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (baseMethod: baseMethod));

      Assert.That (mutableMethod.BaseMethod, Is.SameAs (baseMethod));
    }

    [Test]
    public void GetBaseDefinition ()
    {
      var baseMethod = typeof (DomainType).GetMethod ("OverridingMethod");
      var rootDefinition = baseMethod.GetBaseDefinition ();
      Assert.That (rootDefinition, Is.Not.EqualTo (baseMethod));

      var mutableMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (baseMethod: baseMethod));

      Assert.That (mutableMethod.GetBaseDefinition(), Is.SameAs (rootDefinition));
    }

    [Test]
    public void GetBaseDefinition_NoBaseMethod ()
    {
      var mutableMethod = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (baseMethod: null));

      Assert.That (mutableMethod.GetBaseDefinition(), Is.SameAs (mutableMethod));
    }

    [Test]
    public void IsGenericMethod ()
    {
      var isGenericMethod = BooleanObjectMother.GetRandomBoolean();
      var method = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (isGenericMethod: isGenericMethod));

      Assert.That (method.IsGenericMethod, Is.EqualTo (isGenericMethod));
    }

    [Test]
    public void IsGenericMethodDefinition ()
    {
      var isGenericMethodDefinition = BooleanObjectMother.GetRandomBoolean ();
      var method = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (isGenericMethodDefinition: isGenericMethodDefinition));

      Assert.That (method.IsGenericMethodDefinition, Is.EqualTo (isGenericMethodDefinition));
    }

    [Test]
    public void ContainsGenericParameters ()
    {
      var containsGenericParameters = BooleanObjectMother.GetRandomBoolean ();
      var method = Create (UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (containsGenericParameters: containsGenericParameters));

      Assert.That (method.ContainsGenericParameters, Is.EqualTo (containsGenericParameters));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = UnderlyingParameterInfoDescriptorObjectMother.CreateMultiple (2);
      var methodInfo = CreateWithParameters (parameterDeclarations);

      Assert.That (methodInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
    }

    [Test]
    public void CanAddExplicitBaseDefinition ()
    {
      Assert.That (_newNonVirtualMethod.CanAddExplicitBaseDefinition, Is.False);
      Assert.That (_newFinalMethod.CanAddExplicitBaseDefinition, Is.True);
      Assert.That (_newVirtualMethod.CanAddExplicitBaseDefinition, Is.True);

      Assert.That (_existingNonVirtualMethod.CanAddExplicitBaseDefinition, Is.False);
      Assert.That (_existingFinalMethod.CanAddExplicitBaseDefinition, Is.False);

      Assert.That (_existingVirtualMethod.CanAddExplicitBaseDefinition, Is.True);
    }

    [Test]
    public void CanSetBody ()
    {
      Assert.That (_newNonVirtualMethod.CanSetBody, Is.True);
      Assert.That (_newFinalMethod.CanSetBody, Is.True);
      Assert.That (_newVirtualMethod.CanSetBody, Is.True);

      Assert.That (_existingNonVirtualMethod.CanSetBody, Is.False);
      Assert.That (_existingFinalMethod.CanSetBody, Is.False);

      Assert.That (_existingVirtualMethod.CanSetBody, Is.True);
    }

    [Test]
    public void AddExplicitBaseDefinition ()
    {
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod());

      _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition);

      Assert.That (_existingVirtualMethod.AddedExplicitBaseDefinitions, Is.EquivalentTo (new[] { overriddenMethodDefinition }));
    }

    [Test]
    public void AddExplicitBaseDefinition_AllowsMethodsFromHierarchy ()
    {
      _declaringType.AddInterface (typeof (IAddedInterface));
      var methodFromHierarchy = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IAddedInterface obj) => obj.VirtualMethod ());

      _existingVirtualMethod.AddExplicitBaseDefinition (methodFromHierarchy);

      Assert.That (_existingVirtualMethod.AddedExplicitBaseDefinitions, Is.EquivalentTo (new[] { methodFromHierarchy }));
    }

    [Test]
    public void AddExplicitBaseDefinition_ExistingInterfaceMethod ()
    {
      Assert.That (_declaringType.GetInterfaces(), Has.Member (typeof (IExistingInterface)));
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IExistingInterface obj) => obj.InterfaceMethod ());

      _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition);

      Assert.That (_existingVirtualMethod.AddedExplicitBaseDefinitions, Is.EquivalentTo (new[] { overriddenMethodDefinition }));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Cannot add an explicit base definition to the non-virtual or existing final method 'NonVirtualMethod'.")]
    public void AddExplicitBaseDefinition_CannotAddExplicitBaseDefinition ()
    {
      var overriddenMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod ());

      _existingNonVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition);
    }

    [Test]
    public void AddExplicitBaseDefinition_FinalAndVirtualMethods ()
    {
      var nonVirtualMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod ());
      var finalMethodDefinition = typeof (DomainType).GetMethod ("FinalMethod");

      var message = "Method must be virtual and non-final.\r\nParameter name: overriddenMethodBaseDefinition";
      Assert.That (
          () => _existingVirtualMethod.AddExplicitBaseDefinition (nonVirtualMethodDefinition),
          Throws.ArgumentException.With.Message.EqualTo (message));
      Assert.That (
          () => _existingVirtualMethod.AddExplicitBaseDefinition (finalMethodDefinition),
          Throws.ArgumentException.With.Message.EqualTo (message));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "Method signatures must be equal.\r\nParameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_IncompatibleSignatures ()
    {
      var differentSignatureMethodDefinition =
          NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethodWithDifferentSignature (7));

      _existingVirtualMethod.AddExplicitBaseDefinition (differentSignatureMethodDefinition);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The overridden method must be from the same type hierarchy.\r\nParameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_UnrelatedMethod ()
    {
      var unrelatedMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetMethod ((UnrelatedType obj) => obj.VirtualMethod ());

      _existingVirtualMethod.AddExplicitBaseDefinition (unrelatedMethodDefinition);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = 
        "The given method must be a root method definition. (Use GetBaseDefinition to get a root method.)\r\n"
        + "Parameter name: overriddenMethodBaseDefinition")]
    public void AddExplicitBaseDefinition_NoRootMethod ()
    {
      var nonBaseDefinitionMethod = typeof (DomainType).GetMethod ("OverridingMethod");

      _existingVirtualMethod.AddExplicitBaseDefinition (nonBaseDefinitionMethod);
    }

    [Test]
    public void AddExplicitBaseDefinition_TwiceWithSameMethod ()
    {
      var overriddenMethodDefinition1 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod ());
      var overriddenMethodDefinition2 = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.VirtualMethod2 ());

      _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition1);
      _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition2);
      Assert.That (
          () => _existingVirtualMethod.AddExplicitBaseDefinition (overriddenMethodDefinition2),
          Throws.InvalidOperationException.With.Message.EqualTo ("The given method has already been added to the list of explicit base definitions."));

      Assert.That (
          _existingVirtualMethod.AddedExplicitBaseDefinitions,
          Is.EquivalentTo (new[] { overriddenMethodDefinition1, overriddenMethodDefinition2 }));
    }

    [Test]
    public void SetBody ()
    {
      MethodAttributes nonVirtualAttribtes = 0;
      var returnType = typeof (object);
      var parameterDescriptors = UnderlyingParameterInfoDescriptorObjectMother.CreateMultiple (2);
      var baseMetod = ReflectionObjectMother.GetSomeMethod();
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (
          "Method", nonVirtualAttribtes, returnType, parameterDescriptors, baseMetod);
      var mutableMethod = Create (descriptor);
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (int));
      Func<MethodBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.That (mutableMethod.ParameterExpressions, Is.Not.Empty);
        Assert.That (context.Parameters, Is.EqualTo (mutableMethod.ParameterExpressions));
        Assert.That (context.DeclaringType, Is.SameAs (mutableMethod.DeclaringType));
        Assert.That (context.IsStatic, Is.False);
        Assert.That (mutableMethod.BaseMethod, Is.Not.Null);
        Assert.That (context.BaseMethod, Is.SameAs (mutableMethod.BaseMethod));
        Assert.That (context.PreviousBody, Is.SameAs (mutableMethod.Body));

        return fakeBody;
      };

      mutableMethod.SetBody (bodyProvider);

      var expectedBody = Expression.Convert (fakeBody, returnType);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, mutableMethod.Body);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The body of the existing non-virtual or final method 'NonVirtualMethod' cannot be replaced.")]
    public void SetBody_CannotSetBody ()
    {
      var nonVirtualMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.NonVirtualMethod());
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting (nonVirtualMethod);
      var mutableMethod = Create (descriptor);

      Func<MethodBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.Fail ("Should not be called.");
        throw new NotImplementedException ();
      };

      mutableMethod.SetBody (bodyProvider);
    }


    [Test]
    public void ToString_WithParameters ()
    {
      var parameters = new[]
                       {
                           UnderlyingParameterInfoDescriptorObjectMother.CreateForNew (typeof (int), "p1"),
                           UnderlyingParameterInfoDescriptorObjectMother.CreateForNew (typeof (string).MakeByRefType(), "p2", ParameterAttributes.Out)
                       };
      var methodInfo = MutableMethodInfoObjectMother.Create (returnType: typeof (string), name: "Xxx", parameterDescriptors: parameters);

      Assert.That (methodInfo.ToString(), Is.EqualTo ("String Xxx(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var methodInfo = MutableMethodInfoObjectMother.Create (
          declaringType: MutableTypeObjectMother.CreateForExistingType (GetType()),
          returnType: typeof (void),
          name: "Xxx",
          parameterDescriptors: new[] { UnderlyingParameterInfoDescriptorObjectMother.CreateForNew (typeof (int), "p1") });

      var expected = "MutableMethod = \"Void Xxx(Int32)\", DeclaringType = \"MutableMethodInfoTest\"";
      Assert.That (methodInfo.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void GetParameters ()
    {
      var parameters = UnderlyingParameterInfoDescriptorObjectMother.CreateMultiple (2);
      var methodInfo = CreateWithParameters (parameters);

      var result = methodInfo.GetParameters();

      var actualParameterInfos = result.Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes });
      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) methodInfo, Position = 0, ParameterType = parameters[0].Type, parameters[0].Name, parameters[0].Attributes },
              new { Member = (MemberInfo) methodInfo, Position = 1, ParameterType = parameters[1].Type, parameters[1].Name, parameters[1].Attributes },
          };
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
    }

    [Test]
    public void GetParameters_ReturnsSameParameterInfoInstances ()
    {
      var methodInfo = CreateWithParameters (UnderlyingParameterInfoDescriptorObjectMother.CreateForNew());

      var result1 = methodInfo.GetParameters ().Single ();
      var result2 = methodInfo.GetParameters ().Single ();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var methodInfo = CreateWithParameters (UnderlyingParameterInfoDescriptorObjectMother.CreateForNew());

      var parameters = methodInfo.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = methodInfo.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var result = _existingNonVirtualMethod.GetCustomAttributeData();

      Assert.That (result.Select (a => a.Constructor.DeclaringType), Is.EquivalentTo (new[] { typeof (AbcAttribute) }));
    }

    [Test]
    public void GetCustomAttributeData_Lazy ()
    {
      var result1 = _existingNonVirtualMethod.GetCustomAttributeData ();
      var result2 = _existingNonVirtualMethod.GetCustomAttributeData ();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void VirtualMethodsImplementedByMethodInfo ()
    {
      // None of these members should throw an exception 
      Dev.Null = _mutableMethod.MemberType;
    }

    [Test]
    public void UnsupportedMembers ()
    {
      CheckThrowsNotSupported (() => Dev.Null = _mutableMethod.MetadataToken, "Property", "MetadataToken");
      CheckThrowsNotSupported (() => Dev.Null = _mutableMethod.Module, "Property", "Module");
      CheckThrowsNotSupported (() => _mutableMethod.Invoke (null, 0, null, null, null), "Method", "Invoke");
    }

    private void CheckThrowsNotSupported (TestDelegate memberInvocation, string memberType, string memberName)
    {
      var message = string.Format ("{0} {1} is not supported.", memberType, memberName);
      Assert.That (memberInvocation, Throws.TypeOf<NotSupportedException> ().With.Message.EqualTo (message));
    }

    private MutableMethodInfo Create (UnderlyingMethodInfoDescriptor descriptor)
    {
      return new MutableMethodInfo (_declaringType, descriptor);
    }

    private MutableMethodInfo CreateWithParameters (params UnderlyingParameterInfoDescriptor[] parameterDescriptors)
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (parameterDescriptors: parameterDescriptors);
      return new MutableMethodInfo (_declaringType, descriptor);
    }

    public class DomainTypeBase
    {
      public virtual void OverridingMethod () { }
      public virtual void FinalMethod () { }
    }

    public class DomainType : DomainTypeBase, IExistingInterface
    {
      public virtual void VirtualMethod () { }
      public virtual void VirtualMethod2 () { }
      public virtual void VirtualMethodWithDifferentSignature (int i) { Dev.Null = i; }
      [Abc]
      public void NonVirtualMethod () { }

      public override void OverridingMethod () { }
      public sealed override void FinalMethod () { }
      public void InterfaceMethod () { }
    }

    public class UnrelatedType
    {
      public virtual void VirtualMethod () { }
    }

    public interface IAddedInterface
    {
      void VirtualMethod ();
    }

    public interface IExistingInterface
    {
      void InterfaceMethod ();
    }

    public class AbcAttribute : Attribute { }
  }
}