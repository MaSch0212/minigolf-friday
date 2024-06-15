using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MinigolfFriday.Domain.Options;
using Sqids;

namespace MinigolfFriday.Host.Services;

[GenerateAutoInterface]
public class IdService : IIdService
{
    private const string ALPHABET =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public SqidsEncoder<long> User { get; }
    public SqidsEncoder<long> Map { get; }
    public SqidsEncoder<long> Event { get; }
    public SqidsEncoder<long> EventTimeslot { get; }
    public SqidsEncoder<long> EventInstance { get; }
    public SqidsEncoder<long> Preconfiguration { get; }

    public IdService(IOptions<IdOptions> idOptions)
    {
        var seed = BitConverter.ToInt32(
            SHA1.HashData(Encoding.UTF8.GetBytes(idOptions.Value.Seed ?? string.Empty))
        );
        var rng = new Random(seed.GetHashCode());

        User = GetIdEncoder(rng);
        Map = GetIdEncoder(rng);
        Event = GetIdEncoder(rng);
        EventTimeslot = GetIdEncoder(rng);
        EventInstance = GetIdEncoder(rng);
        Preconfiguration = GetIdEncoder(rng);
    }

    private static SqidsEncoder<long> GetIdEncoder(Random rng)
    {
        var chars = ALPHABET.ToCharArray();
        rng.Shuffle(chars);
        return new SqidsEncoder<long>(new() { Alphabet = new(chars), MinLength = 5 });
    }
}
