using System;
using System.Collections.Generic;
using System.Text;
using CodeDocumentor.Common.Models;
using CodeDocumentor.Helper;
using CodeDocumentor.Locators;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeDocumentor.Managers
{
    public class GenericCommentManager
    {
        private readonly DocumentationHeaderHelper _documentationHeaderHelper = ServiceLocator.DocumentationHeaderHelper;

        public string ProcessDictionary(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, string stringTemplate, WordMap[] wordMaps)
        {
            var argType1 = returnType.TypeArgumentList.Arguments.First();
            var argType2 = returnType.TypeArgumentList.Arguments.Last();
            var items = new List<string>();
            BuildChildrenGenericArgList(argType2, items, wordMaps); //pluaralizeIdentifierType: false
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
                           .ApplyUserTranslations(wordMaps)
                           .WithPeriod();

            comment = string.Format(stringTemplate, argType1.ApplyUserTranslations(wordMaps), comment);
            if (options.IsRootReturnType)
            {
                //This ensure the return string has correct casing
                comment = comment.ToTitleCase();
            }

            return comment;
        }

        public string ProcessList(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            var argType = returnType.TypeArgumentList.Arguments.First();
            var items = new List<string>();
            BuildChildrenGenericArgList(argType, items, wordMaps);

            var returnName = _documentationHeaderHelper.DetermineSpecificObjectName(returnType, wordMaps, false, true).ToLower();
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
                                .ApplyUserTranslations(wordMaps)
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
                    builder.Append($"{_documentationHeaderHelper.DetermineStartingWord(item.ToString().AsSpan(), options.UseProperCasing)}");
                }
                var newOptions = new ReturnTypeBuilderOptions
                {
                    IsRootReturnType = false,
                    ReturnGenericTypeAsFullString = options.ReturnGenericTypeAsFullString,
                    UseProperCasing = false,
                    TryToIncludeCrefsForReturnTypes = options.TryToIncludeCrefsForReturnTypes,
                    IncludeStartingWordInText = false
                };
                builder.Append($"{commentBuilderCallback.Invoke(item, newOptions)}");
                if (i + 1 < returnType.TypeArgumentList.Arguments.Count)
                {
                    builder.Append(" and ");
                }
            }
            comment = builder.ToString();
            comment = comment.RemovePeriod();
            return comment;
        }

        public string ProcessReadOnlyCollection(GenericNameSyntax returnType, ReturnTypeBuilderOptions options, WordMap[] wordMaps)
        {
            var argType = returnType.TypeArgumentList.Arguments.First();

            var items = new List<string>();
            BuildChildrenGenericArgList(argType, items, wordMaps);
            var returnName = _documentationHeaderHelper.DetermineSpecificObjectName(returnType, wordMaps, false, true).ToLower();
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
                                .ApplyUserTranslations(wordMaps)
                                .WithPeriod();
            if (options.IsRootReturnType)
            {
                comment = comment.ToTitleCase();
            }

            return comment;
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
                TryToIncludeCrefsForReturnTypes = options.TryToIncludeCrefsForReturnTypes,
                IncludeStartingWordInText = false
            };
            var buildComment = commentBuilderCallback.Invoke(firstType, newOptions);
            comment = prefix + buildComment;
            comment = comment.RemovePeriod();
            return comment;
        }

        private static string BuildPrefix(GenericNameSyntax returnType, ReturnTypeBuilderOptions options)
        {
            string prefix;
            var startWord = "";
            if (options.IncludeStartingWordInText)
            {
                if (returnType.IsTask())
                {
                    startWord = "a ";
                }
                else
                if (returnType.IsGenericActionResult())
                {
                    startWord = "an ";
                }
                else
                if (returnType.IsGenericValueTask())
                {
                    startWord = "a ";
                }
            }

            if (options.TryToIncludeCrefsForReturnTypes)
            {
                prefix = $"{startWord}<see cref=\"Task\"/> of type ";
                if (returnType.IsGenericActionResult())
                {
                    prefix = $"{startWord}<see cref=\"ActionResult\"/> of type ";
                }
                if (returnType.IsGenericValueTask())
                {
                    prefix = $"{startWord}<see cref=\"ValueTask\"/> of type ";
                }

                return prefix;
            }

            prefix = $"{startWord}Task of type ";
            if (returnType.IsGenericActionResult())
            {
                prefix = $"{startWord}ActionResult of type ";
            }
            if (returnType.IsGenericValueTask())
            {
                prefix = $"{startWord}ValueTask of type ";
            }
            if (options.UseProperCasing)
            {
                return prefix.ToTitleCase();
            }
            return prefix;
        }

        /// <summary>
        ///  Builds the children generic arg list.
        /// </summary>
        /// <param name="argType"> The arg type. </param>
        /// <param name="items"> The items. </param>
        private void BuildChildrenGenericArgList(TypeSyntax argType, List<string> items, WordMap[] wordMaps)
        {
            if (argType is GenericNameSyntax genericArgType)
            {
                var childArg = genericArgType.TypeArgumentList?.Arguments.FirstOrDefault();
                if (childArg != null)
                {
                    BuildChildrenGenericArgList(childArg, items, wordMaps);
                }
            }
            items.Add(_documentationHeaderHelper.DetermineSpecificObjectName(argType, wordMaps, false, true));
        }
    }
}
