using System.Collections.Generic;

namespace Netty.Examples.Common
{
    public class FakeSubjectProvider : ISubjectProvider
    {
        public IList<Subject> Get() => new List<Subject> { new Subject { Id = 1, Code = "APP", Max = 3 } };
    }
}
