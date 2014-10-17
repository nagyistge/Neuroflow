using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeatureErrorReportContract))]
    public interface IFeatureErrorReport
    {
        void ReportError(Vector<double> errors);
    }

    [ContractClassFor(typeof(IFeatureErrorReport))]
    class IFeatureErrorReportContract : IFeatureErrorReport
    {
        void IFeatureErrorReport.ReportError(Vector<double> errors)
        {
            Contract.Requires(errors != null);
            Contract.Requires(!errors.IsEmpty);
        }
    }
}
