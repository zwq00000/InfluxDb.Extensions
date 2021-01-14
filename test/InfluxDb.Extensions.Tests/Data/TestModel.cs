using System;

namespace InfluxDb.Extensions.Tests
{
    public class TestModel {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }

        public MockType Type{get;set;}
    }
}