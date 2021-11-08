using System;
using System.Collections.Generic;
using System.Text;

namespace RawSql.Core
{
    public sealed class RawSqlStringBuilder
    {
        private StringBuilder Context { get; } = new StringBuilder();
        
        private int Indent { get; set; }
        private bool NewLine { get; set; }

        public RawSqlStringBuilder IncreaseIndent()
        {
             Indent++;
             return this;
        }

        public RawSqlStringBuilder DecreaseIndent()
        {
            Indent--;
            return this;
        }

        public RawSqlStringBuilder Append(char value)
        {
            HandleNewLine();
            Context.Append(value);
            return this;
        }
        
        public RawSqlStringBuilder Append(string value)
        {
            HandleNewLine();
            Context.Append(value);
            return this;
        }

        public RawSqlStringBuilder Space() => Append(' ');
        public RawSqlStringBuilder Comma() => Append(',');
        
        public RawSqlStringBuilder AppendQuoted(string value) => Append($"\"{value}\"");

        public RawSqlStringBuilder AppendMultiple<TItem>(IEnumerable<TItem> items, Action<RawSqlStringBuilder, TItem> processItem, Action<RawSqlStringBuilder> separator)
        {
            var first = true;
            foreach (var item in items)
            {
                if (!first)
                {
                    separator(this);
                }

                processItem(this, item);
                first = false;
            }

            return this;
        }

        public RawSqlStringBuilder AppendLine(string value="")
        {
            Append(value);
            Context.AppendLine();
            NewLine = true;

            return this;
        }
        
        private void HandleNewLine()
        {
            if (NewLine)
            {
                ApplyIndent();
                NewLine = false;
            }
        }

        private void ApplyIndent()
        {
            const char IndentValue = ' ';
            const int RepeatCount = 4;

            if (Indent > 0)
            {
                Context.Append(IndentValue, Indent * RepeatCount);
            }
        }

        public override string ToString() => Context.ToString();
    }
}