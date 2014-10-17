using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Globalization;

namespace NeoComp.Computations
{
    public sealed class CompFormatReader
    {
        static readonly char[] ioSeparators = new char[] { '=', '|' };
        static readonly char[] numberSeparators = new char[] { ' ', ',', ';' };
        
        public CompFormatReader(TextReader reader)
        {
            Contract.Requires(reader != null);

            this.reader = reader;
        }

        public CompFormatReader(string compDefinition)
        {
            Contract.Requires(compDefinition != null);
            Contract.Requires(compDefinition.Length >= 3);
            
            this.reader = new StringReader(compDefinition);
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(reader != null);
        }
        
        TextReader reader;

        public CompFormatEntryCollection Read(int? entryCount = null)
        {
            Contract.Requires(!entryCount.HasValue || entryCount.Value > 0);

            CompFormatEntryCollection result = new CompFormatEntryCollection();

            foreach (var line in ReadLines(entryCount))
            {
                Exception inner = null;
                string[] ioParts = line.Split(ioSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (ioParts.Length == 2)
                {
                    string iPart = ioParts[0];
                    string oPart = ioParts[1];
                    if (!string.IsNullOrEmpty(iPart) && !string.IsNullOrEmpty(oPart))
                    {
                        var inputs = ParsePart(iPart, false, line).Select(v => v.Value).ToArray();
                        var outputs = ParsePart(oPart, false, line).ToArray();
                        try
                        {
                            result.Add(new CompFormatEntry(inputs, outputs));
                            continue;
                        }
                        catch (Exception ex)
                        {
                            inner = ex;
                        }
                    }
                }
                var dex = new InvalidDataException("Comp format error on: '" + line + "'.", inner);
                dex.Data["line"] = line;
                throw dex;
            }

            return result;
        }

        #region Parse

        private IEnumerable<double?> ParsePart(string part, bool nullAllowed, string line)
        {
            string[] entries = part.Split(numberSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (entries.Length > 0)
            {
                bool error = false;
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrEmpty(entry))
                    {
                        if (nullAllowed && entry.Length == 1 && (entry[0] == 'x' || entry[0] == 'X'))
                        {
                            yield return null;
                            continue;
                        }
                        else
                        {
                            double value; 
                            if (double.TryParse(entry, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            {
                                yield return value;
                                continue;
                            }
                        }
                    }
                    error = true;
                    break;
                }
                if (!error) yield break;
            }

            var ex = new InvalidDataException(
                "Comp " + 
                (nullAllowed ? "output" : "input") + 
                " number format error on: '" + 
                line + 
                "'.");
            ex.Data["line"] = line;
            throw ex;
        } 

        #endregion

        #region Read Line

        private IEnumerable<string> ReadLines(int? count)
        {
            return count.HasValue ? ReadLines(count.Value) : ReadAllLines();
        }

        private IEnumerable<string> ReadAllLines()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length != 0) yield return line;
            }
        }

        private IEnumerable<string> ReadLines(int count)
        {
            int idx = 0;
            string line;
            while ((line = reader.ReadLine()) != null && (idx++ != count))
            {
                line = line.Trim();
                if (line.Length != 0) yield return line;
            }
        } 

        #endregion
    }
}
