namespace Netty.Examples.Common
{
  public class ReadIdleStateEvent
  {
    /// <summary>
    /// consturctor
    /// </summary>
    /// <param name="retries">number of retries</param>
    /// <param name="maxExceeded">will be true if max retries exceeded</param>
    public ReadIdleStateEvent(int retries, bool maxExceeded)
    {
      Retries = retries;
      MaxRetriesExceeded = maxExceeded;
    }

    /// <summary>
    /// Returns the number of retries
    /// </summary>
    public int Retries { get; }

    /// <summary>
    /// Returns <code>true</code> if this max retries exceeded
    /// </summary>
    public bool MaxRetriesExceeded { get; }
  }
}