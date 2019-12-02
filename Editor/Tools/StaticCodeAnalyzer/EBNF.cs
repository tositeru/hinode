using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hinode.Editors.StaticCodeAnalyzer
{
    /// <summary>
    /// EBNF(extended Backus–Naur form)の実装
    /// </summary>
    public class EBNF
    {
        public EBNFGrammer UseGrammer { get; private set; }

        public EBNF(EBNFGrammer useGrammer)
        {
            UseGrammer = useGrammer;
        }

    }

    /// <summary>
    /// ENBFの文法定義
    /// </summary>
    public class EBNFGrammer
    {
        // terminal symbol
        // [] option
        // {} repeat
        // () grouping

        // メタ識別子 一つ以上の文字からなる
        // EBNFの演算子　(優先順位は最大)
        // * 繰り返しシンボル repetition-symbol
        // - 除外シンボル except-symbol
        // , 連結シンボル concatenate-symbol
        // | 定義分割シンボル definition-separator-symbol
        // = 定義シンボル defining-symbol
        // ; 終端シンボル terminator-symbol
        //

        //括弧の優先順位
        //  ́ first-quote-symbol  ́
        // " second-quote-symbol "
        // (* start/end-comment-symbol *)
        // ( start/end-group-symbol )
        // [ start/end-option-symbol ]
        // { start/end-repeat-symbol }
        // ? special-sequence ?
        // 括弧なし  symbol

        //         構文規則
        // syntax: 一つ以上のSyntax-rule
        // syntax-rule: meta-identifier = definitions-list ;
        // definitions-list: single-definitions | single-definitions | ... 
        // single-definition: syntactic-terms , syntactic-terms , ...
        // syntactic-term: syntactic-factor or syntactic-factor - syntactic-exception
        // syntactic-exception: syntactic-factor(meta-identifiersは含まない)
        // syntactic-factor: integer * syntactic-primary or syntactic-primary
        // integer: 10進数からなる数値
        // syntactic-primary: optional-sequence( [ definitions-list ] )
        //                 or repeated-sequence( { definitions-list } )
        //                 or grouped-sequence( ( definitions-list ) )
        //                 or meta-identifier(初めの文字がmeta-identifier-charactersのローマ字である一つ以上のmeta-identifier-character)
        //                 or terminal-string( ́ 一つ以上のfirst-terminal-character  ́ or " second-terminal-character ")
        //                 or special-sequence(? 空または一つ以上のspecial-sequence-character ?)
        //                 or empty-sequence(空文字列)
        // optional-sequence: [ definitions-list ]
        // repeated-sequence: { definitions-list }
        // grouped-sequence: ( definitions-list )
        // meta-identifier: 初めの文字がmeta-identifier-charactersのローマ字である一つ以上のmeta-identifier-characters
        // meta-identifier-character: ローマ字 or 数字
        // terminal-string:  ́ 一つ以上のfirst-terminal-character  ́ or " 一つ以上のsecond-terminal-character "
        // first-terminal-character:  ́以外のterminal-character
        // second-terminal-character: "以外のterminal-character
        // special-sequence: ? 空または一つ以上のspecial- sequence-character ?
        // special-sequence-character: ?以外のterminal-character
        // empty-sequence: 空文字列
        //
        //          Symbolsの定義
        // terminal-string:  ́ 一つ以上のfirst-terminal-character  ́ or " 一つ以上のsecond-terminal-character "
        // meta-identifier:　syntax-ruleの中のmeta-identifierのリスト。syntactic-primaryのように使用される。
        // grouped-sequence: ()で括られた definitions-listによって定義されたシンボルのリスト
        // optional-sequence: 空または[]で括られたdefinitions-listのリスト
        // repeated-sequence: 空または{}で括られたdefinitions-listのリスト
        // syntactic-factor: 0から指定されたinteger以下のsyntactic-primaryが繰り返される
        // syntactic-term: 展開されたsyntactic-factor
        // single-definition: 対応したsyntactic-termで表現されたシンボルのシーケンスのリスト
        // definitions-list: 一つ以上のsingle-definitionsで表現されたシンボルのシーケンス
        // special-sequence: ユーザー定義の拡張シンボル
        // empty-sequence: 空シンボル
        //
        //          Layoutとコメント
        // terminal-character:
        //      - letter 文字
        //      - decimal-digit 10進数の数字
        //      - concatenate-symbol ,
        //      - defining-symbol = 
        //      - definition-separator-symbol |
        //      - end-comment-symbol *)
        //      - end-group-symbol )
        //      - end-option-symbol ]
        //      - end-repeat-symbol }
        //      - except-symbol -
        //      - first-quote-symbol  ́
        //      - repetition-symbol *
        //      - second-quote-symbol "
        //      - special-sequence-symbol ?
        //      - start-comment-symbol (*
        //      - start-group-symbol (
        //      - start-option-symbol [
        //      - start-repeat-symbol {
        //      - terminator-symbol ;
        //      - other-character
        // gap-free-symbol: first-quote-symbolとsecond-quote-symbolを除いたterminal-character
        // gap-separator: 空白文字,水平タブ,垂直タブ,改行文字,改ページ文字. 構文的に特に意味を持たない
        //      gap-separatorは以下の部分に配置される
        //          - 構文の前
        //          - 構文の後
        //          - 構文内の二つのgap-free-symbolsの間
        // commentless-symbol: 
        //      - concatenate-symbol
        //      - defining-symbol
        //      - definition-separator-symbol
        //      - start/end-group-symbol
        //      - start/end-option-symbol
        //      - start/end-repeat-symbol
        //      - except-symbol
        //      - repetition-symbol
        //      - terminator-symbol
        //      - meta-identifier
        //      - integer
        //      - terminal-string
        //      - special-sequence
        // comment-symbol:
        //      - bracketed-textual-comment
        //      - commentless-symbol
        //      - other-character
        // bracketed-textual-comment:start-comment-symbolから空または一つ以上のcomment-symbolの並び、end-comment-symbolまでもの
        //      構文的に特に意味を持たない
        //      次の場所に配置される
        //          - 構文の前
        //          - 構文の後
        //          - 二つのcommentless-symbolの間
        //
        //      terminal-characterの表現一覧
        // terminal-characterは7bit文字(ISO/IEC 646:1991 International Reference Version)で表現される
        // 
        // Letter and Digit:
        // Other terminal characters: letterとdigit,other-character以外のterminal-character
        // alternative: いくつかのterminal-charactersの代替
        //      - definition-separator-symbol /
        //      - definition-separator-symbol !
        //      - start/end-option-symbol (/ /)
        //      - start/end-repeat-symbol (: :)
        //      - terminator-symbol .
        // other-character:ISO/IEC 646:1991の中から、制御文字とterminal-characterで使用されていない文字
        // gap-separator:
        //      - 空白文字
        //      - 水平タブ文字
        //      - 改行文字 (空でもいい)Carriage Return文字のリストと Line Feedと(空でもいい)Carriage Return文字のシーケンス
        //      - 垂直タブ
        //      - Form Feed character
        // 文字のペアで表現されたterminal-character
        //      - (* *) (: :) (/ /)
        // 不正な文字シーケンス
        //      - (*) (:) (/)
    }
}
/*
(*
    The syntax of Extended BNF can be defined using itself. There are four parts in this example, the first part names the characters, the second part defines the removal of unnecessary non- printing characters, the third part defines the removal of textual comments, and the final part defines the structure of Extended BNF itself.
    Each syntax rule in this example starts with a comment that identifies the corresponding clause in the standard.
    The meaning of special-sequences is not defined in the standard. In this example (see the reference to 7.6) they represent control functions defined by ISO/IEC 6429:1992.
    Another special-sequence defines a
    syntactic-exception (see the reference to 4.7).
*)
(*
    The first part of the lexical syntax defines the characters in the 7-bit character set (ISO / IEC 646:1991) that represent each terminal-character and gap-separator in Extended BNF.
*)
(* see 7.2 *) letter =
    ’a’ | ’b’ | ’c’ | ’d’ | ’e’ | ’f’ | ’g’ | ’h’ |
    ’i’ | ’j’ | ’k’ | ’l’ | ’m’ | ’n’ | ’o’ | ’p’ |
    ’q’ | ’r’ | ’s’ | ’t’ | ’u’ | ’v’ | ’w’ | ’x’ | ’y’ | ’z’ |
    ’A’ | ’B’ | ’C’ | ’D’ | ’E’ | ’F’ | ’G’ | ’H’ |
    ’I’ | ’J’ | ’K’ | ’L’ | ’M’ | ’N’ | ’O’ | ’P’ |
    ’Q’ | ’R’ | ’S’ | ’T’ | ’U’ | ’V’ | ’W’ | ’X’ | ’Y’ | ’Z’;
(* see 7.2 *) decimal digit = ’0’ | ’1’ | ’2’ | ’3’ | ’4’ | ’5’ | ’6’ | ’7’ | ’8’ | ’9’;

(* The representation of the following terminal - characters is defined in clauses 7.3, 7.4 and tables 1, 2. *)
concatenate symbol = ’,’;
defining symbol = ’=’;
definition separator symbol = ’|’ | ’/’ | ’!’; end comment symbol = ’*)’;
end group symbol = ’)’;
end option symbol = ’]’ | ’/)’;
end repeat symbol = ’}’ | ’:)’;
except symbol = ’-’;
first quote symbol = "’";
repetition symbol = ’*’;
second quote symbol = ’"’;
special sequence symbol = ’?’;
start comment symbol = ’(*’;
start group symbol = ’(’;
start option symbol = ’[’ | ’(/’;
start repeat symbol = ’{’ | ’(:’;
terminator symbol = ’;’ | ’.’;
(* see 7.5 *) other character = ’ ’ | ’:’ | ’+’ | ’_’ | ’%’ | ’@’ | ’&’ | ’#’ | ’$’ | ’<’ | ’>’ | ’\’ | ’ˆ’ | ’‘’ | ’ ̃’;
(* see 7.6 *) space character = ’ ’;
horizontal tabulation character = ? ISO 6429 character Horizontal Tabulation
                                  ? ;
new line = { ? ISO 6429 character Carriage Return ? },
         ? ISO 6429 character Line Feed ?,
         { ? ISO 6429 character Carriage Return ? };
vertical tabulation character = ? ISO 6429 character Vertical Tabulation ? ;
form feed = ? ISO 6429 character Form Feed ? ;

(* The second part of the syntax defines the removal of unnecessary non-printing characters from a syntax. *)

(* see 6.2 *) terminal character =
    letter
    | decimal digit
    | concatenate symbol
    | defining symbol
    | definition separator symbol | end comment symbol
    | end group symbol
    | end option symbol
    | end repeat symbol
    | except symbol
    | first quote symbol
    | repetition symbol
    | second quote symbol
    | special sequence symbol
    | start comment symbol
    | start group symbol
    | start option symbol
    | start repeat symbol
    | terminator symbol
    | other character;

(* see 6.3 *) gap free symbol = terminal character - (first quote symbol | second quote symbol) | terminal string;

(* see 4.16 *) terminal string = first quote symbol, first terminal character, {first terminal character}, first quote symbol
    | second quote symbol, second terminal character, {second terminal character}, second quote symbol;

(* see 4.17 *) first terminal character = terminal character - first quote symbol;
(* see 4.18 *) second terminal character = terminal character - second quote symbol;
(* see 6.4 *) gap separator = space character
    | horizontal tabulation character | new line
    | vertical tabulation character | form feed;
(* see 6.5 *) syntax = {gap separator}, gap free symbol, {gap separator}, {gap free symbol, {gap separator}};

(* The third part of the syntax defines the removal of bracketed-textual-comments from gap-free-symbols that form a syntax. *)
(* see 6.6 *) commentless symbol = terminal character
        - ( letter
            | decimal digit
            | first quote symbol
            | second quote symbol
            | start comment symbol
            | end comment symbol
            | special sequence symbol | other character)
    | meta identifier | integer
    | terminal string | special sequence;
(* see 4.9 *) integer = decimal digit, {decimal digit};
(* see 4.14 *) meta identifier = letter, {meta identifier character};
(* see 4.15 *) meta identifier character = letter | decimal digit;
(* see 4.19 *) special sequence = special sequence symbol, {special sequence character}, special sequence symbol;
(* see 4.20 *) special sequence character = terminal character - special sequence symbol;
(* see 6.7 *) comment symbol = bracketed textual comment | other character | commentless symbol;
(* see 6.8 *) bracketed textual comment = start comment symbol, {comment symbol}, end comment symbol;
(* see 6.9 *) syntax = {bracketed textual comment},
    commentless symbol, {bracketed textual comment}, {commentless symbol, {bracketed textual comment}};

(* The final part of the syntax defines the abstract syntax of Extended BNF, i.e. the structure in terms of the commentless symbols. *)
(* see 4.2 *) syntax = syntax rule, {syntax rule};
(* see 4.3 *) syntax rule = meta identifier, defining symbol, definitions list, terminator symbol;
(* see 4.4 *) definitions list = single definition, {definition separator symbol, single definition};
(* see 4.5 *) single definition = syntactic term, {concatenate symbol, syntactic term};
(* see 4.6 *) syntactic term = syntactic factor, [except symbol, syntactic exception];
(* see 4.7 *) syntactic exception = ? a syntactic-factor that could be replaced by a syntactic-factor containing no meta-identifiers
                                    ? ;
(* see 4.8 *) syntactic factor = [integer, repetition symbol], syntactic primary;
(* see 4.10 *) syntactic primary = optional sequence
                                | repeated sequence
                                | grouped sequence
                                | meta identifier
                                | terminal string
                                | special sequence
                                | empty sequence;
(* see 4.11 *) optional sequence = start option symbol, definitions list, end option symbol;
(* see 4.12 *) repeated sequence = start repeat symbol, definitions list, end repeat symbol;
(* see 4.13 *) grouped sequence = start group symbol, definitions list, end group symbol;
(* see 4.21 *) empty sequence = ;
*/
