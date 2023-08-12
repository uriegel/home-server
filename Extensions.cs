using LinqTools;

static class Extensions
{
    public static Task NotFound(HttpContext context, string notFound = "Resource not found")
        => context
                .SideEffect(c => c.Response.StatusCode = 404)
                .SideEffect(c => c.Response.ContentType = "text/plain; charset=utf-8")
                .Response.WriteAsync(notFound);

    public static TResult With<T, R, TResult>(this T t, Func<T, R> selector, Func<R, TResult> resultSelector)
        => resultSelector(selector(t));

    public static Task NotFound(HttpContext context)
        => NotFound(context, "Du möchtest auf etwas zugreifen, was ich nicht finden kann!");

    public static string ReadAllTextFromFilePath(this string path)
        => new StreamReader(File.OpenRead(path))
            .Use(f => f.ReadToEnd());

    public static Option<TResult> Choose<TResult, T>(this T t, params SwitchType<T, TResult>[] switches)
        where TResult : notnull
        => switches
            .FirstOrNone(s => s.Predicate(t))
            .Select(s => s.Selector(t));

    public static Option<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
        where T : notnull
            => enumerable
                .FirstOrDefault()
                .FromNullable();

    public static Option<T> FirstOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        where T : notnull
            => enumerable
                .FirstOrDefault(predicate)
                .FromNullable();

   public record SwitchType<T, TResult>(Predicate<T> Predicate, Func<T, TResult> Selector)
    {
        public static SwitchType<T, TResult> Switch(Predicate<T> predicate, Func<T, TResult> selector)
            => new(predicate, selector);
        public static SwitchType<T, TResult> Default(Func<T, TResult> selector)
            => new(_ => true, selector);
    };
}