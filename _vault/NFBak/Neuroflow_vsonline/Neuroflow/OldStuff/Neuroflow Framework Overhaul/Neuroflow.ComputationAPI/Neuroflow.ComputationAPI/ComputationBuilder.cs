using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.IO;
using Neuroflow.Core;
using System.Diagnostics;

namespace Neuroflow.ComputationAPI
{
    public sealed class ComputationBuilder<T> where T : struct
    {
        const int DefParallelLimit = 4;
        
        struct CompiledMethod { internal string call, body; };

        public ComputationBuilder(int parallelLimit = DefParallelLimit)
        {
            Contract.Requires(parallelLimit > 0);

            ParallelLimit = parallelLimit;
        }

        static readonly Registry<string, Type> compiledTypes = new Registry<string, Type>();

        LinkedList<ComputationBlock> blocks = new LinkedList<ComputationBlock>();

        public int ParallelLimit { get; private set; }

        private string TypeName
        {
            get { return typeof(T).FullName; }
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(blocks != null);
            Contract.Invariant(ParallelLimit > 0);
        }

        public void AddBlock(ComputationBlock block)
        {
            Contract.Requires(block != null);

            blocks.AddLast(block);
        }

        public ComputationHandle Compile(ValueSpace<T> valueSpace, string name)
        {
            Contract.Requires(valueSpace != null);
            Contract.Requires(!String.IsNullOrEmpty(name));

            if (blocks.Count == 0) throw new InvalidOperationException("Cannot build computations, blocks is empty.");

            string className = GenerateClassName(valueSpace.StructuralUID, name);

            var type = compiledTypes.GetOrCreate(className, () => CompileType(valueSpace, className));

            IntPtr ptr = valueSpace.CloseInternal();

            var result = (ComputationHandle)Activator.CreateInstance(type, ptr);

            return result;
        }

