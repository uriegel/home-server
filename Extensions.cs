using LinqTools;
using AspNetExtensions;

static class Extensions
{
    public static Task NotFound(HttpContext context)
        => AspNetExtensions.Extensions.NotFound(context, "Du möchtest auf etwas zugreifen, was ich nicht finden kann!");
}