using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml;

namespace Microsoft.SharePointIntegration
{
    /// <summary>
    /// Static class with general utilities for manipulating strings.
    /// </summary>
    public static partial class TextUtilities
	{
        public static byte[] GetBytesFromString(String str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static char CharAt(this String str, int index)
        {
            return str[index];
        }

        public static String GetSimplifiedString(String name)
        {
            String result = String.Empty;

            foreach (char c in name)
            {
                if (Char.IsDigit(c) || Char.IsLetter(c))
                {
                    result += c;
                }
            }

            return result;
        }

        public static String Normalize(String name)
        {
            String normalizedName= String.Empty;

            foreach (Char c in name)
            {
                if (Char.IsLetterOrDigit(c))
                {
                    normalizedName += c;
                }
            }

            return normalizedName;
        }

        public static IEnumerable<int> GetWordInstances(String source, String word, StringComparison comparisonType)
        {
            List<int> instances = null;

            int i = source.IndexOf(word, comparisonType);

            while (i >= 0)
            {
                bool isWord = true;

                if (i > 0)
                {
                    isWord = TextUtilities.IsBreakerChar(source[i - 1]);
                }

                if (isWord && i + word.Length < source.Length - 1)
                {
                    isWord = TextUtilities.IsBreakerChar(source[i + word.Length]);
                }

                if (isWord)
                {
                    if (instances == null)
                    {
                        instances = new List<int>();
                    }

                    instances.Add(i);
                }

                i = source.IndexOf(word, i + 1, comparisonType);
            }

            return instances;
        }

        public static bool ContainsWord(String source, String word, StringComparison comparisonType)
        {
            int i = source.IndexOf(word, comparisonType);

            while (i >= 0)
            {
                bool isWord = true;

                if (i > 0)
                {
                    isWord = TextUtilities.IsBreakerChar(source[i - 1]);
                }

                if (isWord && i + word.Length < source.Length - 1)
                {
                    isWord = TextUtilities.IsBreakerChar(source[i + word.Length]);
                }

                if (isWord)
                {
                    return true;
                }

                i = source.IndexOf(word, i + 1, comparisonType);
            }

            return false;
        }

        public static bool IsInteger(String text)
        {
            int result = 0;

            return Int32.TryParse(text, out result);
        }

        public static bool IsBreakerChar(char cand)
        {
            return Char.IsControl(cand) || Char.IsPunctuation(cand) || Char.IsSeparator(cand);
        }

        public static String RemoveTextToLeftOfNewLine(String text, int index)
        {
            if (index > text.Length)
            {
                index = text.Length;
            }

            int left = -1;
            int nextChar = text.LastIndexOf("\r", index);

            if (nextChar > left)
            {
                left = nextChar;
            }

            nextChar = text.LastIndexOf("\n", index);

            if (nextChar > left)
            {
                left = nextChar;
            }

            nextChar = text.LastIndexOf("\t", index);

            if (nextChar > left)
            {
                left = nextChar;
            }

            if (left > 0)
            {
                text = text.Substring(left +1, text.Length - (left + 1));
            }

            return text;
        }

        public static String RemoveTextToRightOfNewLine(String text, int index)
        {
            if (index < 0)
            {
                index = 0;
            }

            int right = text.Length + 1;
            int nextChar = text.IndexOf("\r", index);

            if (nextChar < right && nextChar >= 0)
            {
                right = nextChar;
            }

            nextChar = text.IndexOf("\n", index);

            if (nextChar < right && nextChar >= 0)
            {
                right = nextChar;
            }

            nextChar = text.IndexOf("\t", index);

            if (nextChar < right && nextChar >= 0)
            {
                right = nextChar;
            }

            if (right < text.Length)
            {
                text = text.Substring(0, right);
            }

            return text;
        }

		public static void ConvertAllTagsToSingleton(ref String source, String tagName)
		{
			Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
			Match matchSpecificTagStart = regexSpecificTagStart.Match(source);

			while (matchSpecificTagStart.Success)
			{
				int i = matchSpecificTagStart.Index;
				int nextTagStart= source.IndexOf("<", i + 1);
				int nextStartTagEnd= source.IndexOf(">", i + 1);
				int nextSingletonTagEnd = source.IndexOf("/>", i + 1);

				if ( 	nextSingletonTagEnd < 0 || 		// make sure this tag is not a singleton
					(nextTagStart > i && nextSingletonTagEnd > nextTagStart) )
				{
					source = source.Substring(0, nextStartTagEnd) + "/" + source.Substring(nextStartTagEnd, source.Length - nextStartTagEnd);
				}
				else
					matchSpecificTagStart = regexSpecificTagStart.Match(source, nextSingletonTagEnd);
			}

			source = source.Replace("</" + tagName + ">", "");
		}

		public static void EnsureExplicitEndTag(ref String source, String tagName, String[] pseudoEndTags)
		{
			Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
			Match matchSpecificTagStart = regexSpecificTagStart.Match(source);

			while (matchSpecificTagStart.Success)
			{
				int i = matchSpecificTagStart.Index;
				int iNextTagStart= source.IndexOf("<", i + 1);
				int iNextStartTagEnd= source.IndexOf(">", i + 1);
				int iNextSingletonTagEnd = source.IndexOf("/>", i + 1);

				if ( 	iNextSingletonTagEnd < 0 || 		// make sure this tag is not a singleton
					(iNextTagStart > i && iNextSingletonTagEnd > iNextTagStart) )
				{
					int iEndTag = source.IndexOf("</" + tagName + ">", i + 1);

					int iNextPseudoEnd = -1;
					
					foreach (String sPseudoEnd in pseudoEndTags)
					{
						int iPseudoEnd = source.IndexOf("<" + sPseudoEnd, i + 1);

						if (iPseudoEnd > 0 && (iNextPseudoEnd < 0 || iPseudoEnd < iNextPseudoEnd))
						{
							iNextPseudoEnd = iPseudoEnd;
						}
					}

					if ( 	(iEndTag < 0 && iNextPseudoEnd > 0) ||
						(iEndTag > 0 && iNextPseudoEnd > 0 && iEndTag >  iNextPseudoEnd) )
					{
						source = source.Substring(0, iNextPseudoEnd) + "</" + tagName + ">" + source.Substring(iNextPseudoEnd, source.Length - iNextPseudoEnd);
					}

					matchSpecificTagStart = regexSpecificTagStart.Match(source, i + 1);
				}
				else
					matchSpecificTagStart = regexSpecificTagStart.Match(source, iNextSingletonTagEnd);
			}

		}
		
		public static void MakeTagNameUnderneathTagFirst(ref String source, String underneathTagName, String tagName)
		{
			Regex regexSpecificTagStart = new Regex("<" + underneathTagName + "(\\s|>)");
			Match matchSpecificTagStart = regexSpecificTagStart.Match(source);


			while (matchSpecificTagStart.Success)
			{
				int i = matchSpecificTagStart.Index;
				int iNextTagStart= source.IndexOf("<", i + 1);
				int iNextStartTagEnd= source.IndexOf(">", i + 1);
				int iNextSingletonTagEnd = source.IndexOf("/>", i + 1);
				int iNextComplexTagEnd = GetEndOfTag(source, underneathTagName, i);
				
				if ( 	iNextSingletonTagEnd < 0 || 		// make sure this tag is not a singleton
					(iNextTagStart > i && iNextSingletonTagEnd > iNextTagStart) )
				{
					iNextStartTagEnd++;
					int iLast = source.LastIndexOf("</", iNextComplexTagEnd);
					
					String sTag = source.Substring(iNextStartTagEnd, iLast - iNextStartTagEnd );

					String sDiscard= StripTagsOfTypeAndTheirContents(ref sTag, tagName, true);

					MakeTagNameUnderneathTagFirst(ref sTag, underneathTagName, tagName);
					
					sTag = sDiscard + sTag;

					source = source.Substring(0, iNextStartTagEnd) + sTag + source.Substring(iLast, source.Length - iLast);
					matchSpecificTagStart = regexSpecificTagStart.Match(source, iNextComplexTagEnd);
				}
				else
					matchSpecificTagStart = regexSpecificTagStart.Match(source, iNextSingletonTagEnd);
				
			}
		}

		public static int GetMatchingEndCharacter(String source, String starterTags, String endTags, int startIndex)
		{
			Regex regexEnd = new Regex(endTags);
			Regex regexStart = new Regex(starterTags);


			// really find the start of the tag (should match startIndex)						
			Match matchStart = regexStart.Match(source, startIndex);
			Match matchEnd = regexEnd.Match(source, startIndex);

			int iStartTags = 1;

			while (iStartTags > 0 && matchEnd.Success)
			{
				 if (		matchEnd.Index < matchStart.Index ||
						!matchStart.Success)
				{
					iStartTags--;
					if (iStartTags > 0)
						matchEnd = regexEnd.Match(source, matchEnd.Index + 1);
				}
				// is the next thing a tag start?
				else  if (	matchStart.Success &&
						matchStart.Index < matchEnd.Index)
				{
					iStartTags++;

					matchStart = regexStart.Match(source, matchStart.Index + 2);
				}
				else
				{
					Debug.Assert(false, "Warning: malformed tags detected in source '" + source + "' (mismatched tree)");
					return -1;
				}
			}

			if (matchEnd.Success)
			{
				return matchEnd.Index + matchEnd.Length;
			}

//			Console.WriteLine("Warning: malformed tags detected in source '" + source + "' (no end tag)");
			return -1;
		}

		public static String GetTagInnerXml(String source, String tagName)
		{
			return GetTagInnerXml(source, tagName, 0);
		}

		public static String GetTagInnerXml(String source, String tagName, int startIndex)
		{
			Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|/|>)");

			// really find the start of the tag (should match startIndex)						
			Match matchStart = regexSpecificTagStart.Match(source, startIndex);

			if (!matchStart.Success)
			{
				//Console.WriteLine("Warning: could not find start tag '" + tagName + "'.");
				return null;
			}

			int endOfStartTag = source.IndexOf(">", matchStart.Index) + 1;

			if (endOfStartTag <= 1)
			{
				//Console.WriteLine("Warning: could not find start tag '" + tagName + "'.");
				return null;
			}

			int endOfEndTag = GetEndOfTag(source, tagName, matchStart.Index);

			if (endOfStartTag == endOfEndTag)
			{
				return String.Empty;
			}

			int startOfEndTag = source.LastIndexOf("<", endOfEndTag - 1);

			return source.Substring(endOfStartTag, startOfEndTag - endOfStartTag);
		}

