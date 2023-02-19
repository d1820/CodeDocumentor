using System.Linq;
using Pluralize.NET;

namespace CodeDocumentor.Helper
{
    public class CustomPluralizer : PluralizerBase
    {
        public void UpsertIrregularRule(string single, string plural)
        {
            if (_irregularSingles.Any(a => a.Key.Equals(single, System.StringComparison.InvariantCultureIgnoreCase)))
            {
                _irregularSingles[single] = plural;
            }
            else
            {
                this.AddIrregularRule(single.ToLower(), plural);
            }
        }
    }
}
