using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Shared;

public class SubscribableValue<T>
{
    private T _value;
    public event Action<T>? OnChange;
    public T Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            OnChange?.Invoke(_value);
        }
    }

    [JsonConstructor]
    public SubscribableValue(T value)
    {
        _value = value;
    }

    public Unsubscriber Subscribe(Action<T> action)
    {
        OnChange += action;
        return new Unsubscriber(this, action);
    }

    // Can read wrapper implicitly, but not convert to it - must go via Value property setter to trigger OnChange event.
    public static implicit operator T(SubscribableValue<T> wrapper)
    {
        return wrapper.Value;
    }

    public class Unsubscriber : IDisposable
    {

        private Action<T> action;
        private SubscribableValue<T> subscribableValue;

        public Unsubscriber(SubscribableValue<T> subscribableValue, Action<T> action)
        {
            this.subscribableValue = subscribableValue;
            this.action = action;
        }
        public void Dispose()
        {
            subscribableValue.OnChange -= action;
        }
    }
}

public class SubscribableValueConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(SubscribableValue<>))
            return true;
        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArguments = typeToConvert.GetGenericArguments();
        Type valueType = typeArguments[0];

        Type converterType = typeof(Converter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
    public class Converter<T> : JsonConverter<SubscribableValue<T>>
    {
        public override SubscribableValue<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new(JsonSerializer.Deserialize<T>(ref reader, options)!);
        }

        public override void Write(Utf8JsonWriter writer, SubscribableValue<T> value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(value.Value, options));
        }
    }
}