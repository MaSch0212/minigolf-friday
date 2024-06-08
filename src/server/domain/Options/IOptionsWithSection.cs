namespace MinigolfFriday.Domain.Options;

public interface IOptionsWithSection
{
    static abstract string SectionPath { get; }
    static abstract Type? ValidatorType { get; }
}
