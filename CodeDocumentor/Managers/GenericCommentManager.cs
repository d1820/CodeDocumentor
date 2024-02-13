using System;
using System.Collections.Generic;
using System.Text;
using CodeDocumentor.Helper;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Managers
{
    public class GenericCommentManager
    {
        public string ProcessDictionary(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, string stringTemplate)
        {
            var argType1 = returnType.TypeArgumentList.Arguments.First();
            var argType2 = returnType.TypeArgumentList.Arguments.Last();
            var items = new List<string>();
            BuildChildrenGenericArgList(argType2, items); //pluaralizeIdentifierType: false
            items.Reverse();

            var comment = items.ToLowerParts(true)
                           .Tap((parts) =>
                           {
                               if (parts.Count > 1)
                               {
                                   parts.PluaralizeLastWord();
                               }
                               if (parts.Count > 2)
                               {
                                   for (var i = 1; i < parts.Count - 1; i++)
                                   {
                                       parts[i] = "a " + parts[i];
                                   }
                               }
                           })
                           .JoinToString(" of ")
                           .ApplyUserTranslations()
                           .WithPeriod();

            comment = string.Format(stringTemplate, argType1.ApplyUserTranslations(), comment);
            if (options.IsRootReturnType)
            {
                //This ensure the return string has correct casing
                comment = comment.ToTitleCase();
            }

            return comment;
        }

        public string ProcessList(GenericNameSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var argType = returnType.TypeArgumentList.Arguments.First();
            var items = new List<string>();
            BuildChildrenGenericArgList(argType, items);

            var returnName = DocumentationHeaderHelper.DetermineSpecificObjectName(returnType, false, true).ToLower();
            items.Add(returnName);
            items.Reverse();

            var comment = items.ToLowerParts(true)
                                .PluaralizeLastWord()
                                .Tap((parts) =>
                                {
                                    for (var i = 0; i < parts.Count; i++)
                                    {
                                        parts[i] = parts[i].Replace(returnName, $"a {returnName}");
                                    }
                                })
                                .JoinToString(" of ")
                                .ApplyUserTranslations()
                                .WithPeriod();
            if (options.IsRootReturnType)
            {
                comment = comment.ToTitleCase();
            }

            return comment;
        }

        public string ProcessReadOnlyCollection(GenericNameSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var argType = returnType.TypeArgumentList.Arguments.First();

            var items = new List<string>();
            BuildChildrenGenericArgList(argType, items);
            var returnName = DocumentationHeaderHelper.DetermineSpecificObjectName(returnType, false, true).ToLower();
            items.Add(returnName);
            items.Reverse();

            var comment = items.ToLowerParts(true)
                                .PluaralizeLastWord()
                                .Tap((parts) =>
                                {
                                    for (var i = 0; i < parts.Count; i++)
                                    {
                                        parts[i] = parts[i].Replace(returnName, $"a {returnName}");
                                    }
                                })
                                .JoinToString(" of ")
                                .ApplyUserTranslations()
                                .WithPeriod();
            if (options.IsRootReturnType)
            {
                comment = comment.ToTitleCase();
            }

            return comment;
        }

        public string ProcessMultiTypeTaskArguments(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, Func<TypeSyntax, ReturnTypeBuilderOptions, string> commentBuilderCallback)
        {
            string comment;
            //This should be impossible, but will handle just in case
            var builder = new StringBuilder();
            for (var i = 0; i < returnType.TypeArgumentList.Arguments.Count; i++)
            {
                var item = returnType.TypeArgumentList.Arguments[i];
                if (i > 0)
                {
                    builder.Append($"{DocumentationHeaderHelper.DetermineStartingWord(item.ToString().AsSpan(), options.UseProperCasing)}");
                }
                var newOptions = new ReturnTypeBuilderOptions
                {
                    IsRootReturnType = false,
                    ReturnGenericTypeAsFullString = options.ReturnGenericTypeAsFullString,
                    UseProperCasing = false,
                    ForcePredefinedTypeEvaluation = true,
                    BuildWithPeriodAndPrefixForTaskTypes = options.BuildWithPeriodAndPrefixForTaskTypes,
                    TryToIncludeCrefsForReturnTypes = options.TryToIncludeCrefsForReturnTypes,
                    IncludeReturnStatementInGeneralComments = false
                };
                builder.Append($"{commentBuilderCallback.Invoke(item, newOptions)}");
                if (i + 1 < returnType.TypeArgumentList.Arguments.Count)
                {
                    builder.Append(" and ");
                }
            }
            comment = builder.ToString();
            comment = comment.RemovePeriod();
            return !options.BuildWithPeriodAndPrefixForTaskTypes ? comment.WithPeriod() : comment;
        }

        public string ProcessSingleTypeTaskArguments(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, Func<TypeSyntax, ReturnTypeBuilderOptions, string> commentBuilderCallback)
        {
            var prefix = BuildPrefix(returnType, options);
            string comment;
            var firstType = returnType.TypeArgumentList.Arguments.First();
            if (firstType.IsReadOnlyCollection() || firstType.IsDictionary() || firstType.IsList())
            {
                prefix = prefix.Replace("type ", "");
            }

            var newOptions = new ReturnTypeBuilderOptions
            {
                IsRootReturnType = false,
                ReturnGenericTypeAsFullString = options.ReturnGenericTypeAsFullString,
                UseProperCasing = false,
                ForcePredefinedTypeEvaluation = true, //maybe??
                BuildWithPeriodAndPrefixForTaskTypes = options.BuildWithPeriodAndPrefixForTaskTypes,
                TryToIncludeCrefsForReturnTypes = options.TryToIncludeCrefsForReturnTypes,
                IncludeReturnStatementInGeneralComments = false
            };
            var buildComment = commentBuilderCallback.Invoke(firstType, newOptions);
            comment = prefix + buildComment;
            comment = comment.RemovePeriod();
            return !options.BuildWithPeriodAndPrefixForTaskTypes ? comment.WithPeriod() : comment;
        }

        /// <summary>
        ///  Builds the children generic arg list.
        /// </summary>
        /// <param name="argType"> The arg type. </param>
        /// <param name="items"> The items. </param>
        private void BuildChildrenGenericArgList(TypeSyntax argType, List<string> items)
        {
            if (argType is GenericNameSyntax genericArgType)
            {
                var childArg = genericArgType.TypeArgumentList?.Arguments.FirstOrDefault();
                if (childArg != null)
                {
                    BuildChildrenGenericArgList(childArg, items);
                }
            }
            items.Add(DocumentationHeaderHelper.DetermineSpecificObjectName(argType, false, true));
        }

        private static string BuildPrefix(GenericNameSyntax returnType, ReturnTypeBuilderOptions options)
        {
            var startingPrefix = "returns";
            if (options.BuildWithPeriodAndPrefixForTaskTypes)
            {
                startingPrefix = "and return";
            }
            string prefix;
            if (options.TryToIncludeCrefsForReturnTypes)
            {
                prefix = $"{startingPrefix} a <see cref=\"Task\"/> of type ";
                if (returnType.IsGenericActionResult())
                {
                    prefix = $"{startingPrefix} an <see cref=\"ActionResult\"/> of type ";
                }
                if (returnType.IsGenericValueTask())
                {
                    prefix = $"{startingPrefix} a <see cref=\"ValueTask\"/> of type ";
                }

                return prefix;
            }

            prefix = $"{startingPrefix} a Task of type ";
            if (returnType.IsGenericActionResult())
            {
                prefix = $"{startingPrefix} an ActionResult of type ";
            }
            if (returnType.IsGenericValueTask())
            {
                prefix = $"{startingPrefix} a ValueTask of type ";
            }

            return prefix;
        }
    }
}
