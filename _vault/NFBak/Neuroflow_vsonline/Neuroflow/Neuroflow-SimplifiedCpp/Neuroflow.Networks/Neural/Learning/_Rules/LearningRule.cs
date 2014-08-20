﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class LearningRule : IEquatable<LearningRule>
    {
        public LearningRule()
        {
            IsEnabled = true;
        }

        [Category(PropertyCategories.Behavior)]
        [InitValue(true)]
        [DefaultValue(true)]
        public bool IsEnabled { get; set; }

        [Category(PropertyCategories.Algorithm)]
        [InitValue(0)]
        [DefaultValue(0)]
        public int GroupID { get; set; }

        public abstract bool IsBeforeIterationRule { get; }

        public abstract bool IsErrorBasedRule { get; }

        public abstract bool NeedsGradientInformation { get; }

        public bool Equals(LearningRule other)
        {
            return other != null ? (GetType() == other.GetType() && GroupID == other.GroupID) : false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LearningRule);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode() ^ GroupID;
        }

        public override string ToString()
        {
            return GetType().Name + ":" + GroupID;
        }
    }
}