        private Type CompileType(ValueSpace<T> valueSpace, string className)
        {
            string source = GenerateClassToBuild(className);

            WriteSourceToTemp(source, className);

            var provider = CodeDomProvider.CreateProvider("csharp", new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
            var options = new CompilerParameters();
            options.CompilerOptions = "/target:library /optimize /unsafe";
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            options.IncludeDebugInformation = false;
            AddRefs(options.ReferencedAssemblies);

            // Do compile.
            var results = provider.CompileAssemblyFromSource(options, source);

            if (results.Errors.HasErrors)
            {
                // If error throw ex with compiler errors in ex.Data.
                var ex = new InvalidOperationException("Cannot create proxy. See provided data 'compilerErrors' for details.");
                ex.Data["compilerErrors"] = results.Errors;
                throw ex;
            }

            var type = results.CompiledAssembly.GetType(GetType().Namespace + "." + className);

            return type;
        }

        [Conditional("DEBUG")]
        private static void WriteSourceToTemp(string source, string className)
        {
            string dir = @"c:\temp\neuroflow\";
            Directory.CreateDirectory(dir);
            File.WriteAllText(dir + className + ".cs", source);
        }

        private void AddRefs(StringCollection assemblies)
        {
            // Add all useful references.
            foreach (var asm in GetAllAssemblies())
            {
                if (asm.GlobalAssemblyCache)
                {
                    // GAC.
                    assemblies.Add(asm.ManifestModule.FullyQualifiedName);
                }
                else if (File.Exists(asm.Location))
                {
                    // Non in-memory assemblies.
                    assemblies.Add(asm.Location);
                }
            }
        }

        IEnumerable<Assembly> GetAllAssemblies()
        {
            return (from b in blocks
                    from a in b.References
                    select a).Distinct();
        }

        IEnumerable<string> GetAllNamespaces()
        {
            return (from b in blocks
                    from ns in b.Namespaces
                    select ns).Distinct();
        }

        static string GenerateClassName(Guid valueSpaceUID, string name)
        {
            return typeof(ComputationHandle).Name + "_" + valueSpaceUID.ToString("N") + "_" + name;
        }

        string GenerateClassToBuild(string className)
        {
            var sbCode = new StringBuilder();

            // Namespaces:

            foreach (var ns in GetAllNamespaces())
            {
                sbCode.AppendFormat("using {0};\n", ns);
            }

            sbCode.AppendLine();

            // Add namespace def:

            sbCode.AppendLine("namespace " + GetType().Namespace);
            sbCode.AppendLine("{");

            // Add class:

            sbCode.AppendFormat("public sealed class {0} : {1}\n", className, typeof(ComputationHandle).Name);
            sbCode.AppendLine("{");

            sbCode.AppendLine();

            // Add constructor

            sbCode.Append("public ");
            sbCode.Append(className);
            sbCode.AppendLine("(IntPtr ptr) : base(ptr) { }");
            sbCode.AppendLine();

            // Timeframes:

            var timeframes = from b in blocks.Where(b => !b.IsEmpty).Select((b, i) => new { Block = b, Index = i })
                             orderby b.Block.TimeFrame
                             group b by b.Block.TimeFrame into g
                             select g;

            var timeFrameMethods = new LinkedList<CompiledMethod>();

            foreach (var timeFrame in timeframes)
            {
                var compiledBlocks = timeFrame.Select(b => CompileBlock(b.Block, b.Index)).ToList();
                timeFrameMethods.AddLast(CompileTimeFrame(compiledBlocks, timeFrame.Key));
            }

            // Add Run override:

            sbCode.AppendLine("unsafe public override void Run()");
            sbCode.AppendLine("{");
            sbCode.AppendLine(TypeName + "* value = ((" + TypeName + "*)ptr);");

            foreach (var timeFrame in timeFrameMethods)
            {
                sbCode.AppendLine(timeFrame.call);
            }
            sbCode.AppendLine("}");

            // Add Timeframe Methods:

            foreach (var method in timeFrameMethods)
            {
                sbCode.AppendLine();
                sbCode.Append(method.body);
            }

            sbCode.AppendLine("} // class");
            sbCode.AppendLine("} // ns");

            // Ready:
            return sbCode.ToString();
        }

        CompiledMethod CompileTimeFrame(List<CompiledMethod> blocks, int index)
        {
            Contract.Requires(blocks != null);
            Contract.Requires(index >= 0);

            int blockCount = blocks.Count;

            if (blockCount == 1)
            {
                // Return the block:
                return blocks[0];
            }
            else
            {
                string name = "TF_" + index;
                var bodyBuilder = new StringBuilder();

                // Add TF method:
                bodyBuilder.Append("unsafe private static void ");
                bodyBuilder.Append(name);
                bodyBuilder.Append("(" + TypeName + "* " + ComputationBlock.ValueMemberName + ")");
                bodyBuilder.AppendLine("{");

                if (blockCount >= ParallelLimit * 4)
                {
                    bool start = true;
                    bodyBuilder.AppendLine("Parallel.Invoke(");
                    for (int i = 0; i < blockCount; i++)
                    {
                        if (!start)
                        {
                            bodyBuilder.Append(",\n");
                        }
                        else
                        {
                            start = false;
                        }
                        string format = "\t() => {{{0}}}";
                        var sb = new StringBuilder();
                        for (int j = 0; j < ParallelLimit && i < blockCount; j++, i++)
                        {
                            sb.Append(blocks[i].call);
                        }
                        bodyBuilder.AppendFormat(format, sb);
                    }
                    bodyBuilder.AppendLine(");");
                }
                else
                {
                    foreach (var block in blocks)
                    {
                        bodyBuilder.AppendLine(block.call);
                    }
                }

                bodyBuilder.AppendLine("}");

                // Add Blocks:
                foreach (var block in blocks)
                {
                    bodyBuilder.AppendLine(block.body);
                }

                return new CompiledMethod
                {
                    call = name + "(" + ComputationBlock.ValueMemberName + ");",
                    body = bodyBuilder.ToString()
                };
            }
        }

        CompiledMethod CompileBlock(ComputationBlock block, int index)
        {
            Contract.Requires(block != null);
            Contract.Requires(index >= 0);

            string name = "CB_" + block.TimeFrame + "_" + index;
            var bodyBuilder = new StringBuilder();

            bodyBuilder.Append("unsafe private static void ");
            bodyBuilder.Append(name);
            bodyBuilder.Append("(" + TypeName + "* " + ComputationBlock.ValueMemberName + ")");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine("{");
            //bodyBuilder.AppendLine(TypeName + "* value = ((" + TypeName + "*)ptr);");
            bodyBuilder.Append(block.CodeBuilder);
            bodyBuilder.AppendLine("}");
            
            return new CompiledMethod
            {
                call = name + "(" + ComputationBlock.ValueMemberName + ");",
                body = bodyBuilder.ToString()
            };
        }
    }
}
