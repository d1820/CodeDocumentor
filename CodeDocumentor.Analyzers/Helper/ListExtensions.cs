using System;
using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Analyzers.Constructors;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Analyzers.Helper
{
    public static class ListExtensions
    {
        public static List<string> TryIncludeReturnType(this List<string> parts,
            bool useToDoCommentsOnSummaryError,
            bool tryToIncludeCrefsForReturnTypes,
            WordMap[] wordMaps,
            TypeSyntax returnType, Action<string> returnTapAction = null)
        {
            if (returnType.ToString() != "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                //if return type is not a generic type then just force the Todo comment cause what ever we do here will not be a good summary anyway
                if (returnType.GetType() != typeof(GenericNameSyntax))
                {
                    if (useToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { Constants.TODO };
                    }
                    else
                    {
                        parts.Clear();
                    }
                    return parts;
                }

                var options = new ReturnTypeBuilderOptions
                {
                    //ReturnBuildType = ReturnBuildType.SummaryXmlElement,
                    UseProperCasing = false,
                    TryToIncludeCrefsForReturnTypes = tryToIncludeCrefsForReturnTypes,
                    IncludeStartingWordInText = true
                };
                var returnComment = new SingleWordCommentSummaryConstruction(returnType, options, wordMaps).Comment;
                returnTapAction?.Invoke(returnComment);
                if (!string.IsNullOrEmpty(returnComment))
                {
                    if (!returnComment.StartsWith_A_An_And())
                    {
                        parts.Insert(1, "the");
                        parts.Insert(2, returnComment);
                    }
                    else
                    {
                        parts.Insert(1, returnComment);
                    }
                }
                else
                {
                    if (useToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { Constants.TODO };
                    }
                }
            }
            else
            {
                returnTapAction?.Invoke(null);
            }
            return parts;
        }
    }
}
