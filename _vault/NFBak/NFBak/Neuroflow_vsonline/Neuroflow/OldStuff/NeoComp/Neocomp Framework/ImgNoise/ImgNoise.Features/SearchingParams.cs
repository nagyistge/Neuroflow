using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace ImgNoise.Features
{
    public struct SearchingParams
    {
        #region Constructor

        public SearchingParams(Strings directories, Strings filters, bool recursive = false)
        {
            this.directories = directories;
            this.filters = filters;
            this.recursive = recursive;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [Pure]
            get { return directories.IsNullOrEmpty() || filters.IsNullOrEmpty(); }
        }

        Strings directories;

        public Strings Directories
        {
            get { return directories; }
        }

        Strings filters;

        public Strings Filters
        {
            get { return filters; }
        }

        bool recursive;

        public bool Recursive
        {
            get { return recursive; }
        }

        #endregion
    }
}
