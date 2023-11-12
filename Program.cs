﻿using System.Text;

var src = File.ReadAllText(args[0]);
var tokens = Lexer.Tokenize(src);
var program = Parser.Parse(tokens);
var result = Interpreter.Run(program);

Console.WriteLine(result);

internal static class Lexer
{
    public static IEnumerable<Token> Tokenize(string src)
    {
        var reader = new StringReader(src);
        var sb = new StringBuilder();

        while (true)
        {
            var token = NextToken();
            
            yield return token;

            if (token.Kind == TokenKind.EOF)
            {
                yield break;
            }
        }

        Token NextToken()
        {
        nextToken:
            var c = reader.Peek();
            if (c == -1)
            {
                return new Token(TokenKind.EOF);
            }

            var ch = (char)c;

            if (char.IsWhiteSpace(ch))
            {
                reader.Read();
                goto nextToken;
            }

            if (char.IsDigit(ch))
            {
                return ReadInteger();
            }

            if (char.IsAsciiLetter(ch))
            {
                return ReadText();
            }

            return ch is '\'' or '"' ? ReadString() : ReadToken();
        }

        Token ReadToken()
        {
            var ch = (char)reader.Read();
            if (TokenKindMap.TryGetValue(ch, out var kind))
            {
                return new Token(kind);
            }
            throw new Exception($"invalid character: '{ch}'");
        }

        Token ReadInteger() => new TokenInteger(int.Parse(ReadChars(char.IsDigit)));

        Token ReadText()
        {
            var word = ReadChars(char.IsAsciiLetterOrDigit);

            return TokenKeywordMap.TryGetValue(word, out var kind)
                ? new Token(kind)
                : TokenBooleanMap.TryGetValue(word, out kind)
                ? new TokenBoolean(kind == TokenKind.True)
                : new TokenIdentifier(word);
        }

        Token ReadString()
        {
            var quote = (char)reader.Read();

            var str = ReadChars(ch => ch != quote);

            var ch = (char)reader.Read();
            if (ch != quote)
            {
                throw new Exception($"string not closed: {str}");
            }

            return new TokenString(str);
        }

        string ReadChars(Func<char, bool> predicate)
        {
            sb.Clear();

            while (true)
            {
                var ch = reader.Peek();
                if (ch == -1 || !predicate((char)ch))
                {
                    break;
                }

                sb.Append((char)reader.Read());
            }

            return sb.ToString();
        }
    }

    static readonly Dictionary<char, TokenKind> TokenKindMap = new()
    {
        { '+', TokenKind.Plus },
        { '-', TokenKind.Minus },
        { '*', TokenKind.Asterisk },
        { '/', TokenKind.Slash },
        { '(', TokenKind.LParen },
        { ')', TokenKind.RParen },
        { ';', TokenKind.SemiColon },
        { '=', TokenKind.Equal },
    };

    static readonly Dictionary<string, TokenKind> TokenKeywordMap = new()
    {
        { "return", TokenKind.Return },
        { "let", TokenKind.Let },
        { "print", TokenKind.Print },
    };

    static readonly Dictionary<string, TokenKind> TokenBooleanMap = new()
    {
        { "true", TokenKind.True },
        { "false", TokenKind.False },
    };
}

internal enum TokenKind
{
    EOF,
    Plus,
    Minus,
    Asterisk,
    Slash,
    LParen,
    RParen,
    Equal,
    SemiColon,
    Integer,
    Identifier,
    String,
    Return,
    Let,
    Print,
    True,
    False,
    Boolean,
}

internal record Token(TokenKind Kind);

internal record TokenInteger(int Value) : Token(TokenKind.Integer);

internal record TokenIdentifier(string Name) : Token(TokenKind.Identifier);

internal record TokenString(string Value) : Token(TokenKind.String);

internal record TokenBoolean(bool Value) : Token(TokenKind.Boolean);

