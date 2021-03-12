using System;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests {
    public class TaskPerformance : IDisposable {
        private readonly ITestOutputHelper output;
        private readonly string taskName;
        private readonly DateTime start;

        public TaskPerformance (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        public TaskPerformance (ITestOutputHelper outputHelper, string taskName) {
            this.output = outputHelper;
            this.taskName = taskName;
            output.WriteLine ($"start task {taskName}");
            this.start = DateTime.Now;
        }

        public IDisposable NewExec (string name) {
            return new TaskPerformance (this.output, name);
        }

        public void Dispose () {
            output.WriteLine ($"end task {taskName} with time {DateTime.Now-start}");
        }
    }
}