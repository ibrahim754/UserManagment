namespace UserManagement.Extensions
{
    public static class PermissionExtension
    {
        public static string ToPermissionString(this Enum permission)
        {
            string typeName = permission.GetType().Name;
            string permissionName = permission.ToString();
            return $"Permissions.{typeName}.{permissionName}";
        }

    }
}
