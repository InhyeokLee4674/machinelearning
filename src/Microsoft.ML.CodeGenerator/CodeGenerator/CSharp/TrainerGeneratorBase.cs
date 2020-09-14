// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.ML.AutoML;

namespace Microsoft.ML.CodeGenerator.CSharp
{
    /// <summary>
    /// Supports generation of code for trainers (Binary,Multi,Regression,Ranking)
    /// </summary>
    internal abstract class TrainerGeneratorBase : ITrainerGenerator
    {
        private Dictionary<string, string> _arguments;
        private bool _hasAdvancedSettings;
        private string _seperator;
        protected virtual bool IncludeFeatureColumnName => true;

        //abstract properties
        internal abstract string OptionsName { get; }
        internal abstract string MethodName { get; }
        internal abstract IDictionary<string, string> NamedParameters { get; }
        internal abstract string[] Usings { get; }

        /// <summary>
        /// Generates an instance of TrainerGenerator
        /// </summary>
        /// <param name="node"></param>
        protected TrainerGeneratorBase(PipelineNode node)
        {
            Initialize(node);
        }

        private void Initialize(PipelineNode node)
        {
            _arguments = new Dictionary<string, string>();
            if (NamedParameters != null)
            {
                _hasAdvancedSettings = node.Properties.Keys.Any(t => !NamedParameters.ContainsKey(t));
            }
            _seperator = _hasAdvancedSettings ? "=" : ":";
            if (!node.Properties.ContainsKey("LabelColumnName"))
            {
                node.Properties.Add("LabelColumnName", "Label");
            }
            if (IncludeFeatureColumnName)
            {
                node.Properties.Add("FeatureColumnName", "Features");
            }

            foreach (var kv in node.SerializedProperties)
            {
                //For Nullable values.
                if (kv.Value == null)
                    continue;
                var value = kv.Value;
                Type type = kv.Value.GetType();

                if (NamedParameters != null && NamedParameters.Count > 0)
                {
                    _arguments.Add(_hasAdvancedSettings ? kv.Key : NamedParameters[kv.Key], value);
                }
                else
                {
                    _arguments.Add(kv.Key, value);
                }

            }
        }

        internal static string BuildComplexParameter(string paramName, IDictionary<string, string> arguments, string seperator)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("new ");
            sb.Append(paramName);
            sb.Append("(){");
            sb.Append(AppendArguments(arguments, seperator));
            sb.Append("}");
            return sb.ToString();
        }

        internal static string AppendArguments(IDictionary<string, string> arguments, string seperator)
        {
            if (arguments.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var kv in arguments)
            {
                sb.Append(kv.Key);
                sb.Append(seperator);
                sb.Append(kv.Value);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1); //remove the last ,
            return sb.ToString();
        }

        public virtual string GenerateTrainer()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MethodName);
            sb.Append("(");
            if (_hasAdvancedSettings)
            {
                var paramString = BuildComplexParameter(OptionsName, _arguments, "=");
                sb.Append(paramString);
            }
            else
            {
                sb.Append(AppendArguments(_arguments, ":"));
            }
            sb.Append(")");
            return sb.ToString();
        }

        public virtual string[] GenerateUsings()
        {
            if (_hasAdvancedSettings)
                return Usings;

            return null;
        }
    }
}
