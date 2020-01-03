using System.Collections.Generic;

namespace Netty.Examples.Common
{
  public interface ISubjectProvider
  {
    IList<Subject> Get();
  }
}