using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace ParserToolkit.Parser.RecursiveDescent
{
    public class ParserResult<TResult> where TResult : class, new()
    {
        public TResult Result { get; internal set; }
        public IEnumerable<string> Errors { get; internal set; }
    }
}