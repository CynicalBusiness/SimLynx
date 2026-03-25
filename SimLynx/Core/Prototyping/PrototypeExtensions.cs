
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SimLynx.Core.Prototyping;

/// <summary>
/// Extensions for prototypes and related types.
/// </summary>
public static class PrototypeExtensions
{

    extension(PropertyInfo propertyInfo)
    {

        /// <summary>
        /// Indicates whether or not this property is required on its type.
        /// </summary>
        /// <remarks>
        /// A property is considered required if it uses the <see langword="required"/> modifier or marked with
        /// <see cref="RequiredAttribute"/>.
        /// </remarks>
        public bool IsRequired => propertyInfo.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: false)
            || propertyInfo.IsDefined(typeof(RequiredAttribute));

    }

}
