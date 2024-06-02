using System.Numerics;
using FluentValidation;
using Sqids;

namespace MinigolfFriday.Extensions;

public static class SqidsExtensions
{
    public static IRuleBuilder<T, string> ValidSqid<T, TId>(
        this IRuleBuilder<T, string> ruleBuilder,
        SqidsEncoder<TId> sqidsEncoder
    )
        where TId : unmanaged, IBinaryInteger<TId>, IMinMaxValue<TId>
    {
        return ruleBuilder.Must(x => sqidsEncoder.Decode(x).Count == 1).WithMessage("Invalid ID");
    }

    public static T DecodeSingle<T>(this SqidsEncoder<T> sqidsEncoder, ReadOnlySpan<char> id)
        where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
    {
        return sqidsEncoder.Decode(id).Single();
    }

    public static bool TryDecodeSingle<T>(
        this SqidsEncoder<T> sqidsEncoder,
        ReadOnlySpan<char> id,
        out T value
    )
        where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
    {
        var decoded = sqidsEncoder.Decode(id);
        if (decoded.Count == 1)
        {
            value = decoded.Single();
            return true;
        }
        value = default;
        return false;
    }
}
