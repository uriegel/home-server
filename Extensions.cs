static class Extensions
{
    public static Task NotFound(HttpContext context)
        => AspNetExtensions.Extensions.NotFound(context, "Du möchtest auf etwas zugreifen, was ich nicht finden kann!");

    public static TR Match<T, TR>(this T? t, Func<T, TR> someFunc, Func<TR> noneFunc)
        => t != null
            ? someFunc(t)
            : noneFunc();
}