		public static int GetStartOfTag(String source, String tagName, int startIndex)
		{
            tagName = tagName.Replace("?", "\\?");

			Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|/|>)");

			Match matchNextTagStart = regexSpecificTagStart.Match(source, startIndex);

			if (matchNextTagStart.Success)
			{
				return matchNextTagStart.Index;
			}

			return -1;
		}

		///																												<summary>
		/// 
		///																												</summary>
		///																												<param name="source">
		///																												</param>
		///																												<param name="tagName">
		///																												</param>
		///																												<param name="startIndex">
		///																												</param>
		///																												<returns>
		/// The index of the first character after the end of this tag, or -1 if this is a
		/// malformed source.
		///																												</returns>
		public static int GetEndOfTag(String source, String tagName, int startIndex)
		{
			Regex regexSpecificTagStart;
			Regex regexSingleTagEnd = new Regex("/>");
			Regex regexComplexTagEnd;
			Regex regexTagStart = new Regex("<[\\w|?]");
			bool findComplexEndAndLeave = false;

			if (tagName == "?xml")
			{
				regexSpecificTagStart = new Regex("<\\?xml(\\s|/|>)");
				regexComplexTagEnd = new Regex(">");
				findComplexEndAndLeave = true;
			}
			else if (tagName == "![CDATA[")
			{
				regexSpecificTagStart = new Regex("<!\\[CDATA\\[");
				regexComplexTagEnd = new Regex("]]>");
				findComplexEndAndLeave = true;
			}
			else if (tagName.Length > 8 && tagName.Substring(0, 8) == "![CDATA[")
			{
				regexSpecificTagStart = new Regex("<!\\[CDATA\\[");
				regexComplexTagEnd = new Regex("]]>");
				findComplexEndAndLeave = true;
			}
			else
			{
				regexSpecificTagStart = new Regex("<" + tagName + "(\\s|/|>)");
				if (tagName == "!--")
				{
					regexComplexTagEnd = new Regex("-->");
					findComplexEndAndLeave = true;
				}
				else
				{
					regexComplexTagEnd = new Regex("</" + tagName + ">");
				}
			}

			// really find the start of the tag (should match startIndex)						
			Match matchStart = regexSpecificTagStart.Match(source, startIndex);

			if (!matchStart.Success)
			{
				throw new MalformedXmlException("Warning: Could not find and end to the tag '" + tagName + "'.", source);
			}

			// find the end of the start tag
			int iEndOfStartTagInitial = matchStart.Index + matchStart.Length - 1;

			// find next </Foo>
			Match matchNextComplexCloser = regexComplexTagEnd.Match(source, iEndOfStartTagInitial);
			while (!findComplexEndAndLeave && matchNextComplexCloser.Success && IsInsideOfIgnoredArea(ref source, matchNextComplexCloser.Index))
			{ 
				matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 1); 
			}

