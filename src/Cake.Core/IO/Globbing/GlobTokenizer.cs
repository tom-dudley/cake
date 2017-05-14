// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cake.Core.IO.Globbing
{
    internal sealed class GlobTokenizer
    {
        private readonly string _pattern;
        private string _remainingPattern;
        private Dictionary<string, GlobTokenKind> tokenKindChars = new Dictionary<string, GlobTokenKind>();
        private Lazy<Queue<GlobToken>> tokens;

        public GlobTokenizer(string pattern)
        {
            _pattern = pattern;
            tokens = new Lazy<Queue<GlobToken>>(() => QueueTokens());

            tokenKindChars.Add("?", GlobTokenKind.CharacterWildcard);
            tokenKindChars.Add("*", GlobTokenKind.Wildcard);
            tokenKindChars.Add("**", GlobTokenKind.DirectoryWildcard);
            tokenKindChars.Add("/", GlobTokenKind.PathSeparator);
            tokenKindChars.Add(@"\", GlobTokenKind.PathSeparator);
            tokenKindChars.Add(":", GlobTokenKind.WindowsRoot);
            tokenKindChars.Add("\0", GlobTokenKind.Current);
            tokenKindChars.Add(".", GlobTokenKind.Current);
            tokenKindChars.Add("./", GlobTokenKind.Current);
            tokenKindChars.Add("..", GlobTokenKind.Parent);
        }

        /// <summary>
        /// Gets the next token from the pattern.
        /// </summary>
        public GlobToken Scan()
        {
            return tokens.Value.Dequeue();
        }

        /// <summary>
        /// Peeks the next token from the pattern.
        /// </summary>
        public GlobToken Peek()
        {
            return tokens.Value.Peek();
        }

        /// <summary>
        /// Loads the tokens into the token queue.
        /// </summary>
        private Queue<GlobToken> QueueTokens()
        {
            var tokenQueue = new Queue<GlobToken>();
            GlobTokenKind tokenKind;

            while (_remainingPattern.Length > 0)
            {
                tokenKind = GetGlobTokenKindAndTrimRemainingPattern();

                if (tokenKind == GlobTokenKind.Identifier)
                {
                    while (tokenKind == GlobTokenKind.Identifier)
                    {
                        tokenKind = GetGlobTokenKindAndTrimRemainingPattern();
                    }

                    tokenQueue.Enqueue(new GlobToken(GlobTokenKind.Identifier, string.Empty));
                }

                tokenQueue.Enqueue(new GlobToken(tokenKind, string.Empty));
            }

            return tokenQueue;
        }

        /// <summary>
        /// Searches the private dictionary for a token matching the current (and future) character position(s).
        /// Performs a greedy match of the keys in the dictionary against the remaining pattern.
        /// </summary>
        /// <returns>The GlobTokenKind associated with the mathing entry, or GlobTokenKind.Identifier if none was found. </returns>
        private GlobTokenKind GetGlobTokenKindAndTrimRemainingPattern()
        {
            // Suppose the remaining pattern is 'abc' and the dictionary has keys 'a', 'ab', 'abc', 'ac' and 'b'.
            // First we want all the keys starting with 'a', i.e. 'a', 'ab', 'abc' and 'ac'
            // Then we want the greediest match possible

            int numberOfCharsToRemove;
            GlobTokenKind tokenKind;

            var matches = tokenKindChars.Where(pair => _remainingPattern.IndexOf(pair.Key) == 0);
            var greediestMatch = matches.OrderByDescending(pair => pair.Key.Length).FirstOrDefault();

            if (greediestMatch.Key is null)
            {
                tokenKind = GlobTokenKind.Identifier;
                numberOfCharsToRemove = 1;
            }
            else
            {
                tokenKind = greediestMatch.Value;
                numberOfCharsToRemove = greediestMatch.Key.Length;
            }

            _remainingPattern.Substring(numberOfCharsToRemove);
            return tokenKind;
        }
    }
}