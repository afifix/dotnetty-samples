using DotNetty.Transport.Channels;
using System;

namespace Netty.Examples.Common
{
  [Serializable]
  public delegate void NettyEventHandler(IChannelHandlerContext sender, EventArgs e);

  [Serializable]
  public delegate void NettyEventHandler<in T>(IChannelHandlerContext sender, T e);
}