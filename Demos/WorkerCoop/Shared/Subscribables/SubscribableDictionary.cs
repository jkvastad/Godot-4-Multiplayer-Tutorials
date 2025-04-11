using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Shared.GlobalConstants;
namespace Shared;

public class SubscribableDictionary<K, V> : IEnumerable<KeyValuePair<K, V>> where K : notnull
{
    [JsonInclude]
    public Dictionary<K, V> _dictionary { get; private set; } = [];

    public Dictionary<K, V>.ValueCollection Values => _dictionary.Values;

    public event Action<K, V, UpdateType>? OnChange;

    public SubscribableDictionary() { }

    [JsonConstructor]
    public SubscribableDictionary(Dictionary<K, V> _Dictionary)
    {
        _dictionary = _Dictionary;
    }

    public Unsubscriber Subscribe(Action<K, V, UpdateType> action)
    {
        OnChange += action;
        return new Unsubscriber(this, action);
    }
    public V this[K key]
    {
        get
        {
            return _dictionary[key];
        }
        set
        {
            _dictionary[key] = value;
            OnChange?.Invoke(key, value, UpdateType.Add);
        }
    }

    public void Remove(K key)
    {
        V value = _dictionary[key];
        _dictionary.Remove(key);
        OnChange?.Invoke(key, value, UpdateType.Remove);
    }

    public bool ContainsKey(K k)
    {
        if (_dictionary.ContainsKey(k))
            return true;
        else
            return false;
    }

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<K, V>>)_dictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dictionary).GetEnumerator();
    }

    public class Unsubscriber : IDisposable
    {
        private Action<K, V, UpdateType> action;
        private SubscribableDictionary<K, V> observableDictionary;

        public Unsubscriber(SubscribableDictionary<K, V> observableDictionary, Action<K, V, UpdateType> action)
        {
            this.observableDictionary = observableDictionary;
            this.action = action;
        }

        public void Dispose()
        {
            observableDictionary.OnChange -= action;
        }
    }
}

public class ObservableDictionaryConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(SubscribableDictionary<,>))
            return true;
        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] typeArguments = typeToConvert.GetGenericArguments();
        Type keyType = typeArguments[0];
        Type valueType = typeArguments[1];

        Type converterType = typeof(Converter<,>).MakeGenericType(keyType, valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
    public class Converter<K, V> : JsonConverter<SubscribableDictionary<K, V>> where K : notnull
    {
        public override SubscribableDictionary<K, V> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new(JsonSerializer.Deserialize<Dictionary<K, V>>(ref reader, options)!);
        }

        public override void Write(Utf8JsonWriter writer, SubscribableDictionary<K, V> value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(value._dictionary, options));
        }
    }
}