using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//実装する機能のメモ
// macro
// using
// namespace
// class,interface
//   - accessor
//   - static
//   - abstract
//   - inherit, interface
//   - template
//   - attribute
//   - field declare
//      - accessor
//      - new
//      - static
//      - type
//      - attribute
//   - property
//      - accessor
//      - new
//      - static
//      - type
//      - virtual, override
//      - set,get
//      - attribute
//   - method
//      - accessor
//      - new
//      - static
//      - return type
//      - virtual, override
//      - arguments
//      - template
//      - code analysis(Only Method callback, for Stacktrace)
//      - attribute
//   - event,delegate
//      - accessor
//      - new
//      - static
//      - code analysis(Only Method callback, for Stacktrace)
//      - attribute
namespace Hinode.Editors.StaticCodeAnalyzer
{
    public enum TokenType
    {
        Unknown,    //不明なトークン
        Eof,        // Eof
        Newline,    // 改行文字

        Identity, //変数名やクラス名など識別子を表す
        Number,   //数値, 10, 0x34 1.245f 1.24e3

        //Simbol
        Dot,        // .
        Comma,      // ,
        SemiColon,  // ;
        Colon,      // :
        LeftBrace,  // {
        RightBrace, // }
        LeftParen,  // (
        RightParen, // )
        LeftSquareBrackets,     // [
        RightSquareBrackets,    // ]
        Quotation,  // '
        DoubleQuotation, // "
        Question,   // ?
        Exclamation,// !
        Caret,      // ^
        VerticalBar,// |
        Ampersand,  // &
        Tilde,      // ~
        Plus,       // +
        Minus,      // -
        Asterisk,   // *
        Slash,      // '/'
        Percent,    // %
        Assign,     // =
        AtMark,     // @
        Dollar,     // $
        Sharp,      // #
        //
        //Simbolの組み合わせ
        //

        Arrow, // =>
        PointerAccess, // ->
        AliasNamespace, // ::

        // Arithmeric operator
        Increment, // ++
        Decrement, // --
        PlusAssgin, // +=
        MinusAssign, // -=
        MultipleAssign, // *=
        DevideAssgin, // /=
        RemainderAssign, //%=

        // Logical Operator
        Equal, //==
        NotEqual, //!=
        LessEqual, //<=
        GreaterEqual, //>=
        LogicalOr, // ||
        LogicalAnd, // &&

        // Bit operator
        LeftShift, // <<
        RightShift, // >>
        OrAssign, // |=
        AndAssign, // &=
        BitWiseAssign, // ~=
        LeftShiftAssign, // <<=
        RightShiftAssign, // >>=

        // Nullable operator
        NullCoalescing,       // ??
        NullCoalescingAssign, // ??=

        //Keywords
        LineComment,    // '//'
        BeginComment,   // '/*'
        EndComment,     // '*/'

        Using,          // using
        UsingStatic,    // using static
        Namespace,      // namespace
        Global,         // global

        Static,    // static
        Const,     // const
        Volatile,  // volatile
        Readonly,  // readonly

        Operator,  // operator
        Implicit,  // implicit
        Explicit,  // explicit

        Public,    // public
        Protected, // protected
        Internal,  // internal
        Private,   // private
        Extern,    // extern
        Alias,     //alias

        Enum,      // enum
        Class,     // class
        Abstract,  // abstract
        Interface, // interface
        Struct,    // struct
        Base,      // base
        This,      // this
        Partial,   // partial

        Where,      // where
        Unmanaged,  // unmanaged

        Params,     //params

        Override,  // override
        Virtual,   // virtual
        Sealed,    //sealed

        // Property and Indexer
        Set,       // set
        Get,       // get
        Value,     // value

        New,       // new

        Return,    // return
        Break,     // break
        Continue,  // continue
        Yield,     // yield

        Try,       // try
        Catch,     // catch
        Finally,   // finally
        When,       // when

        Null,       // null
        Typeof,     // typeof
        Checked,    // checked
        Unchecked,  // unchecked
        Default,    // default
        NameOf,     // nameof
        SizeOf,     // sizeof
        StackAlloc, // stackalloc

        Delegate,   // delegate
        Event,      // event
        EventAdd,    // add
        EventRemove, // remove

        Lock,       // lock
        Async,      // async
        Await,      // await

        True,       // true
        False,      // false

        Switch, // switch
        Case,   // case
        If,     // if
        Else,   // else
        While,  // while
        For,    // for
        Foreach,// foreach
        Do,     // do
        Goto,   // goto
        In,     // in

        Is,  // is
        As,  // as

        Unsafe, // unsafe
        Fixed,  // fixed

        Ref, // ref
        Out, // out

