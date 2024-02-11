namespace CodeDocumentor.Helper
{
    public class ReturnTypeBuilderOptions
    {
        public bool BuildWithPeriodAndPrefixForTaskTypes { get; set; }

        public bool ForcePredefinedTypeEvaluation { get; set; }

        public bool IsRootReturnType { get; set; } = true;

        public bool ReturnGenericTypeAsFullString { get; set; }

        public bool UseProperCasing { get; set; }

        public bool TryToIncludeCrefsForReturnTypes { get; set; }
    }
}
