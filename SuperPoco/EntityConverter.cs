namespace Azure.TableStorage.SuperPoco
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class EntityConverter
    {
        /// <summary>
        ///  Convert any JSON.NET seriliazable Object into a Dynamic Table Entity
        /// </summary>
        /// <typeparam name="TValue">Object Type to be Seriliazed</typeparam>
        /// <param name="poco">.NET Object to made into Table Object</param>
        /// <param name="partitionKeySelector">Function to Select Partition Key Object</param>
        /// <param name="rowKeySelector">Function to Select Row key from Object</param>
        /// <param name="timeStamp">Timestamp on Table Entity</param>
        /// <param name="etag">Etag on Table Entity</param>
        /// <param name="jsonSerializer">Optional Custom Json Serializer</param>
        /// <returns>Dynamic Table Entity to be stored in Azure Table Storage</returns>
        public static DynamicTableEntity ConvertToDynamicTableEntity<TValue>(TValue poco,
            Func<TValue, string> partitionKeySelector, 
            Func<TValue, string> rowKeySelector, 
            DateTimeOffset? timeStamp = null,
            string etag = null, 
            JsonSerializer jsonSerializer = null)
        {
            return ConvertToDynamicTableEntity(poco,
                partitionKeySelector(poco),
                rowKeySelector(poco),
                timeStamp,
                etag,
                jsonSerializer);
        }

        /// <summary>
        ///  Convert any JSON.NET seriliazable Object into a Dynamic Table Entity
        /// </summary>
        /// <param name="poco">.NET Object to made into Table Object</param>
        /// <param name="partitionKey">Function to Select Partition Key Object</param>
        /// <param name="rowKey">Function to Select Row key from Object</param>
        /// <param name="timeStamp">Timestamp on Table Entity</param>
        /// <param name="etag">Etag on Table Entity</param>
        /// <param name="jsonSerializer">Optional Custom Json Serializer</param>
        /// <returns>Dynamic Table Entity to be stored in Azure Table Storage</returns>
        public static DynamicTableEntity ConvertToDynamicTableEntity(object poco, 
            string partitionKey = null, 
            string rowKey = null,
            DateTimeOffset? timeStamp = null, 
            string etag = null, 
            JsonSerializer jsonSerializer = null)
        {
            var dynamicTableEntity = new DynamicTableEntity
            {
                RowKey = rowKey,
                PartitionKey = partitionKey,
                Properties = new Dictionary<string, EntityProperty>()
            };

            if (timeStamp.HasValue)
            {
                dynamicTableEntity.Timestamp = timeStamp.Value;
            }

            if (!string.IsNullOrWhiteSpace(etag))
            {
                dynamicTableEntity.ETag = etag;
            }

            var jObject = jsonSerializer != null ? JObject.FromObject(poco, jsonSerializer) : JObject.FromObject(poco);

            foreach (var pair in jObject.Values<JProperty>().Select(WriteToEntityProperty).Where(pair => pair.HasValue))
            {
                dynamicTableEntity.Properties.Add(pair.Value);
            }

            return dynamicTableEntity;
        }

        /// <summary>
        ///  Converts a dynamic table entity to .NET Object
        /// </summary>
        /// <typeparam name="TOutput">Desired Object Type</typeparam>
        /// <param name="entity">Dynamic table Entity</param>
        /// <returns>Output Object</returns>
        public static TOutput ConvertTo<TOutput>(DynamicTableEntity entity)
        {
            return ConvertTo<TOutput>(entity.Properties);
        }

        /// <summary>
        ///  Convert a Dynamic Table Entity to A POCO .NET Object.
        /// </summary>
        /// <typeparam name="TOutput">Desired Object Types</typeparam>
        /// <param name="properties">Dictionary of Table Entity</param>
        /// <returns>.NET object</returns>
        public static TOutput ConvertTo<TOutput>(IDictionary<string, EntityProperty> properties)
        {
            var jobject = new JObject();
            foreach (var property in properties)
            {
                WriteToJObject(jobject, property);
            }
            return jobject.ToObject<TOutput>();
        }

        public static KeyValuePair<string, EntityProperty>? WriteToEntityProperty(JProperty property)
        {
            var value = property.Value;
            var name = property.Name;

            switch (value.Type)
            {
                case JTokenType.Bytes:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<byte[]>()));
                case JTokenType.Boolean:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<bool>()));
                case JTokenType.Date:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<DateTime>()));
                case JTokenType.Float:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<double>()));
                case JTokenType.Guid:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<Guid>()));
                case JTokenType.Integer:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<Int64>()));
                case JTokenType.String:
                    return new KeyValuePair<string, EntityProperty>(name, new EntityProperty(value.ToObject<string>()));
                default:
                    return null;
            }
        }

        public static void WriteToJObject(JObject jObject, KeyValuePair<string, EntityProperty> property)
        {
            switch (property.Value.PropertyType)
            {
                case EdmType.Binary:
                    jObject.Add(property.Key, new JValue(property.Value.BinaryValue));
                    return;
                case EdmType.Boolean:
                    jObject.Add(property.Key, new JValue(property.Value.BooleanValue));
                    return;
                case EdmType.DateTime:
                    jObject.Add(property.Key, new JValue(property.Value.DateTime));
                    return;
                case EdmType.Double:
                    jObject.Add(property.Key, new JValue(property.Value.DoubleValue));
                    return;
                case EdmType.Guid:
                    jObject.Add(property.Key, new JValue(property.Value.GuidValue));
                    return;
                case EdmType.Int32:
                    jObject.Add(property.Key, new JValue(property.Value.Int32Value)); 
                    return;
                case EdmType.Int64:
                    jObject.Add(property.Key, new JValue(property.Value.Int64Value));
                    return;
                case EdmType.String:
                    jObject.Add(property.Key, new JValue(property.Value.StringValue));
                    return;
                default:
                    return;
            }
        }
    }
}
