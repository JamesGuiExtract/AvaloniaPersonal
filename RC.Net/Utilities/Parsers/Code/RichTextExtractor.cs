using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Extract.Utilities.Parsers
{
    // Class to extract text and character indexes from RTF files
    // Based on the python function from https://stackoverflow.com/a/188877
    public static class RichTextExtractor
    {
        static readonly string _TOKEN =
            @"(?inxs)
              \\(?'Word'[a-z]+) (?'Arg'-?\d+)? \x20?
            | \\'(?'HexCode'[0-9a-f]{2})
            | \\(?'Escape'[^a-z])
            | (?'OpenBrace'[{])
            | (?'CloseBrace'[}])
            | [\r\n]+
            | (?'Literal'.)";

        static readonly ThreadLocal<Regex> _tokenMatcher = new ThreadLocal<Regex>(() => new Regex(_TOKEN));

        // "destination" control words
        static readonly HashSet<string> DESTINATIONS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
          "aftncn", "aftnsep", "aftnsepc", "annotation", "atnauthor", "atndate", "atnicn", "atnid", "atnparent", "atnref", "atntime", "atrfend", "atrfstart", "author", "background", "bkmkend", "bkmkstart", "blipuid", "buptim",
          "category", "colorschememapping", "colortbl", "comment", "company", "creatim", "datafield", "datastore", "defchp", "defpap", "do", "doccomm", "docvar", "dptxbxtext", "ebcend", "ebcstart", "factoidname", "falt",
          "fchars", "ffdeftext", "ffentrymcr", "ffexitmcr", "ffformat", "ffhelptext", "ffl", "ffname", "ffstattext", "field", "file", "filetbl", "fldinst", "fldrslt", "fldtype", "fname", "fontemb", "fontfile", "fonttbl",
          "footer", "footerf", "footerl", "footerr", "footnote", "formfield", "ftncn", "ftnsep", "ftnsepc", "g", "generator", "gridtbl", "header", "headerf", "headerl", "headerr", "hl", "hlfr", "hlinkbase", "hlloc", "hlsrc",
          "hsv", "htmltag", "info", "keycode", "keywords", "latentstyles", "lchars", "levelnumbers", "leveltext", "lfolevel", "linkval", "list", "listlevel", "listname", "listoverride", "listoverridetable", "listpicture",
          "liststylename", "listtable", "listtext", "lsdlockedexcept", "macc", "maccPr", "mailmerge", "maln", "malnScr", "manager", "margPr", "mbar", "mbarPr", "mbaseJc", "mbegChr", "mborderBox", "mborderBoxPr", "mbox", "mboxPr",
          "mchr", "mcount", "mctrlPr", "md", "mdeg", "mdegHide", "mden", "mdiff", "mdPr", "me", "mendChr", "meqArr", "meqArrPr", "mf", "mfName", "mfPr", "mfunc", "mfuncPr", "mgroupChr", "mgroupChrPr", "mgrow", "mhideBot",
          "mhideLeft", "mhideRight", "mhideTop", "mhtmltag", "mlim", "mlimloc", "mlimlow", "mlimlowPr", "mlimupp", "mlimuppPr", "mm", "mmaddfieldname", "mmath", "mmathPict", "mmathPr", "mmaxdist", "mmc", "mmcJc", "mmconnectstr",
          "mmconnectstrdata", "mmcPr", "mmcs", "mmdatasource", "mmheadersource", "mmmailsubject", "mmodso", "mmodsofilter", "mmodsofldmpdata", "mmodsomappedname", "mmodsoname", "mmodsorecipdata", "mmodsosort", "mmodsosrc",
          "mmodsotable", "mmodsoudl", "mmodsoudldata", "mmodsouniquetag", "mmPr", "mmquery", "mmr", "mnary", "mnaryPr", "mnoBreak", "mnum", "mobjDist", "moMath", "moMathPara", "moMathParaPr", "mopEmu", "mphant", "mphantPr",
          "mplcHide", "mpos", "mr", "mrad", "mradPr", "mrPr", "msepChr", "mshow", "mshp", "msPre", "msPrePr", "msSub", "msSubPr", "msSubSup", "msSubSupPr", "msSup", "msSupPr", "mstrikeBLTR", "mstrikeH", "mstrikeTLBR", "mstrikeV",
          "msub", "msubHide", "msup", "msupHide", "mtransp", "mtype", "mvertJc", "mvfmf", "mvfml", "mvtof", "mvtol", "mzeroAsc", "mzeroDesc", "mzeroWid", "nesttableprops", "nextfile", "nonesttables", "objalias", "objclass",
          "objdata", "object", "objname", "objsect", "objtime", "oldcprops", "oldpprops", "oldsprops", "oldtprops", "oleclsid", "operator", "panose", "password", "passwordhash", "pgp", "pgptbl", "picprop", "pict", "pn", "pnseclvl",
          "pntext", "pntxta", "pntxtb", "printim", "private", "propname", "protend", "protstart", "protusertbl", "pxe", "result", "revtbl", "revtim", "rsidtbl", "rxe", "shp", "shpgrp", "shpinst", "shppict", "shprslt", "shptxt",
          "sn", "sp", "staticval", "stylesheet", "subject", "sv", "svb", "tc", "template", "themedata", "title", "txe", "ud", "upr", "userprops", "wgrffmtfilter", "windowcaption", "writereservation", "writereservhash", "xe",
          "xform", "xmlattrname", "xmlattrvalue", "xmlclose", "xmlname", "xmlnstbl", "xmlopen", "v",
        };

        static readonly HashSet<string> OUTPUT_DESTINATIONS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "field",
            "fldrslt",
            "tc",
            "footnote",
            "xe",
        };

        // Special chars
        static readonly Dictionary<string, string> SPECIAL_CHARS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"par", "\n"},
            {"sect", "\n\n"},
            {"page", "\n\n"},
            {"line", "\n"},
            // NOTE: I don't remember why I commented out this one ('\r') but perhaps it is connected to the fact that I am using just '\n' for \par, etc
            //{"r", "\r"},
            {"n", "\n"},
            {"tab", "\t"},
            {"emdash", "\x151"},
            {"endash", "\x150"},
            {"emspace", " "},
            {"enspace", " "},
            {"qmspace", " "},
            {"bullet", "\x95"},
            {"lquote", "\x91"},
            {"rquote", "\x92"},
            {"ldblquote", "\x93"},
            {"rdblquote", "\x94"},
        };

        /// <summary>
        /// Parse RTF code and return the plain text (not including destinations) and the indexes and lengths of the characters into the original code
        /// </summary>
        /// <param name="input">The rich text code to parse</param>
        /// <param name="sourceDocName">The name to use as debug data for any exceptions</param>
        /// <param name="throwParseExceptions">Whether to throw or only log parse exceptions</param>
        /// <param name="destinationsToOutput">The set of 'RTF destination' names for which text will be output. Null for default collection.</param>
        public static ((int index, int length)[], string) GetTextPositions(string input, string sourceDocName, bool throwParseExceptions, HashSet<string> destinationsToOutput = null)
        {
            if (destinationsToOutput == null)
            {
                destinationsToOutput = OUTPUT_DESTINATIONS;
            }

            Stack<(int, bool)> stack = new Stack<(int, bool)>();
            List<(int, int)> positions = new List<(int, int)>();

            bool ignorable = false; // Whether this group (and all inside it) are to be ignored
            int ucskip = 1; // Number of ASCII characters to skip after a unicode escape sequence
            int curskip = 0;// Number of ASCII characters left to skip
            StringBuilder builder = new StringBuilder();

            foreach (Match match in _tokenMatcher.Value.Matches(input))
            {
                try
                {
                    var groups = match.Groups;
                    var word = groups["Word"];
                    var arg = groups["Arg"];
                    var escape = groups["Escape"];
                    var hex = groups["HexCode"];
                    var openBrace = groups["OpenBrace"];
                    var closeBrace = groups["CloseBrace"];
                    var tchar = groups["Literal"];

                    if (openBrace.Success) // {
                    {
                        curskip = 0;
                        stack.Push((ucskip, ignorable));
                    }
                    // Skip text outside of group, since Nashville RTFs consist of multiple RTF groups with text like "HEADER0001" in between
                    else if (stack.Count == 0)
                    {
                        continue;
                    }
                    else if (closeBrace.Success) // }
                    {
                        ExtractException.Assert("ELI48339", "Invalid group end character; no open group to terminate",
                            stack.Count > 0);
                        curskip = 0;
                        (ucskip, ignorable) = stack.Pop();
                    }
                    else if (escape.Success) // \c
                    {
                        curskip = 0;
                        if (escape.Value == "~") // Non-breaking space
                        {
                            if (!ignorable)
                            {
                                builder.Append('\xA0');
                                positions.Add((escape.Index - 1, 2));
                            }
                        }
                        else if ("{}\\".Contains(escape.Value))
                        {
                            if (!ignorable)
                            {
                                builder.Append(escape.Value);
                                positions.Add((escape.Index - 1, 2));
                            }
                        }
                        else if (escape.Value == "*") // 
                        {
                            ignorable = true;
                        }
                    }
                    else if (word.Success) // \word
                    {
                        curskip = 0;
                        if (DESTINATIONS.Contains(word.Value))
                        {
                            if (!destinationsToOutput.Contains(word.Value))
                            {
                                ignorable = true;
                            }
                        }
                        else if (ignorable)
                        {
                            continue;
                        }
                        else if (SPECIAL_CHARS.TryGetValue(word.Value, out string specialChar))
                        {
                            builder.Append(specialChar);
                            for (int i = 0; i < specialChar.Length; i++)
                            {
                                positions.Add((word.Index - 1, word.Length + 1));
                            }
                        }
                        else if (word.Value == "uc" && arg.Success)
                        {
                            ucskip = int.Parse(arg.Value, CultureInfo.InvariantCulture);
                        }
                        else if (word.Value == "u" && arg.Success)
                        {
                            int c = int.Parse(arg.Value, CultureInfo.InvariantCulture);
                            if (c < 0) // "mid-dot unicode char
                            {
                                c += 0x10000;
                            }

                            char ch = c > 127 ? '^' : Convert.ToChar(c);
                            builder.Append(ch);
                            positions.Add((word.Index - 1, word.Length + 1));

                            curskip = ucskip;
                        }
                    }
                    else if (hex.Success) // \'xx
                    {
                        if (curskip > 0)
                        {
                            curskip--;
                        }
                        else if (!ignorable)
                        {
                            int c = int.Parse(hex.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            char ch = Convert.ToChar(c);
                            builder.Append(ch);
                            positions.Add((hex.Index - 2, 4));
                        }
                    }
                    else if (tchar.Success)
                    {
                        if (curskip > 0)
                        {
                            curskip--;
                        }
                        else if (!ignorable)
                        {
                            builder.Append(tchar.Value);
                            positions.Add((tchar.Index, 1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = throwParseExceptions ? "Error parsing RTF token" : "Application trace: Error parsing RTF token. Skipping...";
                    var uex = new ExtractException("ELI46642", msg, ex);
                    uex.AddDebugData("Source Document", sourceDocName);
                    uex.AddDebugData("Token", match.Value);
                    uex.AddDebugData("Token Index", match.Index);
                    if (throwParseExceptions)
                    {
                        throw uex;
                    }
                    else
                    {
                        uex.Log();
                    }
                }
            }
            if (stack.Count > 0)
            {
                var msg = throwParseExceptions ? "Error parsing RTF: unterminated group" : "Application trace: Error parsing RTF: unterminated group";
                var uex = new ExtractException("ELI48338", msg);
                uex.AddDebugData("Source Document", sourceDocName);
                if (throwParseExceptions)
                {
                    throw uex;
                }
                else
                {
                    uex.Log();
                }
            }

            return (positions.ToArray(), builder.ToString());
        }

        /// <summary>
        /// Parse RTF code and return the plain text (not including destinations) and the indexes and lengths of the characters into the original code encoded as ten bytes per character
        /// </summary>
        /// <param name="input">The rich text code to parse</param>
        /// <param name="sourceDocName">The name to use as debug data for any exceptions</param>
        /// <param name="throwParseExceptions">Whether to throw or only log parse exceptions</param>
        public static byte[] GetIndexedText(string text, string sourceDocName, bool throwParseExceptions)
        {
            try
            {
                var (positions, txt) = GetTextPositions(text, sourceDocName, throwParseExceptions);
                byte[] bytes = new byte[txt.Length * 10];
                for (int i = 0; i < positions.Length; i++)
                {
                    bytes[i * 10] = Convert.ToByte(txt[i]);
                    string position = positions[i].index.ToString("X8", CultureInfo.InvariantCulture);
                    string length = positions[i].length.ToString("X", CultureInfo.InvariantCulture);
                    for (int j = 0; j < 8; j++)
                    {
                        bytes[i * 10 + j + 1] = Convert.ToByte(position[j]);
                    }
                    bytes[i * 10 + 9] = Convert.ToByte(length[0]);
                }

                return bytes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48355");
            }
        }
    }
}
