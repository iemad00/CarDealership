namespace CarDealership.Authorization;

public static class PolicyNames
{
    private const string Prefix = "Permission";

    public static string For(string resource, string action)
    {
        return $"{Prefix}:{resource}:{action}";
    }

    public static bool TryParse(string policyName, out string resource, out string action)
    {
        resource = string.Empty;
        action = string.Empty;

        if (string.IsNullOrWhiteSpace(policyName))
        {
            return false;
        }

        var parts = policyName.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3 && string.Equals(parts[0], Prefix, StringComparison.OrdinalIgnoreCase))
        {
            resource = parts[1];
            action = parts[2];
            return true;
        }

        return false;
    }
}


