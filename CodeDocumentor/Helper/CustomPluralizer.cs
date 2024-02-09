using System.Linq;
using Pluralize.NET;

namespace CodeDocumentor.Helper
{
    public class CustomPluralizer : PluralizerBase
    {
        //This lets us control some internal collections of Pluralizer.Net
        public void UpsertIrregularRule(string single, string plural)
        {
            if (_irregularSingles.Any(a => a.Key.Equals(single, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                _irregularSingles[single] = plural;
            }
            else
            {
                AddIrregularRule(single.ToLower(), plural);
            }
        }
    }
}
