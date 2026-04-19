using System.Linq;
using System.Linq.Dynamic.Core;
using ITStockM.Application.Common.Models;
using System.Text.RegularExpressions;

namespace ITStockM.Services.Utilities;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> items, QueryOptions? query)
    {
        if (query != null)
        {
            if (!string.IsNullOrEmpty(query.Filter))
            {
                // Support filters that are passed as lambda strings (e.g. "i => i.Name.Contains(@0) || i.Adress.Contains(@0)").
                // Dynamic LINQ expects only the expression body (e.g. "Name.Contains(@0) || Adress.Contains(@0)").
                var filter = query.Filter.Trim();
                var arrowIdx = filter.IndexOf("=>", StringComparison.Ordinal);
                if (arrowIdx >= 0)
                {
                    var parameterPart = filter.Substring(0, arrowIdx).Trim();
                    var parameterName = parameterPart.Trim().Trim('(', ')');
                    if (parameterName.Contains(' '))
                    {
                        // If a type was included (rare), keep the last token as the parameter name.
                        parameterName = parameterName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? parameterName;
                    }

                    filter = filter.Substring(arrowIdx + 2).Trim();
                    if (filter.StartsWith("(") && filter.EndsWith(")"))
                    {
                        filter = filter.Substring(1, filter.Length - 2).Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(parameterName))
                    {
                        // Remove all occurrences of "{param}." from the expression body.
                        filter = Regex.Replace(filter, $@"\b{Regex.Escape(parameterName)}\.", string.Empty);
                    }
                }

                items = items.Where(filter, query.FilterParameters ?? Array.Empty<object>());
            }

            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                items = items.OrderBy(query.OrderBy);
            }

            if (query.Skip.HasValue)
            {
                items = items.Skip(query.Skip.Value);
            }

            if (query.Top.HasValue)
            {
                items = items.Take(query.Top.Value);
            }
        }

        return items;
    }
}
