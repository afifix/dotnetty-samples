using System;
using System.Threading.Tasks;

using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;

namespace Netty.Examples.Common
{
    public abstract class BaseChannelWrapper : IChannelWrapper
    {
        protected IChannel _channel;
        protected bool _disposed;
        protected bool _connecting;
        protected bool _closing;

        public bool Active => _channel?.Active ?? false;

        public IChannelGroup ChannelGroup { get; protected set; }

        public abstract Task CloseAsync();

        public abstract Task RunAsync();

        public async Task WriteAsync(Packet packet)
        {
            if(_channel == null || !_channel.Active || _closing || _connecting || _disposed)
                return;

            await _channel.WriteAndFlushAsync(packet);
        }

        public IChannelGroup NewChannelGroup()
        {
            if(_channel == null || !_channel.Active)
                throw new ApplicationException("server not running.");

            var executor = _channel.Pipeline.FirstContext().Executor;
            return new DefaultChannelGroup(executor);
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly

        public void Dispose()
        {
            if(_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if(_disposed)
                return;

            if(disposing)
                await CloseAsync();

            _disposed = true;
        }

        ~BaseChannelWrapper()
        {
            Dispose(false);
        }
    }
}
