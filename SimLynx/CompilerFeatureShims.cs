/*
 * Auto-generated set of shims for compiler features not present in .NET Standard 2.1.
 */

#if NETSTANDARD
#pragma warning disable IDE0130
#pragma warning disable IDE0290

using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit;

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property,
        Inherited = false
    )]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class RequiredMemberAttribute : Attribute;

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class SetsRequiredMembersAttribute : Attribute;
}

#endif
