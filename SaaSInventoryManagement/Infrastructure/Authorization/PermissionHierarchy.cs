namespace SaaSInventoryManagement.Infrastructure.Authorization
{
    public static class PermissionHierarchy
    {

        public static HashSet<string> ExpandWithImplied(
            IEnumerable<string> requestedKeys,
            IEnumerable<string> activeCatalogKeys)
        {
            var catalog = new HashSet<string>(activeCatalogKeys, StringComparer.Ordinal);
            var expanded = new HashSet<string>(requestedKeys, StringComparer.Ordinal);

            foreach (var key in expanded.ToList())
            {
                var impliedViewKey = GetImpliedViewKey(key);
                if (impliedViewKey is not null && catalog.Contains(impliedViewKey))
                    expanded.Add(impliedViewKey);
            }

            return expanded;
        }


        private static string? GetImpliedViewKey(string key)
        {
            var lastDot = key.LastIndexOf('.');
            if (lastDot < 0)
                return null;

            var module = key[..lastDot];
            var action = key[(lastDot + 1)..];

            if (action == "view")
                return null;

            return $"{module}.view";
        }
    }
}
