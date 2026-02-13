using DislocationFilter.Application.Abstractions.Messaging;
using DislocationFilter.Application.Common;

namespace DislocationFilter.Application.UseCases.MainWindow.ReloadMainTitle;

public sealed record ReloadMainTitleCommand(string Prefix) : ICommand<Result<string, string>>;
