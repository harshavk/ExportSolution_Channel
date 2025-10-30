using System.Threading.Channels;

namespace ExportWeb.Background
{
    public interface IExportChannel
    {
        ValueTask EnqueueAsync(Guid jobId);
        ChannelReader<Guid> Reader { get; }
    }

    public class ExportChannel : IExportChannel
    {
        private readonly Channel<Guid> _channel;

        public ExportChannel()
        {
            _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public ChannelReader<Guid> Reader => _channel.Reader;

        public async ValueTask EnqueueAsync(Guid jobId) => await _channel.Writer.WriteAsync(jobId);
    }
}
