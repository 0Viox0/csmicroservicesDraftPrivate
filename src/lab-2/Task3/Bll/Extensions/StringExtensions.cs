namespace Task3.Bll.Extensions;

public static class StringExtensions
{
    public static string FromPascalToSnakeCase(this string text)
    {
        return string.Concat(
            text.Select((ch, index) => index > 0 && char.IsUpper(ch)
                ? "_" + ch.ToString().ToLowerInvariant()
                : ch.ToString().ToLowerInvariant()));
    }

    public static string FromSnakeToPascalCase(this string text)
    {
        return string.Concat(
            text.Split('_')
                .Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
    }
}