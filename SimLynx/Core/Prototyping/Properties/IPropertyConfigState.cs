using System;

namespace SimLynx.Core.Prototyping.Properties;

internal interface IPropertyConfigState
{
    void CopyTo(IPropertyConfig other);
}

internal interface IPropertyConfigState<TValue> : IPropertyConfigState
{
    void SetValue(Func<TValue> value);
    void AddConfiguration(Func<TValue, TValue> configuration);
}
