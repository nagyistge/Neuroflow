using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace ImgNoise.Features
{
    public struct SearchingParams
    {
        #region Constructor

        public SearchingParams(string[] directories, string[] filters, bool recursive = false)
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
            get { return directories == null || filters == null; }
        }

        string[] directories;

        public string[] Directories
        {
            get { return directories; }
        }

        string[] filters;

        public string[] Filters
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
