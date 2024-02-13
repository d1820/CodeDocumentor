namespace CodeDocumentor.Helper
{
    public enum ReturnBuildType
    {
        ReturnXmlElement,
        SummaryXmlElement
    }

    public class ReturnTypeBuilderOptions
    {
        public bool IncludeStartingWordInText { get; set; }

        //This controls if TitleCase needs to be applied to the return text
        public bool IsRootReturnType { get; set; } = true;

        public ReturnBuildType ReturnBuildType { get; set; }

        //This controls if we just return the type as a string and not process it
        public bool ReturnGenericTypeAsFullString { get; set; }

        //This controls if we inject <see cref> tags into the return text
        public bool TryToIncludeCrefsForReturnTypes { get; set; }

        //public bool IncludeReturnStatementInGeneralComments { get; set; }
        public bool UseProperCasing { get; set; }

        public ReturnTypeBuilderOptions Clone()
        {
            var clone = new ReturnTypeBuilderOptions();
            clone.ReturnGenericTypeAsFullString = ReturnGenericTypeAsFullString;
            clone.TryToIncludeCrefsForReturnTypes = TryToIncludeCrefsForReturnTypes;
            clone.IsRootReturnType = IsRootReturnType;
            clone.ReturnBuildType = ReturnBuildType;
            clone.IncludeStartingWordInText = IncludeStartingWordInText;
            clone.UseProperCasing = UseProperCasing;
            return clone;
        }
    }
}
