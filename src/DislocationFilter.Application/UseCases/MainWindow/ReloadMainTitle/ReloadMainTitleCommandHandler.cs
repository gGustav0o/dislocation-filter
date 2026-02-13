using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Abstractions.Time;
using DislocationFilter.Application.Common;

namespace DislocationFilter.Application.UseCases.MainWindow.ReloadMainTitle;

public sealed class ReloadMainTitleCommandHandler
    : ICommandHandler<ReloadMainTitleCommand, Result<string, string>>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReloadMainTitleCommandHandler(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<string, string>> Handle(ReloadMainTitleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Prefix))
        {
            return Task.FromResult(Result<string, string>.Failure("Title prefix cannot be empty."));
        }

        var title = $"{command.Prefix} - {_dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        return Task.FromResult(Result<string, string>.Success(title));
    }
}
