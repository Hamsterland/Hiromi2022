using System.ComponentModel;
using Hiromi.Services;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Hiromi.Commands;

public class NekogirlCommands : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ITrashgirlService _trashgirlService;
    private readonly ICommandContext _commandContext;

    public NekogirlCommands(
        IDiscordRestChannelAPI channelApi,
        ITrashgirlService trashgirlService, 
        ICommandContext commandContext)
    {
        _channelApi = channelApi;
        _trashgirlService = trashgirlService;
        _commandContext = commandContext;
    }

    [Command("trashgirl")]
    [Description("sends a trashgirl image with two users")]
    public async Task<Result> Trashgirl
    (
        [Description("the left trashgirl")] IUser left,
        [Description("the right trashgirl")] IUser right
    )
    {
        var trashgirlResult = await _trashgirlService.GenerateTrashgirlImage(left, right);
        if (!trashgirlResult.IsSuccess)
        {
            return Result.FromError(trashgirlResult);
        }
        
        var path = trashgirlResult.Entity;
        await using var stream = File.Open(path, FileMode.Open);

        var sendTrashgirlResult = await _channelApi.CreateMessageAsync
        (
            _commandContext.ChannelID,
            $"<@{left.ID.Value}> <@{right.ID.Value}>",
            attachments: new List<OneOf<FileData, IPartialAttachment>> { new FileData(path, stream) }
        );

        return sendTrashgirlResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(sendTrashgirlResult);
    }
}