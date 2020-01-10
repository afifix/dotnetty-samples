using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Server
{
    public class Server : IServerSession, IDisposable
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

        public event EventHandler<ReadIdleStateEvent> ClientTimedout;

        public event EventHandler<Ping> ClientPinged;

        public event EventHandler<Subscribe> ClientSubscribed;

        public event EventHandler NewClientConnected;

        public event EventHandler ClientDisconnected;

        public bool Disposed { get; set; }

        public async Task RunAsync()
        {
            if (_channel == null && _channelFactory is ServerChannelFactory factory)
                _channel = factory.Create(
                    OnClientTimedout,
                    OnClientPinged,
                    Subscribe,
                    OnNewClientConnected,
                    OnClientDisconnected);

            if (_channel == null || _channel.Active)
                return;

            await _channel.RunAsync();

            _subjects = null;
            _groups = null;
            _groups = new ConcurrentDictionary<int, IChannelGroup>();
            _subjects = new ConcurrentDictionary<int, Subject>();
            foreach (var subject in _subjectProvider.Get())
            {
                _groups.TryAdd(subject.Id, _channel.NewChannelGroup());
                _subjects.TryAdd(subject.Id, subject);
            }
            _logger.LogTrace("subjects initialized.");
            OnConnected();
        }

        public async Task CloseAsync()
        {
            if (_channel == null || !_channel.Active)
                return;

            await _channel.CloseAsync();

            var tasks = _groups.Values.Select(x => x.CloseAsync());
            await Task.WhenAll(tasks);

            _subjects = null;
            _groups = null;
            _channel = null;

            OnClosed();
        }

        public async void Dispose()
        {
            if (Disposed)
                return;

            await CloseAsync();
            Disposed = true;
        }

        public int GetTotalConnection()
        {
            return _channel.ChannelGroup.Count;
        }

        internal async void Subscribe(IChannelHandlerContext ctx, Subscribe e)
        {
            var channel = ctx.Channel;
            if (!_subjects.TryGetValue(e.SubjectId, out var subject))
            {
                await ctx.WriteAndFlushAsync(Suback.New(e, 0, "subject not found"));
                return;
            }

            if (!_groups.TryGetValue(e.SubjectId, out var group))
            {
                // should never happen
                await ctx.WriteAndFlushAsync(Suback.New(e, 0, "group not found"));
                return;
            }

            if (subject.Max <= group.Count)
            {
                _logger.LogInformation($"{channel.Id.AsShortText()} not accecpted into subject {subject.Code}");
                await ctx.WriteAndFlushAsync(Suback.New(e, 0, "subject exceeded the maximum number of connections"));
                return;
            }

            group.Add(ctx.Channel);
            _logger.LogInformation($"{channel.Id.AsShortText()} accecpted into subject {subject.Code}");
            await ctx.WriteAndFlushAsync(Suback.New(e, 1, $"welcome to {subject.Code}, [{channel.Id}]"));
            OnClientSubscribed(ctx, e);
        }

        internal async void OnClientTimedout(IChannelHandlerContext ctx, ReadIdleStateEvent e)
        {
            _logger.LogTrace("raise `Timedout` event");
            ThreadPool.QueueUserWorkItem(s => { ClientTimedout?.Invoke(this, e); });

            if (!e.MaxRetriesExceeded) return;
            await ctx.CloseAsync();
        }

        internal async void OnClientPinged(IChannelHandlerContext ctx, Ping e)
        {
            _logger.LogTrace("raise `ping` event");
            ThreadPool.QueueUserWorkItem(s => { ClientPinged?.Invoke(this, e); });
            await ctx.WriteAndFlushAsync(Pong.New(e));
        }

        internal void OnClientSubscribed(IChannelHandlerContext ctx, Subscribe e)
        {
            _logger.LogTrace("raise `client subscribed` event");
            ThreadPool.QueueUserWorkItem(s => { ClientSubscribed?.Invoke(this, e); });
        }

        internal void OnNewClientConnected(IChannelHandlerContext ctx)
        {
            _logger.LogTrace("raise `new client connected` event");
            ThreadPool.QueueUserWorkItem(s => { NewClientConnected?.Invoke(this, EventArgs.Empty); });
            var channelId = ctx.Channel.Id;
            _logger.LogInformation(channelId.AsShortText());
            _logger.LogInformation($"total session: {GetTotalConnection()}");
        }

        internal void OnClientDisconnected(IChannelHandlerContext ctx)
        {
            _logger.LogTrace("raise `client disconnected` event");
            ThreadPool.QueueUserWorkItem(s => { ClientDisconnected?.Invoke(this, EventArgs.Empty); });
            var channelId = ctx.Channel.Id;
            _logger.LogInformation(channelId.AsShortText());
            _logger.LogInformation($"total session: {GetTotalConnection()}");
        }

        internal void OnConnected()
        {
            _logger.LogTrace("raise `Connected` event");
            ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(this, EventArgs.Empty); });
        }

        internal void OnClosed()
        {
            _logger.LogTrace("raise `Closed` event");
            ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(this, EventArgs.Empty); });
        }
    }
}
