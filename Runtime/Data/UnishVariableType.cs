namespace RUtil.Debug.Shell
{
    public enum UnishVariableType
    {
        Error = -1, // Failed to resolve its type
        Unit  = 0,  // No value
        String,
        Bool,
        Int,
        Float,
        Vector2,
        Vector3,
        Color,
        Array,
    }
}
