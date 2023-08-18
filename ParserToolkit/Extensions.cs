// ReSharper disable UnusedMember.Global

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ParserToolkit;

public static class Extensions
{
    public static bool IsRegexMatch(this string input, string regex)
    {
        return Regex.Match(input, regex, RegexOptions.Compiled).Success;
    }

    public static bool IsRegexMatch(this string input, Regex regex)
    {
        return regex.Match(input).Success;
    }

    public static string ToDot<TToken>(this AstNode<TToken> node) where TToken : Enum
    {
        var builder = new StringBuilder();
        builder.AppendLine("digraph AST {");
        node.BuildDotString(builder);
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void BuildDotString<TToken>(this AstNode<TToken> node, StringBuilder builder) where TToken : Enum
    {
        var nodeType = node.GetType().GetCustomAttribute<AstNodeTypeAttribute>()?.Name ?? node.GetType().Name;

        builder.AppendLine($"{node.Token.Position} [label=\"{nodeType} ({node.Token.Value})\"];");

        foreach (var childProp in node.GetType().GetProperties().Where(p => p.GetCustomAttribute<AstChildAttribute>() != null))
        {
            if (childProp.GetValue(node) is not AstNode<TToken> child) continue;
            builder.AppendLine($"{node.Token.Position} -> {child.Token.Position};");
            BuildDotString(child, builder);
        }
    }
}