using Hiromi.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Hiromi.Services;

public interface ITrashgirlService
{
    Task<Result<string>> GenerateTrashgirlImage(IUser left, IUser right);
}

public class TrashgirlService : ITrashgirlService
{
    private readonly HttpClient _client = new();
    
    public async Task<Result<string>> GenerateTrashgirlImage(IUser left, IUser right)
    {
        var leftAvatarResult = GetAvatarFromUser(left);
        if (!leftAvatarResult.IsSuccess)
        {
            return Result<string>.FromError(leftAvatarResult);
        }

        var rightAvatarResult = GetAvatarFromUser(right);
        if (!rightAvatarResult.IsSuccess)
        {
            return Result<string>.FromError(rightAvatarResult);
        }

        var leftStream = await GetStreamFromUrl(leftAvatarResult.Entity);
        var rightStream = await GetStreamFromUrl(rightAvatarResult.Entity);

        using var background = await Image.LoadAsync("nekogirls.jpg");
        using var leftImage = await Image.LoadAsync(leftStream);
        using var rightImage = await Image.LoadAsync(rightStream);

        leftImage.Resize(0.65);
        rightImage.Resize(0.55);

        using var backgroundOverlayLeft = background.Overlay(leftImage, 375, 175);
        using var backgroundOverlayBoth = backgroundOverlayLeft.Overlay(rightImage, 570, 200);
        
        var path = GetFilename(left, right);
        await backgroundOverlayBoth.SaveAsJpegAsync(path, new JpegEncoder());
        return path;
    }

    private static Result<Uri> GetAvatarFromUser(IUser user)
    {
        var avatarResult = CDN.GetUserAvatarUrl(user, imageSize: 128);
        if (avatarResult.IsSuccess)
        {
            return avatarResult.Entity;
        }
        
        var defaultAvatarResult = CDN.GetDefaultUserAvatarUrl(user, imageSize: 128);
        if (!defaultAvatarResult.IsSuccess)
        {
            return Result<Uri>.FromError(avatarResult);
        }

        return defaultAvatarResult.Entity;
    }

    private async Task<Stream> GetStreamFromUrl(Uri uri)
    {
        var response = await _client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        return stream;
    }

    private static void ResizeImage(Image image, double percentage)
    {
        var width = (int) Math.Floor(image.Width * percentage);
        var height = (int) Math.Floor(image.Height * percentage);
        image.Mutate(x => x.Resize(width, height));
    }

    private static Image OverlayImage(Image background, Image overlay, Point point)
    {
        return background.Clone(x => x.DrawImage(overlay, point, new GraphicsOptions()));
    }
    
    private static string GetFilename(IUser left, IUser right)
    {
        return $"trashgirls-{left.ID.Value}-{right.ID.Value}.jpeg";
    }
}