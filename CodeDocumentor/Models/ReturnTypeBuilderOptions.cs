namespace CodeDocumentor.Helper
{
    public class ReturnTypeBuilderOptions
    {
        public bool ForcePredefinedTypeEvaluation { get; set; }
        public bool ReturnGenericTypeAsFullString { get; set; }
        public bool IsRootReturnType { get; set; } = true;
        public bool UseProperCasing { get; set; }
        public bool BuildWithAndPrefixForTaskTypes { get; set; }
    }
}
