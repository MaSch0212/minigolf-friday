using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MinigolfFriday.Common;

public static class DbUpdateBuilder
{
    public static DbUpdateBuilder<T> Create<T>(IQueryable<T> query)
    {
        return new DbUpdateBuilder<T>(query);
    }
}

public class DbUpdateBuilder<T>(IQueryable<T> query)
{
    private Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> _expression = sett => sett;

    public DbUpdateBuilder<T> With(
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setter
    )
    {
        var call = (MethodCallExpression)setter.Body;
        _expression = Expression.Lambda<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>(
            Expression.Call(_expression.Body, call.Method, call.Arguments),
            _expression.Parameters
        );
        return this;
    }

    public async Task<int> ExecuteAsync(CancellationToken cancellation = default)
    {
        return await query.ExecuteUpdateAsync(_expression, cancellation);
    }
}
