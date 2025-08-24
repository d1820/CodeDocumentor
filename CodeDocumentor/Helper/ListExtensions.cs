using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CodeDocumentor.Common;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Constructors;
using CodeDocumentor.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Helper
{
    public static class ListExtensions
    {
        public static List<string> TryIncudeReturnType(this List<string> parts, IOptionsService optionsService, TypeSyntax returnType, Action<string> returnTapAction = null)
        {
            if (returnType.ToString() != "void" && (parts.Count == 1 || (parts.Count == 2 && parts.Last() == "asynchronously")))
            {
                //if return type is not a generic type then just force the Todo comment cause what ever we do here will not be a good summary anyway
                if (returnType.GetType() != typeof(GenericNameSyntax))
                {
                    if (optionsService.UseToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { "TODO: Add Summary" };
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
                    TryToIncludeCrefsForReturnTypes = optionsService.TryToIncludeCrefsForReturnTypes,
                    IncludeStartingWordInText = true
                };
                var returnComment = new SingleWordCommentSummaryConstruction(returnType, options, optionsService).Comment;
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
                    if (optionsService.UseToDoCommentsOnSummaryError)
                    {
                        parts = new List<string> { "TODO: Add Summary" };
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