        Bool,       // bool
        Byte,       // byte
        Char,       // char
        Decimal,    // decimal
        Double,     // double
        Float,      // float
        Int,        // int
        Long,       // long
        Object,     // object
        SByte,      // sbyte
        Short,      // short
        String,     // string
        Uint,       // uint
        ULong,      // ulong
        UShort,     // ushort
        Void,       // void
        Dynamic,    // dynamic
        Var,        // var

        // Query clause 
        Ascending,  // ascending
        By,         // by
        Descending, // descending
        Equals,     // equals
        From,       // from
        Group,      // group
        Into,       // into
        Join,       // join
        Let,        // let
        On,         // on
        OrderBy,    // orderby
        
        Remove,     // remove
        Select,     // select

        //Macro系
        MacroDefine,    // #define
        MacroUndefine,  // #undef
        MacroWarning,   // #warning
        MacroError,     // #error
        MacroLine,      // #line
        MacroProgma,    // #progma
        MacroProgmaWarning, // #progma warning
        MacroProgmaCheckSum, // #progma checksum
        MacroIf,        // #if
        MacroElse,      // #else
        MacroElseIf,    // #elif
        MacroEndIf,     // #endif
        Region,         // #region
        EndRegion,      // #endregion
    }

    public class Token
    {
        public TokenType Type { get; private set; }
        public string Str { get; private set; }
        public int LineNo { get; private set; }
        /// <summary>
        /// 一つ前のTokenとの間に空白文字など余分な文字がないかどうか?
        /// </summary>
        public bool IsContinuedChar { get; private set; }

        public Token(string str, int line, bool isContinuedChar)
        {
            Str = str;
            LineNo = line;
            Type = ToType(str);
            IsContinuedChar = isContinuedChar;
        }

        TokenType ToType(string str)
        {
            switch(str)
            {
                case ".":   return TokenType.Dot;
                case "\n": return TokenType.Newline;
                case "using": return TokenType.Using;
                default:
                    //TokenType.Identity; //変数名やクラス名など識別子を表す
                    //TokenType.Number;  //数値, 10, 0x34 1.245f 1.24e3
                    return TokenType.Unknown;
            }
        }
    }

    public class TokenParser
    {
        public static IEnumerator<Token> ParseToTokens(string source)
        {
            var str = "";
            var line = 0;
            var col = 0;
            var isContinuedChar = false;
            for(var i=0; i<source.Length; ++i)
            {
                if(IsSpaceChar(source[i]))
                {
                    if(str != "")
                        yield return new Token(str, line, isContinuedChar);
                    str = "";
                    col++;
                    isContinuedChar = false;
                }
                else if(IsSimbolChar(source[i]))
                {
                    if(str != "")
                        yield return new Token(str, line, isContinuedChar);

                    str = "";
                    str += source[i];
                    isContinuedChar = true;
                    yield return new Token(str, line, isContinuedChar);

                    if(source[i] == '\n')
                    {
                        line++;
                        col = 0;
                    }
                }
                else if(IsIdentityChar(source[i]))
                {
                    str += source[i];
                    col++;
                }
                else
                {
                    throw new ParserErrorException($"Found Invalid character({source[i]})...", "(raw string)", line, col);
                }
            }
        }

        static bool IsSpaceChar(char ch)
        {
            return ch == ' '
                || ch == '\t'
                || ch == '\r';
        }
        static bool IsIdentityChar(char ch)
        {
            return char.IsLetter(ch)
                || ch == '_'
                ;
        }
        static bool IsSimbolChar(char ch)
        {
            return ch == '.'    // Dot
                || ch == ','    // Comma
                || ch == ';'    // SemiColon
                || ch == ':'    // Colon
                || ch == '{'    // LeftBrace
                || ch == '}'    // RightBrace
                || ch == '('    // LeftParen
                || ch == ')'    // RightParen
                || ch == '['    // LeftSquareBrackets
                || ch == ']'    // RightSquareBrackets
                || ch == '\''   // Quotation
                || ch == '"'    // DoubleQuotation
                || ch == '?'    // Question
                || ch == '!'    // Exclamation
                || ch == '^'    // Caret
                || ch == '|'    // VerticalBar
                || ch == '&'    // Ampersand
                || ch == '~'    // Tilde
                || ch == '+'    // Plus
                || ch == '-'    // Minus
                || ch == '*'    // Asterisk
                || ch == '/'    // Slash
                || ch == '%'    // Percent
                || ch == '='    // Assign
                || ch == '@'    // AtMark
                || ch == '$'    // Dollar
                || ch == '#'    // Sharp
                || ch == '\n'   // Newline
                ;
        }
    }

    public class ParserErrorException : System.Exception
    {
        public string Filepath { get; protected set; }
        public int LineNo { get; private set; }
        public int ColNo { get; private set; }

        public ParserErrorException(string message, string filepath, int line, int col)
            : base($"{filepath}:{line}:{col}:{message}")
        {
            Filepath = filepath;
            LineNo = line;
            ColNo = col;
        }
    }

    public class StaticCodeAnalyzer
    {
    }

}