internal static class Parser
{
    public static NodeProgram Parse(IEnumerable<Token> tokenStream)
    {
        var tokens = new Queue<Token>(tokenStream);

        return ParseProgram();

        NodeProgram ParseProgram()
        {
            var statements = ParseStatements().ToList();

            ConsumeToken(TokenKind.EOF);

            return new NodeProgram(statements);
        }

        IEnumerable<NodeStatement> ParseStatements()
        {
            while (true)
            {
                var token = tokens.Peek();
                if (token.Kind != TokenKind.EOF)
                {
                    yield return ParseStatement();
                }
                else
                {
                    yield break;
                }
            }
        }

        NodeStatement ParseStatement()
        {
            var token = tokens.Peek();
            return token.Kind switch
            {
                TokenKind.Return => ParseReturn(),
                TokenKind.Let => ParseLet(),
                TokenKind.Print => ParsePrint(),
                _ => throw new Exception($"invalid token: {token}, expected statement")
            };
        }

        NodeStatement ParseReturn()
        {
            ConsumeToken(TokenKind.Return);

            var value = TryParseValue();

            ConsumeToken(TokenKind.SemiColon);

            return new NodeReturn(value);
        }

        NodeStatement ParseLet()
        {
            ConsumeToken(TokenKind.Let);

            var identifier = ParseIdentifier();

            ConsumeToken(TokenKind.Equal);

            var value = ParseValue();

            ConsumeToken(TokenKind.SemiColon);

            return new NodeLet(identifier, value);
        }

        NodeStatement ParsePrint()
        {
            ConsumeToken(TokenKind.Print);

            var value = TryParseValue();

            ConsumeToken(TokenKind.SemiColon);

            return new NodePrint(value);
        }

        NodeIdentifier ParseIdentifier() => new (ConsumeTokenAs<TokenIdentifier>().Name);

        NodeValue? TryParseValue() => 
            (NodeValue?)TryParseBoolean()
            ?? (NodeValue?)TryParseInteger()
            ?? (NodeValue?)TryParseString()
            ?? TryParseIdentifier();

        NodeString? TryParseString()
        {
            var token = TryConsumeToken<TokenString>();
            return token is null ? null : new (token.Value);
        }

        NodeBoolean? TryParseBoolean()
        {
            var token = TryConsumeToken<TokenBoolean>();
            return token is null ? null : new (token.Value);
        }

        NodeInteger? TryParseInteger()
        {
            var token = TryConsumeToken<TokenInteger>();
            return token is null ? null : new (token.Value);
        }

        NodeIdentifier? TryParseIdentifier()
        {
            var token = TryConsumeToken<TokenIdentifier>();
            return token is null ? null : new (token.Name);
        }

        NodeValue ParseValue()
            => TryParseValue() ?? throw new Exception($"invalid token: {tokens.Peek()}, expected integer, boolean or string value");

        void ConsumeToken(TokenKind kind)
        {
            var token = tokens.Dequeue();
            if (token.Kind != kind)
            {
                throw new Exception($"invalid token: {token}, expected: {kind}");
            }
        }

        TToken? TryConsumeToken<TToken>() where TToken : Token 
            => tokens.Peek() is TToken ? tokens.Dequeue() as TToken : null;

        TToken ConsumeTokenAs<TToken>() where TToken : Token 
            => TryConsumeToken<TToken>() ?? throw new Exception($"invalid token: {tokens.Peek()}, expected: {typeof(TToken)}");
    }
}

internal abstract record Node;

internal abstract record NodeValue : Node;

internal record NodeString(string Value) : NodeValue;

internal record NodeInteger(int Value) : NodeValue;

internal record NodeBoolean(bool Value) : NodeValue;

internal record NodeIdentifier(string Name) : NodeValue;

internal abstract record NodeStatement : Node;

internal record NodeReturn(NodeValue? ReturnValue) : NodeStatement;

internal record NodeLet(NodeIdentifier Identifier, NodeValue Value) : NodeStatement;

internal record NodePrint(NodeValue? PrintValue) : NodeStatement;

internal record NodeProgram(IEnumerable<NodeStatement> Statements) : Node;

internal static class Interpreter
{
    public static object Run(NodeProgram program)
    {
        Dictionary<string, object> variables = new();

        foreach (var statement in program.Statements)
        {
            var value = RunStatement(statement);
            if (value is not null)
            {
                return value;
            }
        }

        return 0;

        object? RunStatement(NodeStatement node)
        {
            // only return statements return a value
            var r = node as NodeReturn;
            if (r is not null)
            {
                return RunReturn(r);
            }

            switch (node)
            {
                case NodeLet l: RunLet(l); break;
                case NodePrint p: RunPrint(p); break;
                default: throw new Exception($"unexpected node: {node}");
            }

            return null;
        }


        void RunLet(NodeLet node)
        {
            // assert variable is not already defined
            if (variables.ContainsKey(node.Identifier.Name))
            {
                throw new Exception($"variable already defined: {node.Identifier.Name}");
            }

            variables[node.Identifier.Name] = RunValue(node.Value);
        }

        void RunPrint(NodePrint node)
        {
            var value = node.PrintValue is null ? "" : RunValue(node.PrintValue);

            var printValue = value switch
            {
                bool b => b ? "true" : "false",
                _ => value.ToString()
            };

            Console.WriteLine(printValue);
        }

        object RunReturn(NodeReturn node) => node.ReturnValue is null ? 0 : RunValue(node.ReturnValue);

        object RunValue(NodeValue node) => node switch
        {
            NodeBoolean b => b.Value,
            NodeInteger i => i.Value,
            NodeString s => s.Value,
            NodeIdentifier i => RunIdentifier(i),
            _ => throw new Exception($"unexpected node: {node}")
        };

        object RunIdentifier(NodeIdentifier node)
        {
            if (!variables.TryGetValue(node.Name, out var value))
            {
                throw new Exception($"variable not defined: {node.Name}");
            }

            return value;
        }
    }
}