			if (findComplexEndAndLeave && matchNextComplexCloser.Success)
			{
				return matchNextComplexCloser.Index + matchNextComplexCloser.Length;
			}

			Debug.Assert(!findComplexEndAndLeave);

			// find next <Foo
			Match matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, iEndOfStartTagInitial);
			while (matchNextSpecificSingleStart.Success && IsInsideOfIgnoredArea(ref source, matchNextSpecificSingleStart.Index))
			{
				matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, matchNextSpecificSingleStart.Index + 1);
			}

			// find next />
			Match matchNextSingleCloser = regexSingleTagEnd.Match(source, iEndOfStartTagInitial);
			while (matchNextSingleCloser.Success && IsInsideOfIgnoredArea(ref source, matchNextSingleCloser.Index))
			{
				matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSingleCloser.Index + 1);
			}

			// find next <
			Match matchNextSingleStart = regexTagStart.Match(source, iEndOfStartTagInitial);
			while (matchNextSingleStart.Success && IsInsideOfIgnoredArea(ref source, matchNextSingleStart.Index))
			{
				matchNextSingleStart = regexTagStart.Match(source, matchNextSingleStart.Index + 1);
			}

			// are we dealing with a singleton (<Foo/>) tag?  if so, just kill it.
			if (			matchNextSingleCloser.Success &&
					(	!matchNextSingleStart.Success ||
						matchNextSingleCloser.Index < matchNextSingleStart.Index) &&
					(	!matchNextComplexCloser.Success ||
						matchNextSingleCloser.Index < matchNextComplexCloser.Index)
				)
			{
				return matchNextSingleCloser.Index + 2;
			}


			// ok, it's a complex tag.  We need to maintain a count of the number of start tags
			// we have seen, and subtract out the number of closer tags we have seen
			// when starttags == 0, we know we are done.
			int iStartTags = 1;

			while (iStartTags > 0 && matchNextComplexCloser.Success)
			{
				 if (		matchNextComplexCloser.Index < matchNextSpecificSingleStart.Index ||
							!matchNextSpecificSingleStart.Success)
				{
					iStartTags--;
					if (iStartTags > 0)
					{
						matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 1);
						while (matchNextComplexCloser.Success && IsInsideOfIgnoredArea(ref source, matchNextComplexCloser.Index))
						{ 
							matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 1); 
						}
}
				}
				// is the next thing a tag start?
				else  if (	matchNextSpecificSingleStart.Success &&
						matchNextSpecificSingleStart.Index < matchNextComplexCloser.Index)
				{
					iStartTags++;

					// see if this is a singleton tag... if so, subtract the count back out.
					matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSpecificSingleStart.Index + 1);
					matchNextSingleStart = regexTagStart.Match(source, matchNextSpecificSingleStart.Index + 1);

					if (	matchNextSingleCloser.Success && 								// we've found a />
						matchNextSingleCloser.Index < matchNextComplexCloser.Index && 	// it comes before the next </Foo>	
							( matchNextSingleCloser.Index < matchNextSingleStart.Index ||	// it comes before the next <Bar> or <Foo> or whatever.
							!matchNextSingleStart.Success))
					{
						iStartTags--;
					}

					matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, matchNextSpecificSingleStart.Index + 2);
					while (matchNextSpecificSingleStart.Success && IsInsideOfIgnoredArea(ref source, matchNextSpecificSingleStart.Index))
					{
						matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, matchNextSpecificSingleStart.Index + 1);
					}
				}
				else
				{
					throw new MalformedXmlException("Warning: malformed tags detected in source (mismatched tree)", source);
				}
			}

			if (matchNextComplexCloser.Success)
			{
				int iComplexEnd = source.IndexOf(">", matchNextComplexCloser.Index);
				
				// for any complex tag ender </, there should be a > that follows it. if not, bail.
				if (iComplexEnd >= 0)
				{
					iComplexEnd++;
					return iComplexEnd;
				}
			}

			throw new MalformedXmlException("Warning: malformed tags detected in source  (no end tag)", source);
		}

        /// <summary>
        /// This derivation of GetEndOfTag accepts markup more tolerant than strict XML rules, and will accept things like <div><img src="fa"></div>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tagName"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int GetEndOfTagTolerant(String source, String tagName, int startIndex)
        {
            Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
            Regex regexSingleTagEnd = new Regex("/>");
            Regex regexComplexTagEnd = new Regex("</" + tagName + ">");
            Regex regexTagStart = new Regex("<[\\w|?]");


            // really find the start of the tag (should match startIndex)						
            Match matchStart = regexSpecificTagStart.Match(source, startIndex);

            if (!matchStart.Success)
            {
                // Console.WriteLine("Warning: could not refind start tag '" + tagName + "'.");
                return -1;
            }


            // find the end of the start tag
            int iEndOfStartTagInitial = matchStart.Index + matchStart.Length;

            // find next </Foo>
            Match matchNextComplexCloser = regexComplexTagEnd.Match(source, iEndOfStartTagInitial);

            // find next <Foo
            Match matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, iEndOfStartTagInitial);

            // find next />
            Match matchNextSingleCloser = regexSingleTagEnd.Match(source, iEndOfStartTagInitial);

            // find next <
            Match matchNextSingleStart = regexTagStart.Match(source, iEndOfStartTagInitial);

            // are we dealing with a singleton (<Foo/>) tag?  if so, just kill it.
            if (matchNextSingleCloser.Success &&
                    (!matchNextSingleStart.Success ||
                        matchNextSingleCloser.Index < matchNextSingleStart.Index) &&
                    (!matchNextComplexCloser.Success ||
                        matchNextSingleCloser.Index < matchNextComplexCloser.Index)
                )
            {
                return matchNextSingleCloser.Index + 2;
            }


            // ok, it's a complex tag.  We need to maintain a count of the number of start tags
            // we have seen, and subtract out the number of closer tags we have seen
            // when starttags == 0, we know we are done.
            int iStartTags = 1;

            while (iStartTags > 0 && matchNextComplexCloser.Success)
            {
                if (matchNextComplexCloser.Index < matchNextSpecificSingleStart.Index ||
                       !matchNextSpecificSingleStart.Success)
                {
                    iStartTags--;
                    if (iStartTags > 0)
                        matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 1);
                }
                // is the next thing a tag start?
                else if (matchNextSpecificSingleStart.Success &&
                       matchNextSpecificSingleStart.Index < matchNextComplexCloser.Index)
                {
                    iStartTags++;

                    // see if this is a singleton tag... if so, subtract the count back out.
                    matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSpecificSingleStart.Index + 1);
                    matchNextSingleStart = regexTagStart.Match(source, matchNextSpecificSingleStart.Index + 1);

                    if (matchNextSingleCloser.Success && 								// we've found a />
                        matchNextSingleCloser.Index < matchNextComplexCloser.Index && 	// it comes before the next </Foo>	
                            (matchNextSingleCloser.Index < matchNextSingleStart.Index ||	// it comes before the next <Bar> or <Foo> or whatever.
                            !matchNextSingleStart.Success))
                    {
                        iStartTags--;
                    }

                    matchNextSpecificSingleStart = regexSpecificTagStart.Match(source, matchNextSpecificSingleStart.Index + 2);
                }
                else
                {
                    //					Console.WriteLine("Warning: malformed tags detected in source '" + source + "' (mismatched tree)");
                    return -1;
                }
            }

            if (matchNextComplexCloser.Success)
            {
                int iComplexEnd = source.IndexOf(">", matchNextComplexCloser.Index);

                // for any complex tag ender </, there should be a > that follows it. if not, bail.
                if (iComplexEnd >= 0)
                {
                    iComplexEnd++;
                    return iComplexEnd;
                }
            }

            //			Console.WriteLine("Warning: malformed tags detected in source '" + source + "' (no end tag)");
            return -1;
        }



		private static bool IsInsideOfIgnoredArea(ref String content, int index)
		{
			int lastTag = content.LastIndexOf("<![CDATA[", index + 1);

			if (lastTag >= 0)
			{
				int lastEnd = content.LastIndexOf("]]>", index + 1);

				if (lastEnd < lastTag)
				{
					return true;
				}
			}

			return false;
		}

        public static int CountCharsInBetween(ref String source, String token, int start, int end)
        {
            int count = 0;
            int next = source.IndexOf(token, start);

            while (next >= 0 && next < end)
            {
                count++;

                next = source.IndexOf(token, next + token.Length);
            }

            return count;
        }

        public static bool IsInQuote(ref String source, int index)
        {
            if (index < 0)
            {
                return false;
            }

            return ((TextUtilities.CountCharsInBetween(ref source, "\"", 0, index) % 2) == 1);
        }

        public static bool IsInComment(String source, int index)
        {
            int lastStart = source.LastIndexOf("/*", index);

            if (lastStart >= 0)
            {
                int lastEnd = source.LastIndexOf("*/", index);

                if (lastEnd < lastStart)
                {
                    return true;
                }
            }

            lastStart = source.LastIndexOf("//", index);

            if (lastStart >= 0)
            {
                int lastEnd = source.LastIndexOf("\n", index);

                if (lastEnd < lastStart)
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetStartOfComment(String source, int index)
        {
            return GetStartOfComment(source, index, "/*", "//", "<!--");
        }

        private static int GetStartOfComment(String source, int index, String slashStar, String slashSlash, String lessThanQuestionMark)
        {
            int previousSlashStar = source.LastIndexOf(slashStar, index);
            int previousSlashSlash = source.LastIndexOf(slashSlash, index);
            int previousLessThanQuestionMark = source.LastIndexOf(lessThanQuestionMark, index);

            if (previousSlashStar >= 0 && previousSlashSlash < previousSlashStar && previousLessThanQuestionMark < previousSlashStar)
            {
                return previousSlashStar;
            }

            if (previousSlashSlash >= 0 && previousLessThanQuestionMark < previousSlashSlash)
            {
                previousSlashSlash = source.LastIndexOf('\n', previousSlashSlash) + 1;
                return previousSlashSlash;
            }

            return previousLessThanQuestionMark;
        }

        public static int GetEndOfComment(String source, int index)
        {
            int start = GetStartOfComment(source, index);

            Debug.Assert(start >= 0);
            String end;

            if (source.Substring(start, 2) == "/*")
            {
                end = "*/";
            }
            else if (source.Substring(start, 4) == "<!--")
            {
                end = "-->";
            }
            else
            {
                end = "\n";
            }

            return source.IndexOf(end, index) + end.Length;
        }

        public static int GetTagDepth(String source, int tagStartIndex)
		{
			int iTagDepth = 0;
			String sPrior = source.Substring(0, tagStartIndex);
			
			Regex regexSingleTagEnd = new Regex("/>");
			Regex regexComplexTagEnd = new Regex("</");
			Regex regexTagStart = new Regex("<[\\w|?]");
						
			Match matchNextTagStart = regexTagStart.Match(sPrior);
			Match matchNextTagEnd = regexComplexTagEnd.Match(sPrior);

			while (matchNextTagStart.Success || matchNextTagEnd.Success)
			{
				
				 if (		(matchNextTagEnd.Index < matchNextTagStart.Index && matchNextTagEnd.Success)  ||
						!matchNextTagStart.Success)
				{
					iTagDepth --;
					matchNextTagEnd = regexComplexTagEnd.Match(sPrior, matchNextTagEnd.Index + 1);
				}
				// is the next thing a tag start?
				else  if (	(matchNextTagStart.Index < matchNextTagEnd.Index && matchNextTagStart.Success) || 
						!matchNextTagEnd.Success)
				{
					iTagDepth++;
					
					Match matchNextSingleTagEnd = regexSingleTagEnd.Match(sPrior, matchNextTagStart.Index + 2);
					matchNextTagStart = regexTagStart.Match(sPrior, matchNextTagStart.Index + 1);

					// see if this is a singleton tag... if so, subtract the count back out.
					if (matchNextSingleTagEnd.Success)
					{

						if (	(!matchNextTagStart.Success || 
							matchNextSingleTagEnd.Index < matchNextTagStart.Index) && 
							(!matchNextTagEnd.Success || 
							matchNextSingleTagEnd.Index < matchNextTagEnd.Index))
						{
							iTagDepth--;
						}
					}
				}
				else
				{
					Console.WriteLine("Warning: malformed tags detected in sPrior '" + sPrior + "' (mismatched tree)");
					return -1;
				}
			}
			
			return iTagDepth;
		}
		
		public static void ConvertTagNameUnderneathTag(ref String source, String underneathTagName, String oldTagName, String newTagName)
		{
			Regex regexSpecificTagStart = new Regex("<" +underneathTagName + "(\\s|>)");
			
			Match matchStart = regexSpecificTagStart.Match(source);

			while (matchStart.Success)
			{
				int iEndOfTag= GetEndOfTag(source, underneathTagName, matchStart.Index);

				if ( 	iEndOfTag >= 0 )
				{
					String sTag = source.Substring(matchStart.Index, iEndOfTag - matchStart.Index);

					ConvertTagName(ref sTag, oldTagName, newTagName);

					source = source.Substring(0, matchStart.Index) + sTag + source.Substring(iEndOfTag, source.Length - iEndOfTag);
				}
				
				matchStart = regexSpecificTagStart.Match(source, matchStart.Index + 1);
			}
		}

		public static void ConvertTagName(ref String source, String oldTagName, String newTagName)
		{
			// stupid but effective...
			source = source.Replace("<" + oldTagName + "\t", "<" + newTagName + "\t");
			source = source.Replace("<" + oldTagName + " ", "<" + newTagName + " ");
			source = source.Replace("<" + oldTagName + ">", "<" + newTagName + ">");
			source = source.Replace("<" + oldTagName + "/>", "<" + newTagName + "/>");
			source = source.Replace("</" + oldTagName + ">", "</" + newTagName + ">");
		}

		public static String GetAttribute(ref String source, String attributeName)
		{
			return GetAttribute(ref source, attributeName, 0);
		}

        public static int GetAttributeStart(ref String source, String attributeName, int startIndex)
        {
//            Regex attributeInitial = new Regex(attributeName + "=(\"|\')");
            Regex attributeInitial = new Regex(attributeName + "=");
            
            Match attributeStartMatch = attributeInitial.Match(source, startIndex);

            if (!attributeStartMatch.Success)
            {
                return -1;
            }

            return attributeStartMatch.Index + attributeStartMatch.Length;
        }

        public static int GetAttributeEnd(ref String source, String attributeName, int startIndex)
        {
            char sep = source[startIndex];
            Match attributeEndMatch = null;

            if (sep == '\'' || sep == '\"')
            {
                Regex attributeEnd = new Regex("(\"|\')");
                attributeEndMatch = attributeEnd.Match(source, startIndex + 1);

                if (!attributeEndMatch.Success)
                {
                    return -1;
                }
            }
            else
            {
                Regex attributeEnd = new Regex("(/|\\s|>)");
                attributeEndMatch = attributeEnd.Match(source, startIndex);

                if (!attributeEndMatch.Success)
                {
                    return -1;
                }
            }

            return attributeEndMatch.Index;
        }
        
        public static String GetAttribute(ref String source, String attributeName, int startIndex)
		{
            int startOfAttributeContent = GetAttributeStart(ref source, attributeName, startIndex);

            if (startOfAttributeContent < 0)
            {
                return null;
            }

            int endOfAttributeContent = GetAttributeEnd(ref source, attributeName, startOfAttributeContent);

			if (endOfAttributeContent < 0)
			{
				return null;
			}

			return source.Substring(startOfAttributeContent + 1, endOfAttributeContent - (startOfAttributeContent + 1));
		}

		public static void StripAttributeFromTags(ref String source, String[] tagNames, String attributeName)
		{
			foreach (String tagName in tagNames)
				StripAttributeFromTag(ref source, tagName, attributeName);
		}

		public static void StripAttributeFromTag(ref String source, String tagName, String attributeName)
		{

			// find tag starts
			int i = source.IndexOf("<" + tagName);

			while (i >= 0)
			{
				// find the end of the tag
				int iEnd = source.IndexOf(">", i);

				// if there is no closer for this tag, then things are bad.  bail.
				if (iEnd < i)
				{
					Console.WriteLine("Incomplete tag detected in source '" + source + "'.  Parts of this file could not be processed.");
					return;
				}

				// now find the attribute within the tag.
				int iAttributeStart = source.IndexOf(attributeName + "=", i);

				if (iAttributeStart > i && iAttributeStart < iEnd)
				{
					// get the quote char in use... is it ' or " ?
					char chQuote = source[iAttributeStart + attributeName.Length + 1];

					// find the closing quote char
					int iAttributeEnd = source.IndexOf(chQuote, iAttributeStart + attributeName.Length + 2);

					if (iAttributeEnd > iAttributeStart && iAttributeEnd < iEnd)
					{
                        iEnd -= (iAttributeEnd - iAttributeStart);
						source = source.Substring(0, iAttributeStart) + source.Substring(iAttributeEnd + 1, source.Length - (iAttributeEnd + 1));
					}
				}

				i = source.IndexOf("<" + tagName, iEnd);
			}

		}
		

		public static void ChangeAttributeNameForTags(ref String source, String[] tagNames, String oldAttributeName, String newAttributeName)
		{
			foreach (String tagName in tagNames)
			{
				ChangeAttributeNameForTag(ref source, tagName, oldAttributeName, newAttributeName);
			}
		}

		public static void ChangeAttributeNameForTag(ref String source, String tagName, String oldAttributeName, String newAttributeName)
		{
			int i = source.IndexOf("<" + tagName);

			while (i >= 0)
			{
				int iEnd = source.IndexOf(">", i);

				if (iEnd < i)
					return;

				int iOldAttribute = source.IndexOf(oldAttributeName, i);

				if (iOldAttribute > i && iOldAttribute < iEnd)
				{
					source = source.Substring(0, iOldAttribute) + newAttributeName + source.Substring(iOldAttribute + oldAttributeName.Length, source.Length - (iOldAttribute + oldAttributeName.Length));
				}

				i = source.IndexOf("<" + tagName, iEnd);
			}

		}

		public static void StripTagsOfType(ref String source, String tagName)
		{
			int iLeft = source.IndexOf("<" + tagName);

			while (iLeft >= 0)
			{
				int iRight = source.IndexOf(">", iLeft);
				if (iRight < iLeft)
				{
					Console.WriteLine("Warning: source string '" + source + "' looks malformed.");
					return;
				}
					
				source = source.Substring(0, iLeft) + source.Substring(iRight + 1, source.Length - (iRight + 1));

				iLeft = source.IndexOf("<" + tagName);
			}

			source = source.Replace("</" + tagName + ">", "");
		}

		
		public static void StripTags(ref String source)
		{
			StripChunks(ref source, "<", ">");
		}
		
		public static void StripChunks(ref String source, String chunkStart, String chunkEnd)
		{
			int iChunkStart = source.IndexOf(chunkStart);
			int iChunkEnd;

			while (iChunkStart >= 0)
			{
				iChunkEnd = source.IndexOf(chunkEnd, iChunkStart);

				if (iChunkEnd >= iChunkStart)
					source = source.Substring(0, iChunkStart) + source.Substring(iChunkEnd + chunkEnd.Length, source.Length - (iChunkEnd + chunkEnd.Length));
				else
				{
					Console.WriteLine("Warning: malformed chunk found.  There is no chunk end '" + chunkEnd + "' for the start.");
					return;					
				}
				
				iChunkStart = source.IndexOf(chunkStart);
			}
		}
		
		public static void StripTagsAndTheirContents(ref String source)
		{
			// deal with meta tags
			StripChunks(ref source, "<?", "?>");

			// deal with comments
			StripChunks(ref source, "<!--", "-->");

			// now deal with normal tags.
			Regex regexTagStart = new Regex("<[\\w|?]");
			Regex regexSingleTagEnd = new Regex("/>");
			Regex regexComplexTagEnd = new Regex("</");
			
			
			Match matchStart = regexTagStart.Match(source);

			while (matchStart.Success)
			{
				int iEndOfStartTagInitial = matchStart.Index + matchStart.Length;
				
				Match matchNextSingleCloser = regexSingleTagEnd.Match(source, iEndOfStartTagInitial);
				Match matchNextComplexCloser = regexComplexTagEnd.Match(source, iEndOfStartTagInitial);
				Match matchNextSingleStart = regexTagStart.Match(source, iEndOfStartTagInitial);

				// are we dealing with a singleton (<Foo/>) tag?  if so, just kill it.
				if (	matchNextSingleCloser.Success &&
						(	!matchNextSingleStart.Success ||
							matchNextSingleCloser.Index < matchNextSingleStart.Index) &&
						(	!matchNextComplexCloser.Success ||
							matchNextSingleCloser.Index < matchNextComplexCloser.Index)
					)
				{
					source = source.Substring(0, matchStart.Index) + source.Substring(matchNextSingleCloser.Index + 2, source.Length - (matchNextSingleCloser.Index + 2));
				}
				else
				{
					int iStartTags = 1;


					while (iStartTags > 0 && matchNextComplexCloser.Success)
					{
						// is the next thing a />?
						if (	matchNextSingleCloser.Success &&		// we've found a single closer
							(	matchNextSingleCloser.Index < matchNextSingleStart.Index ||	// it comes before the next <foo.. tag (or there is no more <foo.. tags)
								!matchNextSingleStart.Success) &&
							matchNextSingleCloser.Index < matchNextComplexCloser.Index) // it comes before the next </foo
						{
							iStartTags--;
							matchNextSingleCloser = regexSingleTagEnd.Match(source, matchNextSingleCloser.Index + 2);
						}
						// is the next thing a </ ?)
						// note we always assume there must be a future complex closer...
						else  if (	(	matchNextComplexCloser.Index < matchNextSingleStart.Index ||
									!matchNextSingleStart.Success)
								&&
								(	matchNextComplexCloser.Index < matchNextSingleCloser.Index ||
									!matchNextSingleCloser.Success))
							{
								iStartTags--;
								if (iStartTags > 0)
									matchNextComplexCloser = regexComplexTagEnd.Match(source, matchNextComplexCloser.Index + 2);
							}
						// is the next thing a tag start?
						else  if (	matchNextSingleStart.Success &&
								matchNextSingleStart.Index < matchNextComplexCloser.Index &&
									(	matchNextSingleStart.Index < matchNextSingleCloser.Index ||
										!matchNextSingleCloser.Success))
							{
								iStartTags++;
								matchNextSingleStart = regexTagStart.Match(source, matchNextSingleStart.Index + 2);
							}
					}

					if (matchNextComplexCloser.Success)
					{
						int iComplexEnd = source.IndexOf(">", matchNextComplexCloser.Index);
						
						// for any complex tag ender </, there should be a > that follows it. if not, bail.
						if (iComplexEnd < 0)
						{
							Console.WriteLine("Warning: malformed tags detected in source '" + source + "'");
							return;
						}

						iComplexEnd++;
						
						source = source.Substring(0, matchStart.Index) + source.Substring(iComplexEnd, source.Length - iComplexEnd);
					}
					else
					{
						//Console.WriteLine("Warning: could not find a proper end tag in source '" + source + "'");
						return;
					}

				}

				matchStart = regexTagStart.Match(source);
			}
		}

        /// <summary>
        /// Given, say, a list of files on a path, returh
        /// </summary>
        public static String[] GetFileList(String source)
        {
            source = source.Trim();

            List<String> files = new List<string>();
            int last = 0;
            int nextSpace = source.IndexOf(" ");

            while (TextUtilities.IsInQuote(ref source, nextSpace))
            {
                nextSpace = source.IndexOf(" ", nextSpace + 1);
            }

            while (nextSpace >= 0)
            {
                files.Add(source.Substring(last, nextSpace - last));
                nextSpace++;
                last = nextSpace;

                nextSpace = source.IndexOf(" ", nextSpace);

                while (TextUtilities.IsInQuote(ref source, nextSpace))
                {
                    nextSpace = source.IndexOf(" ", nextSpace + 1);
                }
            }

            String lastPart = source.Substring(last, source.Length - last);

            files.Add(lastPart);

            String[] fileList = new String[files.Count];

            files.CopyTo(fileList);

            return fileList;
        }

		public static String StripTagsOfTypeAndTheirContents(ref String source, String tagName, bool topLevelTagsOnly)
		{
			String sDiscardedContent = "";

			Regex regexSpecificTagStart = new Regex("<" + tagName + "(\\s|>)");
			
			Match matchStart = regexSpecificTagStart.Match(source);

			while (matchStart.Success)
			{
				if (!topLevelTagsOnly || GetTagDepth(source, matchStart.Index) < 1)
				{
					int iEndOfTag = GetEndOfTag(source, tagName, matchStart.Index);
					int iTagBeginning = matchStart.Index - 1;

					// back the count up and get the whitespace before the tag start.
					while (iTagBeginning >= 0 && (source[iTagBeginning] == ' ' || source[iTagBeginning] == '\r' || source[iTagBeginning] == '\n' || source[iTagBeginning] == '\t'))
						iTagBeginning--;

					// we iterated until we found the first non whitespace char, so add one back in
					// to take us to a whitespace char.
					iTagBeginning++;
					
					if (iEndOfTag < 0)
						return null;

					sDiscardedContent += source.Substring(iTagBeginning, iEndOfTag - iTagBeginning);
					source = source.Substring(0, iTagBeginning) + source.Substring(iEndOfTag, source.Length - iEndOfTag);
					matchStart = regexSpecificTagStart.Match(source, iTagBeginning);
				}
				else
				{
					matchStart = regexSpecificTagStart.Match(source, matchStart.Index + 1);
				}
			}
			return sDiscardedContent;
		}

        public static String GetUtf8StringFromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static String EncodeForParam(String str)
        {
            String copy = str;

            XmlUtilities.Entitize(ref copy);

            copy = copy.Replace(",", "&comma;");

            return copy;

        }

        public static String GetAsciiStringFromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            if (bytes.Length >= 3 && bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191)
            {
                String resultStr = Encoding.UTF8.GetString(bytes);

                if (resultStr[0] == 65279)   // TODO: find out what this thing is, and handle appropriately
                {
                    resultStr = resultStr.Substring(1, resultStr.Length - 1);
                }

                return resultStr;
            }

            return Encoding.UTF8.GetString(bytes);

        }

        public static String PostPadText(String content, String postPendText, int targetLength)
        {
            Debug.Assert(postPendText.Length > 0);

            if (content == null)
            {
                content = String.Empty;
            }

            while (content.Length < targetLength)
            {
                content = content + postPendText;
            }

            return content;
        }

        public static String PrePadText(String content, String prePendText, int targetLength)
        {
            Debug.Assert(prePendText.Length > 0);

            if (content == null)
            {
                content = String.Empty;
            }

            while (content.Length < targetLength)
            {
                content = prePendText + content;
            }

            return content;
        }

        public static String DecodeFromParam(String str)
        {
            String copy = str;

            copy = copy.Replace("&comma;", ",");

            XmlUtilities.DeEntitize(ref copy);

            return copy;
        }
	}
}

