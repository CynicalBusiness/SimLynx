/*
 * Auto-generated set of shims for compiler features not present in .NET Standard 2.1.
 */

#if NETSTANDARD && !NO_COMPILER_FEATURE_SHIMS
#pragma warning disable IDE0130
#pragma warning disable IDE0290
#pragma warning disable CS1591

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit;

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property,
        Inherited = false
    )]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class RequiredMemberAttribute : Attribute;

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }
        public bool IsOptional { get; set; }

        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class MemberNotNullWhenAttribute : Attribute
    {
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            ReturnValue = returnValue;
            Members = members;
        }

        public bool ReturnValue { get; }
        public string[] Members { get; }
    }
}

#endif
