using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    public sealed class TextItemReader : IEnumerable<IEnumerable>
    {
        public TextItemReader(TextReader reader)
            : this(reader, new[] { ' ', ',', ';', ':', '\t' })
        {
        }

        public TextItemReader(TextReader reader, char[] columnSeparators)
        {
            Contract.Requires(reader != null);
            Contract.Requires(!columnSeparators.IsNullOrEmpty());

            this.reader = reader;
            this.columnSeparators = columnSeparators;
        }
        
        TextReader reader;

        char[] columnSeparators;

        int? columnSize;

        int lineIndex;

        public IEnumerator<IEnumerable> GetEnumerator()
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return SplitLine(line);
                lineIndex++;
            }
        }

        private IEnumerable SplitLine(string line)
        {
            string[] parts = line.Split(columnSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                if (columnSize == null)
                {
                    columnSize = parts.Length;
                }
                else if (columnSize != parts.Length)
                {
                    throw new InvalidDataException("Invalid number of item on line: " + line + ".");
                }
                return parts;
            }
            return Enumerable.Repeat("", 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
