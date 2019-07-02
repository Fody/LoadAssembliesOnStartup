using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(DiffReporter))]

public static class TargetFrameworkResolver
{
    public const string Current =

#if NET45
            "NET45"
#elif NET46
            "NET46"
#elif NET47
            "NET47"
#else
            "Unknown"
#endif
        ;
}
