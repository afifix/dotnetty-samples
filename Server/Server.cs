using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotNetty.Transport.Channels.Groups;

using Microsoft.Extensions.Logging;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Server
{
    public class Server : IServerSession
    {
        private readonly ILogger<Server> _logger;
        private readonly IChannelFactory _channelFactory;
        private readonly ISubjectProvider _subjectProvider;

        private ConcurrentDictionary<int, IChannelGroup> _groups;
        private ConcurrentDictionary<int, Subject> _subjects;

        private IChannelWrapper _channel;

        public Server(
          ILoggerFactory loggerFactory,
          IChannelFactory channelFactory,
          ISubjectProvider subjectProvider)
        {
            _logger = loggerFactory?.CreateLogger<Server>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            _subjectProvider = subjectProvider ?? throw new ArgumentNullException(nameof(subjectProvider));
        }

        public event EventHandler Connected;

        public event EventHandler Closed;

        public event EventHandler<ReadIdleStateEventArgs> ClientTimedout;

        public event EventHandler<PacketEventArgs<Ping>> ClientPinged;

        public event EventHandler<PacketEventArgs<Subscribe>> ClientSubscribed;

        public event EventHandler NewClientConnected;

        public event EventHandler ClientDisconnected;

        public async Task RunAsync()
        {
            if(_channel == null && _channelFactory is ServerChannelFactory factory)
                _channel = factory.Create(
                    OnClientTimedout,
                    OnClientPinged,
                    Subscribe,
                    OnNewClientConnected,
                    OnClientDisconnected);

            if(_channel == null || _channel.Active)
                return;

            await _channel.RunAsync();

            _subjects = null;
            _groups = null;
            _groups = new ConcurrentDictionary<int, IChannelGroup>();
            _subjects = new ConcurrentDictionary<int, Subject>();
            foreach(var subject in _subjectProvider.Get())
            {
                _ = _groups.TryAdd(subject.Id, _channel.NewChannelGroup());
                _ = _subjects.TryAdd(subject.Id, subject);
            }
            _logger.LogTrace("subjects initialized.");
            OnConnected();
        }

        public async Task CloseAsync()
        {
            if(_channel == null || !_channel.Active)
                return;

            await _channel.CloseAsync();

            var tasks = _groups.Values.Select(x => x.CloseAsync());
            await Task.WhenAll(tasks);

            _subjects = null;
            _groups = null;
            _channel = null;
            OnClosed();
        }

        public int GetTotalConnection() => _channel.ChannelGroup.Count;

        internal async void Subscribe(NettyPacketEventArgs<Subscribe> e)
        {
            var ctx = e.Context;
            var channel = ctx.Channel;
            var packet = e.Packet;
            if(!_subjects.TryGetValue(packet.SubjectId, out var subject))
            {
                await ctx.WriteAndFlushAsync(Suback.New(packet, 0, "subject not found"));
                return;
            }

            if(!_groups.TryGetValue(packet.SubjectId, out var group))
            {
                // should never happen
                await ctx.WriteAndFlushAsync(Suback.New(packet, 0, "group not found"));
                return;
            }

            if(subject.Max <= group.Count)
            {
                _logger.LogInformation($"{channel.Id.AsShortText()} not accecpted into subject {subject.Code}");
                await ctx.WriteAndFlushAsync(Suback.New(packet, 0, "subject exceeded the maximum number of connections"));
                return;
            }

            group.Add(ctx.Channel);
            _logger.LogInformation($"{channel.Id.AsShortText()} accecpted into subject {subject.Code}");
            await ctx.WriteAndFlushAsync(Suback.New(packet, 1, $"welcome to {subject.Code}, [{channel.Id}]"));
            OnClientSubscribed(packet);
        }

        internal async void OnClientTimedout(ReadIdleStateEventArgs e)
        {
            _logger.LogTrace("raise `Timedout` event");
            _ = ThreadPool.QueueUserWorkItem(s => ClientTimedout?.Invoke(this, e));

            if(!e.MaxRetriesExceeded)
                return;

            var ctx = e.Context;
            await ctx.CloseAsync();
        }

        internal async void OnClientPinged(NettyPacketEventArgs<Ping> e)
        {
            _logger.LogTrace("raise `ping` event");
            _ = ThreadPool.QueueUserWorkItem(s => ClientPinged?.Invoke(this, e.ToPacketEventArgs()));

            var ctx = e.Context;
            await ctx.WriteAndFlushAsync(Pong.New(e.Packet));
        }

        internal void OnClientSubscribed(Subscribe packet)
        {
            _logger.LogTrace("raise `client subscribed` event");
            _ = ThreadPool.QueueUserWorkItem(s => ClientSubscribed?.Invoke(this, new PacketEventArgs<Subscribe>(packet)));
        }

        internal void OnNewClientConnected(NettyEventArgs e)
        {
            _logger.LogTrace("raise `new client connected` event");
            _ = ThreadPool.QueueUserWorkItem(s => NewClientConnected?.Invoke(this, EventArgs.Empty));

            var ctx = e.Context;
            var channelId = ctx.Channel.Id;
            _logger.LogInformation(channelId.AsShortText());
            _logger.LogInformation($"total active sessions: {GetTotalConnection()}");
        }

        internal void OnClientDisconnected(NettyEventArgs e)
        {
            _logger.LogTrace("raise `client disconnected` event");
            _ = ThreadPool.QueueUserWorkItem(s => ClientDisconnected?.Invoke(this, EventArgs.Empty));
            _logger.LogInformation($"total active sessions: {GetTotalConnection()}");
        }

        internal void OnConnected()
        {
            _logger.LogTrace("raise `Connected` event");
            _ = ThreadPool.QueueUserWorkItem(s => Connected?.Invoke(this, EventArgs.Empty));
        }

        internal void OnClosed()
        {
            _logger.LogTrace("raise `Closed` event");
            _ = ThreadPool.QueueUserWorkItem(s => Closed?.Invoke(this, EventArgs.Empty));
        }
    }
}
