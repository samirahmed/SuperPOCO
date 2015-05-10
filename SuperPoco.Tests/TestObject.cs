using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SuperPoco.Tests
{
    public class TestObject
    {
        [JsonIgnore]
        public string IgnoreableValue { get; set; }

        public List<string> StringListValue { get; set; }

        public bool BoolValue { get; set; }

        public string StringValue { get; set; }

        public Guid GuidValue { get; set; }

        public DateTime DateTimeValue { get; set; }

        public byte[] BinaryValue { get; set; }

        public Double DoubleValue { get; set; }

        public Int64 Int64Value { get; set; }

        public Int32 Int32Value { get; set; }

        public static TestObject BuildTestObject()
        {
            return new TestObject
            {
                StringValue = "string",
                DoubleValue = 12.4,
                Int32Value = Convert.ToInt32(1125),
                Int64Value = Convert.ToInt64(125125),
                GuidValue = Guid.NewGuid(),
                DateTimeValue = DateTime.UtcNow,
                BoolValue = true,
                BinaryValue = Encoding.UTF8.GetBytes("hello world!"),
                StringListValue = new List<string>() {"hello", "world"},
                IgnoreableValue = "ignore this"
            };
        }
    }
}