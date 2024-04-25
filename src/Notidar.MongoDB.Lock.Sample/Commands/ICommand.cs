namespace Notidar.MongoDB.Lock.Sample.Commands
{
    public interface ICommand<TCommandOptions>
    {
        Task ExecuteAsync(TCommandOptions options, CancellationToken cancellationToken = default);
    }
}
