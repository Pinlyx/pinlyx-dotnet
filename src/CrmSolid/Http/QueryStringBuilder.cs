using System;
using System.Collections.Generic;
using System.Text;

namespace CrmSolid.Http;

/// <summary>Minimal allocation-free-ish query string builder.</summary>
internal sealed class QueryStringBuilder
{
    private readonly List<KeyValuePair<string, string>> _params = new();

    public void Add(string key, string value)
    {
        _params.Add(new KeyValuePair<string, string>(key, value));
    }

    public override string ToString()
    {
        if (_params.Count == 0) return string.Empty;
        var sb = new StringBuilder();
        sb.Append('?');
        for (var i = 0; i < _params.Count; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(_params[i].Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(_params[i].Value));
        }
        return sb.ToString();
    }
}
