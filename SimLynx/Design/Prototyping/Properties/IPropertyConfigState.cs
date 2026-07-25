using System;

namespace SimLynx.Design.Prototyping.Properties;

internal interface IPropertyConfigState : IPropertyConfig
{
    void CopyTo(IPropertyConfig other);
}

internal interface IPropertyConfigState<TValue> : IPropertyConfigState
{
    void SetValue(Func<TValue> value);
    void AddConfiguration(Func<TValue, TValue> configuration);
}
