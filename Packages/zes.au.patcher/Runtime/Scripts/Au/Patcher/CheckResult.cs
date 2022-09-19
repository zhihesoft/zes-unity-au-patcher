namespace Au.Patcher
{
    public enum CheckResult
    {
        None,       // No patch found
        Found,      // patch found
        Reinstall,  // need reinstall
        Failed,     // check failed
    }
}
