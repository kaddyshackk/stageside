namespace ComedyPull.Application.Modules.Public.Shared
{
    public record ComedianResponse(
        string Slug,
        string Name,
        string? Bio
    );
}