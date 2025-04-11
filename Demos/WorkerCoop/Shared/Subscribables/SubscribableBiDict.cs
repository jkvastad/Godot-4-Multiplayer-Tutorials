using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Shared.GlobalConstants;
namespace Shared;

//Disable nullable key warning as reverse dictionary needs to have value type for key
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

public class SubscribableBiDict<K, V> : IEnumerable<KeyValuePair<K, V>> where K : notnull
{    
    [JsonInclude]
    public Dictionary<K, V> _dictionary { get; private set; } = [];
    public Dictionary<V, K> _reverseDictionary { get; private set; } = [];

    public Dictionary<K, V>.ValueCollection Values => _dictionary.Values;


    private event Action<K, V, UpdateType>? OnChange; // See Subscribe method

    public SubscribableBiDict() { }

    [JsonConstructor]
    public SubscribableBiDict(Dictionary<K, V> _Dictionary)
    {
        var values = _Dictionary.Values;
        if (values.Count != values.Distinct().Count())
            throw new ArgumentException("duplicate values in bidirectional dictionary");
        _dictionary = _Dictionary;
        _reverseDictionary = _dictionary.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    /// <summary>
    /// Subscribes to the OnChange event with the specified action.
    /// action parameters represent key, value and if entry was removed from dictionary
    /// <returns>An <see cref="Unsubscriber"/> that can be used to unsubscribe from the event.</returns>
    public Unsubscriber Subscribe(Action<K, V, UpdateType> action)
    {
        OnChange += action;
        return new Unsubscriber(this, action);
    }
    public void Add(K key, V value)
    {
        if (_reverseDictionary.ContainsKey(value))
            throw new ArgumentException($"Added value {value} already in BiDict");

        _dictionary[key] = value;
        _reverseDictionary[value] = key;

        OnChange?.Invoke(key, value, UpdateType.Add);
    }

    public void Remove(K key)
    {
        V value = _dictionary[key];

        _reverseDictionary.Remove(value);
        _dictionary.Remove(key);

        OnChange?.Invoke(key, value, UpdateType.Remove);
    }

    public V GetByKey(K key)
    {
        return _dictionary[key];
    }

    public K GetByValue(V value)
    {
        return _reverseDictionary[value];
    }

    public bool ContainsKey(K key)
    {
        if (_dictionary.ContainsKey(key))
            return true;
        else
            return false;
    }

    public bool ContainsValue(V value)
    {
        if (_reverseDictionary.ContainsKey(value))
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
        private SubscribableBiDict<K, V> observableDictionary;

        public Unsubscriber(SubscribableBiDict<K, V> observableDictionary, Action<K, V, UpdateType> action)
        {
            this.observableDictionary = observableDictionary;
            this.action = action;
        }

        public void Dispose()
        {
            observableDictionary.OnChange -= action;
            return;
        }
    }
}

public class SubscribableBiDictConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(SubscribableBiDict<,>))
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
    public class Converter<K, V> : JsonConverter<SubscribableBiDict<K, V>> where K : notnull
    {
        public override SubscribableBiDict<K, V> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new(JsonSerializer.Deserialize<Dictionary<K, V>>(ref reader, options)!);
        }

        public override void Write(Utf8JsonWriter writer, SubscribableBiDict<K, V> value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(value._dictionary, options));
        }
    }
}