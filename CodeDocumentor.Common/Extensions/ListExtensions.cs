using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Helpers;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis;

namespace CodeDocumentor.Common.Extensions
{
    public static class ListExtensions
    {
        public static List<string> AddCustomPart(this List<string> parts, string part = null, int idx = -1)
        {
            if (part is null)
            {
                return parts;
            }
            if (idx == -1)
            {
                parts.Add(part);
                return parts;
            }
            parts.Insert(idx, part);
            return parts;
        }

        public static string JoinToString(this List<string> parts, string delimiter = " ")
        {
            return $"{string.Join(delimiter, parts)}";
        }

        public static List<string> PluaralizeLastWord(this List<string> parts)
        {
            var lastIdx = parts.Count - 1;
            parts[lastIdx] = Pluralizer.ForcePluralization(parts[lastIdx]);
            return parts;
        }

        public static List<string> Tap(this List<string> parts, Action<List<string>> tapAction)
        {
            tapAction?.Invoke(parts);
            return parts;
        }

        public static List<string> ToLowerParts(this List<string> parts, bool forceFirstCharToLower = false)
        {
            var i = forceFirstCharToLower ||
                    (
                        parts.Count > 0 &&
                        !parts[0].Equals("The", StringComparison.InvariantCultureIgnoreCase) &&
                        !parts[0].Equals("If true,", StringComparison.InvariantCultureIgnoreCase) &&
                        !parts[0].IsVerb() //if the first word is a verb we are not adding The anyway so we need to leave it Pascal
                    )
                ? 0 : 1;

            parts.SwapXmlTokens((part) =>
            {
                if (!part.All(a => char.IsUpper(a)))
                {
                    part = part.ToLower();
                }
                return part;
            }, i);

            //First letter is always caps unless it was forced lower
            if (!forceFirstCharToLower && parts.Count > 0 && char.IsLower(parts[0], 0))
            {
                parts[0] = parts[0].ToTitleCase();
            }
            return parts;
        }

        public static List<string> TranslateParts(this List<string> parts, WordMap[] wordMaps)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                var nextWord = i + 1 < parts.Count ? parts[i + 1] : null;
                var userMaps = wordMaps ?? Array.Empty<WordMap>();
                foreach (var wordMap in Constants.INTERNAL_WORD_MAPS)
                {
                    if (!CanEvaluateWordMap(wordMap, i))
                    {
                        continue;
                    }
                    //dont run an internal word map if the user has one for the same thing
                    if (!userMaps.Any(a => a.Word == wordMap.Word))
                    {
                        var wordToLookFor = string.Format(Constants.WORD_MATCH_REGEX_TEMPLATE, wordMap.Word);
                        parts[i] = Regex.Replace(parts[i], wordToLookFor, wordMap.GetTranslation(nextWord));
                    }
                }
            }
            return parts;
        }

        private static bool CanEvaluateWordMap(WordMap wordMap, int partIdx)
        {
            return wordMap.Word != "Is" || partIdx == 0;
        }

        public static List<string> TryInsertTheWord(this List<string> parts, Action<List<string>> customInsertCallback = null)
        {
            if (customInsertCallback != null)
            {
                customInsertCallback.Invoke(parts);
            }
            else if (parts.Count > 0)
            {
                var checkWord = parts[0].GetWordFirstPart();
                var skipThe = checkWord.IsVerb();
                var addTheAnyway = Constants.ADD_THE_ANYWAY_LIST.Any(w => w.Equals(parts[0], StringComparison.InvariantCultureIgnoreCase));
                if (!skipThe || addTheAnyway)
                {
                    if (!parts[0].Equals("the", StringComparison.InvariantCultureIgnoreCase))
                    {
                        parts.Insert(0, "The");
                    }
                    else
                    {
                        parts[0] = "The"; //force casing
                    }
                }
            }
            return parts;
        }

        public static List<string> AddPropertyBooleanPart(this List<string> parts)
        {
            if (parts.Count > 0)
            {
                var booleanPart = " a value indicating whether to";
                if (parts[0].IsPastTense() || parts[0].IsVerb())
                {
                    booleanPart = "a value indicating whether";
                }

                //is messes up readability. Lets remove it. ex) IsEnabledForDays
                var isTwoLettweWord = parts[0].IsTwoLetterPropertyExclusionVerb();//we only care if forst word is relavent
                if (isTwoLettweWord)
                {
                    parts.Remove(parts[0]);
                }
                parts.Insert(0, booleanPart);
            }

            return parts;
        }

        public static List<string> HandleAsyncKeyword(this List<string> parts, bool excludeAsyncSuffix)
        {
            if (excludeAsyncSuffix && parts.Last().IndexOf("async", StringComparison.OrdinalIgnoreCase) > -1)
            {
                parts.Remove(parts.Last());
            }
            var idx = parts.FindIndex(f => f.Equals("async", StringComparison.OrdinalIgnoreCase));
            if (idx > -1)
            {
                parts[idx] = "asynchronously";
            }
            return parts;
        }

        public static List<string> TryAddTodoSummary(this List<string> parts, string returnType, bool useToDoCommentsOnSummaryError)
        {
            if (IsReturnVoidAndOnePart(parts, returnType) || IsOnePart(parts))
            {
                if (useToDoCommentsOnSummaryError)
                {
                    parts = new List<string> { Constants.TODO};
                }
                else
                {
                    parts = new List<string>();
                }
            }
            return parts;
        }

        private static bool IsReturnVoidAndOnePart(List<string> parts, string returnType) {
            return returnType == "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously"));
        }

        private static bool IsOnePart(List<string> parts)
        {
            return (parts.Count <= 1 || (parts.Count == 2 && parts.Last() == "asynchronously"));
        }

        public static List<string> TryPluarizeFirstWord(this List<string> parts)
        {
            if (parts.Count > 0)
            {
                if (parts.Count >= 2)
                {
                    parts[0] = Pluralizer.Pluralize(parts[0], parts[1]);
                }
                else
                {
                    parts[0] = Pluralizer.Pluralize(parts[0]);
                }
            }
            return parts;
        }
    }
}
