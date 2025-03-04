using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Shingle;
using Token = Lucene.Net.Analysis.Token;
using ITermAttribute = Lucene.Net.Analysis.Tokenattributes.ITermAttribute;
using IOffsetAttribute = Lucene.Net.Analysis.Tokenattributes.IOffsetAttribute;
using IPositionIncrementAttribute = Lucene.Net.Analysis.Tokenattributes.IPositionIncrementAttribute;

namespace CDR.Indexer
{
    /// <summary>
    /// Een simularity die de lengte van een veld uitschakeld voor wat betreft de boost factor
    /// </summary>
    public class DefaultSimilarityExtended : Lucene.Net.Search.DefaultSimilarity
    {
        /*
        public override float Coord(int overlap, int maxOverlap)
        {
            return (float)(1.0);
        }
        */
        public override float Idf(int docFreq, int numDocs)
        {
            return (float)(1.0);
        }

        /// <summary>Implemented as <code>1/sqrt(numTerms)</code>. </summary>
        public override float LengthNorm(System.String fieldName, int numTerms)
        {
            return (float)(1.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        public override float Tf(float freq)
        {
            return (float)(1.0);
        }
    }

    /// <summary>
    /// Een simularity die de lengte van een veld uitschakeld voor wat betreft de boost factor
    /// </summary>
    public class NoLengthSimilarity : Lucene.Net.Search.Similarity
    {
        public override float ComputeNorm(System.String field, Lucene.Net.Index.FieldInvertState state)
        {
            int numTerms;
            if (discountOverlaps)
                numTerms = state.Length - state.NumOverlap;
            else
                numTerms = state.Length;
            return (float)(state.Boost * LengthNorm(field, numTerms));
        }

        /// <summary>Implemented as <code>1/sqrt(numTerms)</code>. </summary>
        public override float LengthNorm(System.String fieldName, int numTerms)
        {
            return (float)(1.0);
        }

        /// <summary>Implemented as <code>1/sqrt(sumOfSquaredWeights)</code>. </summary>
        public override float QueryNorm(float sumOfSquaredWeights)
        {
            return (float)(1.0 / System.Math.Sqrt(sumOfSquaredWeights));
        }

        /// <summary>Implemented as <code>sqrt(freq)</code>. </summary>
        public override float Tf(float freq)
        {
            return (float)System.Math.Sqrt(freq);
        }

        /// <summary>Implemented as <code>1 / (distance + 1)</code>. </summary>
        public override float SloppyFreq(int distance)
        {
            return 1.0f / (distance + 1);
        }

        /// <summary>Implemented as <code>log(numDocs/(docFreq+1)) + 1</code>. </summary>
        public override float Idf(int docFreq, int numDocs)
        {
            return (float)(System.Math.Log(numDocs / (double)(docFreq + 1)) + 1.0);
        }

        /// <summary>Implemented as <code>overlap / maxOverlap</code>. </summary>
        public override float Coord(int overlap, int maxOverlap)
        {
            return overlap / (float)maxOverlap;
        }

        // Default false
        protected internal bool discountOverlaps;

        /// <summary>Determines whether overlap tokens (Tokens with
        /// 0 position increment) are ignored when computing
        /// norm.  By default this is false, meaning overlap
        /// tokens are counted just like non-overlap tokens.
        /// 
        /// <p/><b>WARNING</b>: This API is new and experimental, and may suddenly
        /// change.<p/>
        /// 
        /// </summary>
        /// <seealso cref="computeNorm">
        /// </seealso>
        public virtual void SetDiscountOverlaps(bool v)
        {
            discountOverlaps = v;
        }

        /// <seealso cref="setDiscountOverlaps">
        /// </seealso>
        public virtual bool GetDiscountOverlaps()
        {
            return discountOverlaps;
        }
    }

    /// <summary>
    /// Speciale analyzer die werk als de SimpleAnalyzer en die diacrieten normalizeert.
    /// </summary>
    public class CDRAnalyzer : Analyzer
    {
        public CDRAnalyzer()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader))));
            return result;
        }
    }

    public class CDRShinkleAnalyzer : Analyzer
    {
        int minShingles = 1;
        public CDRShinkleAnalyzer()
        {
        }

        public CDRShinkleAnalyzer(int minShingles)
            : base()
        {
            if (minShingles < 1 || minShingles > 2)
            {
                minShingles = 1;
            }
            this.minShingles = minShingles;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new ShingleMatrixFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))), minShingles, 2, '_', false, new Lucene.Net.Analysis.Shingle.Codec.TwoDimensionalNonWeightedSynonymTokenSettingsCodec());
            return result;
        }
    }

    /// <summary>
    /// Analyzer met standaard diacriet filter van lucenen zelf.
    /// </summary>
    public class CDRAnalyzer2 : Analyzer
    {
        private bool useStopWords = false;

        public CDRAnalyzer2()
            : base()
        {
        }

        public CDRAnalyzer2(bool useStopWords)
            : base()
        {
            this.useStopWords = useStopWords;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = null;
            if (useStopWords)
            {
                ISet<string> stopWords = StopFilter.MakeStopSet(new string[]{"a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with", // EN
                    "van", "aan", "dat", "de", "den", "der", "des", "deze", "die", "dit", "door", "een", "het", "ik", "is", "je", "na", // NL
                    "au", "aux", "la", "le", "les"}); // FR

                result = new PorterStemFilter(new StopFilter(false, new ASCIIFoldingFilter(new LowerCaseFilter(new CDRWhitespaceTokenizer(reader))), stopWords));
            }
            else
            {
                result = new PorterStemFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new CDRWhitespaceTokenizer(reader))));
            }

            return result;
        }
    }

    /// <summary>
    /// Insert variant of a string based on special none wheiet cpace chars which are
    /// not digits or char
    /// </summary>
    public class CDRWhitespaceTokenizer : CharTokenizer
    {
        public static char[] NoneWhiteSpaceChars = new char[] { '.', '-', '#' };

        private char[] noneWhiteSpaceChars = null;
        /// <summary>Construct a new WhitespaceTokenizer. </summary>
        public CDRWhitespaceTokenizer(char[] noneWhiteSpaceChars, System.IO.TextReader in_Renamed)
            : base(in_Renamed)
        {
            this.noneWhiteSpaceChars = noneWhiteSpaceChars;
        }

        public CDRWhitespaceTokenizer(System.IO.TextReader in_Renamed)
            : base(in_Renamed)
        {
            this.noneWhiteSpaceChars = CDRWhitespaceTokenizer.NoneWhiteSpaceChars;
        }

        /// <summary>Collects only characters which do not satisfy
        /// {@link Character#isWhitespace(char)}.
        /// </summary>
        protected internal override bool IsTokenChar(char c)
        {
            if (System.Char.IsLetterOrDigit(c))
            {
                return true;
            }

            if (noneWhiteSpaceChars != null)
            {
                foreach (char nonWhitespaceChar in noneWhiteSpaceChars)
                {
                    if (c == nonWhitespaceChar)
                    {
                        return true;
                    }
                }
            } //foreach

            return false;
        }
    }

    /// <summary>
    /// Considers &^&$*( etc as white special except when tehre a digit or letters beforen and/or after
    /// then emit bothg cases
    /// A-Ha Emits:
    ///  A-Ha
    ///  A
    ///  Ha
    /// </summary>
    public sealed class SpecialNoneWhiteSpaceFilter : TokenFilter
    {
        private char[] noneWhiteSpaceChars = null;
        private class SavedTerm
        {
            public string Term = "";
            public int StartOffset = -1;
            public int EndOffset = -1;
        }

        private List<SavedTerm> savedTerms;
        private ITermAttribute termAtt;
        private Lucene.Net.Util.AttributeSource.State current;
        private IPositionIncrementAttribute posIncrAtt; //Lucene.Net.Util.
        private IOffsetAttribute termOff;

        public SpecialNoneWhiteSpaceFilter(TokenStream in_Renamed)
            : base(in_Renamed)
        {
            noneWhiteSpaceChars = CDRWhitespaceTokenizer.NoneWhiteSpaceChars;
            savedTerms = new List<SavedTerm>();
            termAtt = AddAttribute<ITermAttribute>();
            posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
            termOff = AddAttribute<IOffsetAttribute>();
        }

        public override bool IncrementToken()
        {
            if (savedTerms.Count > 0)
            {
                RestoreState(current);

                SavedTerm savedTerm = savedTerms[0];
                savedTerms.RemoveAt(0);
                termAtt.SetTermBuffer(savedTerm.Term);
                posIncrAtt.PositionIncrement = 0;
                termOff.SetOffset(savedTerm.StartOffset, savedTerm.EndOffset);

                return true;
            }

            do
            {
                if (!input.IncrementToken())
                {
                    return false;
                }
            }
            while (IsNonWhiteSpaceChar(termAtt.Term));

            // A-Ha
            string s = "";
            int pos = 0;
            foreach (char c in termAtt.Term)
            {
                if (IsNonWhiteSpaceChar(c))
                {
                    if (s.Length > 0)
                    {
                        SavedTerm savedTerm = new SavedTerm();
                        savedTerm.Term = s;
                        savedTerm.StartOffset = termOff.StartOffset;
                        savedTerm.EndOffset = termOff.EndOffset;
                        savedTerms.Add(savedTerm);
                        s = "";
                    }
                }
                else
                {
                    s += c;
                }
                pos++;
            }
            if (s.Length > 0 && s != termAtt.Term)
            {
                SavedTerm savedTerm = new SavedTerm();
                savedTerm.Term = s;
                savedTerm.StartOffset = termOff.StartOffset;
                savedTerm.EndOffset = termOff.EndOffset;
                savedTerms.Add(savedTerm);
            }

            if (savedTerms.Count > 0)
            {
                current = CaptureState();
            }

            return true;
        }

        private bool IsNonWhiteSpaceChar(char c)
        {
            foreach (char nonWhiteSpaceChar in noneWhiteSpaceChars)
            {
                if (nonWhiteSpaceChar == c)
                {
                    return true;
                }
            } //foreach

            return false;
        }

        private bool IsNonWhiteSpaceChar(string s)
        {
            if (s.Length > 0)
            {
                return IsNonWhiteSpaceChar(s[0]);
            }

            return false;
        }
    }

    public class WhitespaceFilter : TokenFilter
    {
        private bool done;
        private StringBuilder receivedText = new StringBuilder();
        private ITermAttribute termAtt;
        private IOffsetAttribute offsetAtt;
        private IPositionIncrementAttribute posIncrAtt;
        private HashSet<string> seen;

        /** Constructs a filter which tokenizes words from the input stream.
         * @param input The token stream from a tokenizer
         */
        public WhitespaceFilter(TokenStream input)
            : base(input)
        {
            seen = new HashSet<string>();
            termAtt = AddAttribute<ITermAttribute>();
            offsetAtt = AddAttribute<IOffsetAttribute>();
            posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
            ClearAttributes();
        }

        private string GetNextPart()
        {
            StringBuilder emittedText = new StringBuilder();
            //left trim the token
            while (true)
            {
                if (receivedText.Length == 0) break;
                char c = receivedText[0];
                if (char.IsLetterOrDigit(c)) break;
                receivedText.Remove(0, 1);
            }
            //keep the good stuff
            while (true)
            {
                if (receivedText.Length == 0) break;
                char c = receivedText[0];
                if (!char.IsLetterOrDigit(c)) break;
                emittedText.Append(receivedText[0]);
                receivedText.Remove(0, 1);
            }
            //right trim the token
            while (true)
            {
                if (receivedText.Length == 0) break;
                char c = receivedText[0];
                if (char.IsLetterOrDigit(c)) break;
                receivedText.Remove(0, 1);
            }
            return emittedText.ToString();
        }

        /** Returns the next word in the stream.
         * @throws IOException If a problem occurs
         * @return The word
         */
        public override bool IncrementToken()
        {
            while (true)
            {
                if (receivedText.Length == 0)
                {
                    if (input.IncrementToken())
                    {
                        receivedText.Append(termAtt.TermBuffer());
                        receivedText.Length = termAtt.TermLength();
                    }
                }
                if (receivedText.Length == 0)
                {
                    return false;
                }
                while (true)
                {
                    string emittedText = GetNextPart();
                    if (emittedText.Length > 0 && !seen.Contains(emittedText))
                    {
                        termAtt.SetTermBuffer(emittedText.ToCharArray(), 0, emittedText.Length);
                        offsetAtt.SetOffset(0, emittedText.Length);
                        seen.Add(emittedText);

                        return true;
                    }


                    if (emittedText.Length <= 0)
                    {
                        break;
                        //return false;
                    }
                }
            }
            /*
            while (true)
            {
                //New token ?
                if (receivedText.Length == 0)
                {
                    receivedToken = input.Next();
                    newToken = true;
                    if (receivedToken == null) return false;
                    receivedText.Append(receivedToken.TermText());
                }
                String emittedText = GetNextPart();
                if (emittedText.Length > 0)
                {
                    termAtt.SetTermBuffer(emittedText.ToString().ToCharArray(), receivedToken.StartOffset(), receivedToken.EndOffset());
                    offsetAtt.SetOffset(0, emittedText.Length);

                    if (newToken) posIncrAtt.SetPositionIncrement(receivedToken.GetPositionIncrement());
                    else posIncrAtt.SetPositionIncrement(0);

                    return true;
                }
            }
            */
        }

        /// <summary>Reset the filter as well as the input TokenStream. </summary>
        public override void Reset()
        {
            input.Reset();
            done = false;
            seen.Clear();
        }
    }

    /// <summary>
    /// Verwijderd duplicate tokens die zelf Term hebben MET Zelfde Start en Eind offset
    /// </summary>
    public class RemoveDuplicatesTokenFilter : TokenFilter
    {
        private ITermAttribute termAtt;
        private IPositionIncrementAttribute posIncrAtt; //Lucene.Net.Util.      
        private IOffsetAttribute termOff;
        private List<Token> tokenList = null;
        private int index = -1;

        public RemoveDuplicatesTokenFilter(TokenStream in_Renamed)
            : base(in_Renamed)
        {
            termAtt = AddAttribute<ITermAttribute>();
            posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
            termOff = AddAttribute<IOffsetAttribute>();
            tokenList = null;
        }

        public override bool IncrementToken()
        {
            if (tokenList != null)
            {
                index++;
                if (index < tokenList.Count)
                {
                    termAtt.SetTermBuffer(tokenList[index].TermBuffer(), 0, tokenList[index].TermLength());
                    termOff.SetOffset(tokenList[index].StartOffset, tokenList[index].EndOffset);
                    return true;
                }

                tokenList = null;
                return false;
            }

            tokenList = new List<Token>();
            // First cache result
            while (input.IncrementToken())
            {
                Token newToken = new Token(termAtt.Term, termOff.StartOffset, termOff.EndOffset);
                foreach (Token token in tokenList)
                {
                    if (token.StartOffset == newToken.StartOffset && token.Term == newToken.Term)
                    {
                        token.SetOffset(newToken.StartOffset, newToken.EndOffset);
                        newToken = null;
                        break;
                    }
                } //foreach

                if (newToken != null)
                {
                    tokenList.Add(newToken);
                }
            } // while;

            // now output the tokens!
            if (tokenList.Count > 0)
            {
                index = 0;
                termAtt.SetTermBuffer(tokenList[index].TermBuffer(), 0, tokenList[index].TermLength());
                termOff.SetOffset(tokenList[index].StartOffset, tokenList[index].EndOffset);
                return true;
            }

            return false;
        }

        public override void Reset()
        {
            base.Reset();
            tokenList = null;
        }
    }

    /// <summary>
    /// Doet Portersteming, maar geeft ook nog het orginele token terug.
    /// Dit is nodig voor de highlighter en daar wordt hij ook ALLEEN maar
    /// gebruikt.
    /// NIET GEBRUIKEN VOOR INDEXEREN EN ZOEKEN!!!!!
    /// </summary>
    public sealed class PorterStemFilterAndOrginal : TokenFilter
    {
        private readonly PorterStemmer stemmer;
        private readonly ITermAttribute termAtt;
        private char[] orginal = null;

        public PorterStemFilterAndOrginal(TokenStream in_Renamed)
            : base(in_Renamed)
        {
            stemmer = new PorterStemmer();
            termAtt = AddAttribute<ITermAttribute>();
            orginal = null;
        }

        public override bool IncrementToken()
        {
            if (orginal != null)
            {
                termAtt.SetTermBuffer(orginal, 0, orginal.Length);
                orginal = null;
                return true;
            }

            if (!input.IncrementToken())
            {
                return false;
            }

            orginal = new char[termAtt.TermLength()];
            Array.Copy(termAtt.TermBuffer(), 0, orginal, 0, termAtt.TermLength());

            if (stemmer.Stem(termAtt.TermBuffer(), 0, termAtt.TermLength()))
            {
                termAtt.SetTermBuffer(stemmer.ResultBuffer, 0, stemmer.ResultLength);

                if (CharArrayIsEqual(orginal, termAtt.TermBuffer()))
                {
                    orginal = null;
                }
            }

            return true;
        }

        public override void Reset()
        {
            base.Reset();
            orginal = null;
        }

        private bool CharArrayIsEqual(char[] one, char[] two)
        {
            if (one.Length != two.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < one.Length; i++)
                {
                    if (one[i] != two[i])
                    {
                        return false;
                    }
                } //for
            }

            return true;
        }
    }

    /// <summary> Emits 1 to the full lengte of the input as a token</summary>
    public class PartKeywordTokenizer : Tokenizer
    {
        private bool done;
        private StringBuilder sb;
        private ITermAttribute termAtt;
        private IOffsetAttribute offsetAtt;

        public PartKeywordTokenizer(System.IO.TextReader input)
            : base(input)
        {
            done = false;
            sb = new StringBuilder();
            termAtt = AddAttribute<ITermAttribute>();
            offsetAtt = AddAttribute<IOffsetAttribute>();
            ClearAttributes();
        }

        public override bool IncrementToken()
        {
            if (!done)
            {
                while (true)
                {
                    char[] buffer = new char[1];

                    int length = input.Read(buffer, 0, 1);
                    if (length == 0)
                    {
                        done = true;
                        return false;
                        // break;
                    }
                    sb.Append(buffer);

                    termAtt.SetTermBuffer(sb.ToString().ToCharArray(), 0, sb.Length);
                    offsetAtt.SetOffset(0, sb.Length);

                    // Skip keywoords met op het eind whitespaces
                    if (!char.IsWhiteSpace(buffer[0]))
                    {
                        break;
                    }
                }

                return true;
            }

            return false;
        }

        public override void End()
        {
            // set final offset 
            offsetAtt.SetOffset(0, sb.Length);
        }

        public override void Reset(System.IO.TextReader input)
        {
            base.Reset(input);
            done = false;
        }
    }

    /// <summary>
    /// Geen Porterstemmer!
    /// </summary>
    public class CDRAnalyzerForTypeAsYouGo : Analyzer
    {
        public CDRAnalyzerForTypeAsYouGo()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new Lucene.Net.Analysis.NGram.EdgeNGramTokenFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))), Lucene.Net.Analysis.NGram.Side.FRONT, 1, 1024);
            return result;
        }
    }

    /// <summary>
    /// Juist met porterstemmer
    /// </summary>
    public class CDRAnalyzerForTypeAsYouGoWithPorter : Analyzer
    {
        public CDRAnalyzerForTypeAsYouGoWithPorter()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            //TokenStream result = new Lucene.Net.Analysis.NGram.EdgeNGramTokenFilter(new PorterStemFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader))))), Lucene.Net.Analysis.NGram.Side.FRONT, 1, 1024);
            TokenStream result = new PorterStemFilter(new Lucene.Net.Analysis.NGram.EdgeNGramTokenFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))), Lucene.Net.Analysis.NGram.Side.FRONT, 1, 1024));
            return result;
        }
    }

    /// <summary>
    ///  Als de CDRAnalyzerForTypeAsYouGoWithPorter, maar nu worden dubbelde ngram's weggefiltered
    ///  Is bedoeld as analyzer voor de orginele text bij het highlighten
    /// </summary>
    public class HighlightOriginalTextAnalyser : Analyzer
    {
        public HighlightOriginalTextAnalyser()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new RemoveDuplicatesTokenFilter(new PorterStemFilterAndOrginal(new Lucene.Net.Analysis.NGram.EdgeNGramTokenFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))), Lucene.Net.Analysis.NGram.Side.FRONT, 1, 1024)));
            
            return result;
        }
    }

    /// <summary>
    /// Deze analyser is bedoeld voor het query van de te highlighten text
    /// </summary>
    public class HighlightQueryAnalyser : Analyzer
    {
        public HighlightQueryAnalyser()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new RemoveDuplicatesTokenFilter(new PorterStemFilterAndOrginal(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader))))));
            
            return result;
        }
    }
    
    public class CDRAnalyzerWithPorter : Analyzer
    {
        private bool useStopWords = false;

        public CDRAnalyzerWithPorter()
            : base()
        {
        }

        public CDRAnalyzerWithPorter(bool useStopWords)
            : base()
        {
            this.useStopWords = useStopWords;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = null;
            if (useStopWords)
            {
                ISet<string> stopWords = StopFilter.MakeStopSet(new string[]{"a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with", // EN
                    "van", "aan", "dat", "de", "den", "der", "des", "deze", "die", "dit", "door", "een", "het", "ik", "is", "je", "na", // NL
                    "au", "aux", "la", "le", "les"}); // FR

                result = new PorterStemFilter(new StopFilter(false, new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))), stopWords));
            }
            else
            {
                //result = new PorterStemFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new CDRWhitespaceTokenizer(reader))));
                result = new PorterStemFilter(new ASCIIFoldingFilter(new LowerCaseFilter(new SpecialNoneWhiteSpaceFilter(new CDRWhitespaceTokenizer(reader)))));
            }

            return result;
        }
    }

    public class CDRAnalyzerKeyword: Analyzer
    {
        public CDRAnalyzerKeyword()
            : base()
        {
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new ASCIIFoldingFilter(new LowerCaseFilter(new KeywordTokenizer(reader)));
            return result;
        }
    }

    public static class CDRAnalyzerHelper
    {
        public static string AnalyzeString(string s, Analyzer analyzer)
        {
            StringBuilder sb = new StringBuilder();
            TokenStream stream = analyzer.TokenStream("DUMMY", new StringReader(s));
            ITermAttribute termAtt = stream.AddAttribute<ITermAttribute>();

            while (true)
            {
                if (!stream.IncrementToken())
                {
                    break;
                }

                if (termAtt.Term.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(termAtt.Term);
                }
            } //while

            return sb.ToString().Trim();
        }
    }
}
