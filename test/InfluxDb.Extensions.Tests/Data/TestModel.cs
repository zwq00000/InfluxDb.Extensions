using System;

namespace InfluxDb.Extensions.Tests {
    public class TestModel {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }

        public float FloatValue { get; set; }

        public double DoubleValue { get; set; }

        public long LongValue { get; set; }

        public int IntValue { get; set; }

        public short ShortValue { get; set; }

        public int ReadonlyValue { get; }

        public MockType Type { get; set; }
    }
}