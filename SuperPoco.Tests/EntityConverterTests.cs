using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.TableStorage.SuperPoco;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;

namespace SuperPoco.Tests
{
    [TestClass]
    public class EntityConverterTests
    {
        [TestMethod]
        public void ConvertToPocoTest()
        {
            var expected = TestObject.BuildTestObject();
            var props = new Dictionary<string, EntityProperty>()
            {
                {"StringValue", new EntityProperty(expected.StringValue)},
                {"BoolValue", new EntityProperty(expected.BoolValue)},
                {"DoubleValue", new EntityProperty(expected.DoubleValue)},
                {"Int32Value", new EntityProperty(expected.Int32Value)},
                {"Int64Value", new EntityProperty(expected.Int64Value)},
                {"GuidValue", new EntityProperty(expected.GuidValue)},
                {"BinaryValue", new EntityProperty(expected.BinaryValue)},
                {"DateTimeValue", new EntityProperty(expected.DateTimeValue)},
            };

            var entity = new DynamicTableEntity("hello", "world") {Properties = props};
            var actual = EntityConverter.ConvertTo<TestObject>(entity);

            actual.Should().NotBeNull();
            actual.DateTimeValue.Should().Be(expected.DateTimeValue);
            actual.DoubleValue.Should().Be(expected.DoubleValue);
            actual.Int32Value.Should().Be(expected.Int32Value);
            actual.GuidValue.Should().Be(expected.GuidValue);
            actual.StringValue.Should().Be(expected.StringValue);
            actual.Int64Value.Should().Be(expected.Int64Value);
            actual.BoolValue.Should().Be(expected.BoolValue);
            actual.BinaryValue.Should().BeSameAs(expected.BinaryValue);

            actual.IgnoreableValue.Should().BeNullOrWhiteSpace();
            actual.StringListValue.Should().BeNull();
        }

        [TestMethod]
        public void WriteToEntityPropertyTest()
        {
            KeyValuePair<string, EntityProperty>? kv;

            kv = EntityConverter.WriteToEntityProperty(new JProperty("string", "world"));
            ValidateKv(kv,"string","world");

            kv = EntityConverter.WriteToEntityProperty(new JProperty("number", 1125));
            ValidateKv(kv,"number", (long) 1125);

            kv = EntityConverter.WriteToEntityProperty(new JProperty("float", 42.4));
            ValidateKv(kv,"float",42.4);

            var date = new DateTime(2015, 10, 10, 15, 5, 5);
            kv = EntityConverter.WriteToEntityProperty(new JProperty("dateTime", date));
            ValidateKv(kv ,"dateTime", date);

            var guid = System.Guid.NewGuid();
            kv = EntityConverter.WriteToEntityProperty(new JProperty("guid", guid));
            ValidateKv(kv, "guid", guid);
            
            kv = EntityConverter.WriteToEntityProperty(new JProperty("binary-false", false));
            ValidateKv(kv, "binary-false", false);
            
            kv = EntityConverter.WriteToEntityProperty(new JProperty("binary-true", true));
            ValidateKv(kv, "binary-true", true);

            var bytes = Encoding.UTF8.GetBytes("hello world");
            kv = EntityConverter.WriteToEntityProperty(new JProperty("bytes", bytes));
            ValidateKv(kv, "bytes", bytes);

            kv = EntityConverter.WriteToEntityProperty(new JProperty("bytes", new List<string>(){"hello"}));
            ValidateKv(kv, null, new List<string>(), false);
        }

        [TestMethod]
        public void CovertToDynamicTableEntityTest()
        {
            var testObj = TestObject.BuildTestObject();
            var currentTime = DateTime.Now;
            var dynamicTableEntity = EntityConverter.ConvertToDynamicTableEntity(testObj, (_ => "partitionA"), (_ => "rowB"), currentTime, "etag");
            
            dynamicTableEntity.PartitionKey.Should().Be("partitionA");
            dynamicTableEntity.RowKey.Should().Be("rowB");
            dynamicTableEntity.Timestamp.Should().Be(currentTime);
            dynamicTableEntity.ETag.Should().Be("etag");

            dynamicTableEntity.Properties.Should().NotBeNull();
            dynamicTableEntity.Properties.Keys.Count.Should().Be(8);
            dynamicTableEntity.Properties["StringValue"].StringValue.Should().Be(testObj.StringValue);
            dynamicTableEntity.Properties["BoolValue"].BooleanValue.Should().Be(testObj.BoolValue);
            dynamicTableEntity.Properties["BinaryValue"].BinaryValue.Should().BeSameAs(testObj.BinaryValue);
            dynamicTableEntity.Properties["Int32Value"].Int64Value.Should().Be(testObj.Int32Value); // Converts 32 to 64 always
            dynamicTableEntity.Properties["Int64Value"].Int64Value.Should().Be(testObj.Int64Value);
            dynamicTableEntity.Properties["DoubleValue"].DoubleValue.Should().Be(testObj.DoubleValue);
            dynamicTableEntity.Properties["GuidValue"].GuidValue.Should().Be(testObj.GuidValue);
            dynamicTableEntity.Properties["DateTimeValue"].DateTime.Should().Be(testObj.DateTimeValue);
        }

        [TestMethod]
        public void WriteToJobjectTest()
        {
            var jObject = new JObject();
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("string", new EntityProperty("world")));
            ValidateJObject(jObject,"string","world",1);

            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("number", new EntityProperty(1125)));
            ValidateJObject(jObject,"number",1125,2);
            
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("float", new EntityProperty(42.4)));
            ValidateJObject(jObject,"float",42.4,3);

            var date = new DateTime(2015, 10, 10, 15, 5, 5);
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("dateTime", new EntityProperty(date)));
            ValidateJObject(jObject,"dateTime", date ,4);

            var guid = System.Guid.NewGuid();
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("guid", new EntityProperty(guid)));
            ValidateJObject(jObject, "guid", guid ,5);
            
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("binary-false", new EntityProperty(false)));
            ValidateJObject(jObject, "binary-false", false,6);
            
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("binary-true", new EntityProperty(true)));
            ValidateJObject(jObject, "binary-true", true,7);

            var bytes = Encoding.UTF8.GetBytes("hello world");
            EntityConverter.WriteToJObject(jObject, new KeyValuePair<string,EntityProperty>("bytes", new EntityProperty(bytes)));
            ValidateJObject(jObject, "bytes", bytes,8);
        }
        
        public static void ValidateKv<TValue>(KeyValuePair<string, EntityProperty>? actual, string key, TValue value, bool hasValue = true)
        {
            actual.HasValue.Should().Be(hasValue);
            if (actual.HasValue)
            {
                actual.Value.Key.Should().Be(key);
                actual.Value.Value.PropertyAsObject.Should().Be(value);
            }
        }

        public static void ValidateJObject<TValue>(JObject jObject, string name, TValue expected, int expectedSize)
        {
            jObject.Values<JProperty>().Count().Should().Be(expectedSize);
            jObject[name].Should().NotBeNull();
            jObject[name].ToObject<TValue>().Should().Be(expected);
        }
    }
}
