using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;
using System.Runtime.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace Hinode.Editors
{
    /// <summary>
    /// Text Template Engine
    ///
    /// 元となるテキスト(Template)にパラメータを指定することでテキストを生成することができるクラスになります。
    ///
    /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine"/>
    /// </summary>
    [CreateAssetMenu(fileName = "TextTemplateEngine", menuName = "Hinode/Create TextTemplate")]
    public class TextTemplateEngine : ScriptableObject
    {
        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.SimpleUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.KeywordWithMultipleValueUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        /// <returns></returns>
        public static TextTemplateEngine Create()
        {
            return ScriptableObject.CreateInstance<TextTemplateEngine>();
        }

        public enum Newline
        {
            Newline,
            ReturnAndNewline,
        }
#pragma warning disable CS0649
        [SerializeField] TextTemplateEngine _replacementKeywords;
        [SerializeField] bool _isOnlyEmbbed;
        [SerializeField] bool _doShareKaywords;
        [SerializeField] bool _isSingleKeywordPairMode;
        [SerializeField, TextArea(15, 9999)] string _templateText;
        [SerializeField] List<Keyword> _keywords = new List<Keyword>();
        [SerializeField] List<IgnorePair> _ignorePairs = new List<IgnorePair>();
        [SerializeField] List<EmbbedTemplate> _embbedTemplates = new List<EmbbedTemplate>();
        [SerializeField] List<SingleKeywordPair> _singleKeywordPairList = new List<SingleKeywordPair>();
        //[SerializeField] Dictionary<string, TextTemplateEngine> _embbedDict = new Dictionary<string, TextTemplateEngine>();
        [SerializeField] Newline _newline = Newline.Newline;
#pragma warning restore CS0649

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public TextTemplateEngine ReplacemenetKeywords { get => _replacementKeywords; set => _replacementKeywords = value; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public bool ContainsReplacementKeywords { get => ReplacemenetKeywords != null; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// </summary>
        public bool IsOnlyEmbbed { get => _isOnlyEmbbed; set => _isOnlyEmbbed = value; }

        /// <summary>
        /// 埋め込みTextTemplateがある時、自身のキーワード、無視リストなどのパラメータを使用するかどうかのフラグ。
        /// trueの時は、埋め込みTextTemplateが持つパラメータは使用されないため、
        /// 埋め込みTextTemplateのテンプレートにあるキーワードで自身のパラメータと一致しないものは展開されない場合が出てきますので注意してください。
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public bool DoShareKaywords { get => _doShareKaywords; set => _doShareKaywords = value;}

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public bool IsSingleKeywordPairMode { get => _isSingleKeywordPairMode; set => _isSingleKeywordPairMode = value;}

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.SimpleUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.KeywordWithMultipleValueUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public string TemplateText { get => _templateText; set => _templateText = value; }
        public IEnumerable<Keyword> Keywords { get => _keywords; }
        public IEnumerable<IgnorePair> IgnorePairs { get => _ignorePairs; }
        public IEnumerable<EmbbedTemplate> EmbbedTemplates { get => _embbedTemplates; }
        public IEnumerable<SingleKeywordPair> SingleKeywordPairList { get => _singleKeywordPairList; }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.KeywordWithMultipleValueUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        public string NewLineStr
        {
            get
            {
                switch (_newline)
                {
                    case Newline.Newline: return "\n";
                    case Newline.ReturnAndNewline: return "\r\n";
                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.SimpleUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.KeywordWithMultipleValueUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AddKeyword(string key, params string[] values)
        {
            var keyword = _keywords.FirstOrDefault(_k => _k.key == key);
            if(keyword == null)
            {
                keyword = new Keyword
                {
                    key = key,
                    values = new List<string>(values.AsEnumerable()),
                };
                _keywords.Add(keyword);
            }
            else
            {
                keyword.values = new List<string>(values.AsEnumerable());
            }
        }

        public void RemoveKeyword(string key)
        {
            _keywords.RemoveAll(_k => _k.key == key);
        }

        public void RemoveKeywordAll()
        {
            _keywords.Clear();
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// </summary>
        /// <param name="pairs"></param>
        public void AddIgnorePair(params (string, string)[] pairs)
        {
            _ignorePairs.Add(new IgnorePair(pairs));
        }

        public void RemoveIgnorePair(IgnorePair target)
        {
            _ignorePairs.Remove(target);
        }

        public void RemoveIgnorePairAll()
        {
            _ignorePairs.Clear();
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="embbedTarget"></param>
        public void AddEmbbed(string key, TextTemplateEngine embbedTarget)
        {
            Assert.AreNotEqual(embbedTarget, this, "Don't set self () embbed...");

            _embbedTemplates.Add(new EmbbedTemplate(key, embbedTarget));
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// </summary>
        /// <param name="pairs"></param>
        public void AddSingleKeywordPair(params string[] pairs)
            => AddSingleKeywordPair(pairs.AsEnumerable());
        public void AddSingleKeywordPair(IEnumerable<string> pairs)
        {
            _singleKeywordPairList.Add(new SingleKeywordPair(pairs));
        }

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.SimpleUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.KeywordWithMultipleValueUsagePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyIgnorePairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.EmptyEmbbedTemplatePasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.DoShareKeywordsPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsSingleKeywordPairPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.IsOnlyEmbbedPasses()"/>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.ReplacemenetParamPasses()"/>
        /// </summary>
        /// <returns></returns>
        public string Generate()
            => Generate(this);

        /// <summary>
        /// <seealso cref="Hinode.Tests.Editors.Tools.TestTextTemplateEngine.UseOtherTextTemplatePasses()"/>
        /// </summary>
        /// <param name="useTextTemplate"></param>
        /// <returns></returns>
        public string Generate(TextTemplateEngine useTextTemplate)
        {
            var useKeywords = useTextTemplate.ContainsReplacementKeywords
                ? useTextTemplate.ReplacemenetKeywords
                : useTextTemplate;
            if (IsOnlyEmbbed)
            {
                var text = _templateText;
                return ExpandEmbbedText(text, useTextTemplate);
            }
            else if(useKeywords.IsSingleKeywordPairMode)
            {
                var keywordListEnumerable = useKeywords.SingleKeywordPairList
                        .Select(_pairList =>
                            _pairList.List.Zip(useKeywords.Keywords, (_p, _k) => (_k.key, _p))
                        );
                string text = "";
                foreach (var keywordList in keywordListEnumerable)
                {
                    text += (text.Length <= 0 ? "" : NewLineStr)
                        + Generate(useTextTemplate, _templateText, keywordList);
                }
                return text;
            }
            else
            {
                var keywordEnumerable = new AllKeywordEnumerable(useTextTemplate);
                if (!keywordEnumerable.Any() || useKeywords.IgnorePairs.Any(_i => !_i.IsValid))
                {
                    Debug.LogWarning($"キーワードまたは値が設定されていない無視リストのアイテムが存在しています。変換は行わずに出力します。");
                    return _templateText;
                }

                var enableKeywords = SerachEnableKeywords(_templateText);
                string text = "";
                var keywordListEnumerable = keywordEnumerable
                        .Where(_l => !useKeywords.IgnorePairs.Any(_igPair => _igPair.DoIgnore(_l)))
                        .Select(_l => _l.Where(_k => enableKeywords.Contains(_k.Item1)))
                        ;
                // IEnumerableだと値が固定化されないので、IEnumerable.Distinct()がうまく動作しなかったので、直接キャッシュするようにしている。
                var cacheKeywordList = new HashSet<IEnumerable<(string, string)>>();
                foreach (var keywordList in keywordListEnumerable
                    .Where(_l => !cacheKeywordList.Contains(_l, new CahcedKeywordListEqualityComparer()))
                    )
                {
                    //Debug.Log($"{name} -- keys=>{keywordList.Select(_t => $"{_t.Item1}:{_t.Item2}").Aggregate((_s, _c) => _s + "; " + _c)}");
                    text += (text.Length <= 0 ? "" : NewLineStr)
                        + Generate(useTextTemplate, _templateText, keywordList);
                    cacheKeywordList.Add(keywordList.ToArray()); // <- must need ToArray()!
                }
                //Debug.Log($"{name} keywordList Count={cacheKeywordList.Count}");
                return text;
            }
        }

        class CahcedKeywordListEqualityComparer : IEqualityComparer<IEnumerable<(string, string)>>
        {
            public bool Equals(IEnumerable<(string, string)> x, IEnumerable<(string, string)> y)
            {
                return x.All(_k => y.Any(_y => _k.Item1 == _y.Item1 && _k.Item2 == _y.Item2));
            }

            public int GetHashCode(IEnumerable<(string, string)> obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// 単一のキーワードリストを使用してテキストを生成する
        /// </summary>
        /// <param name="templateText"></param>
        /// <param name="keywordList"></param>
        /// <returns></returns>
        string Generate(TextTemplateEngine useTextTemplate, string templateText, IEnumerable<(string, string)> keywordList)
        {
            //重い処理なので最適化する？
            var text = templateText;
            foreach (var k in keywordList)
            {
                text = text.Replace($"${k.Item1}$", k.Item2);
            }

            return ExpandEmbbedText(text, useTextTemplate);
        }

        readonly Regex _keywordRegex = new Regex(@"\$([^¥$]+)+?\$");
        HashSet<string> SerachEnableKeywords(string templateText)
        {
            HashSet<string> keywords = new HashSet<string>();
            foreach(Match m in _keywordRegex.Matches(templateText))
            {
                var key = m.Groups[1].Value;
                if (!keywords.Contains(key))
                {
                    keywords.Add(key);
                }
            }
            //Debug.Log($"{name} enable Key: {keywords.Aggregate("", (_s, _c) => _s + _c + "; ")}");
            return keywords;
        }

        string ExpandEmbbedText(string text, TextTemplateEngine useTextTemplate)
        {
            foreach (var pair in DoShareKaywords
                ? useTextTemplate.EmbbedTemplates
                : EmbbedTemplates)
            {
                if (pair.Template == this)
                {
                    Debug.LogWarning($"生成中のTextTemplateと一致する埋め込みテキストは展開できません。この埋め込みテキストは展開しません。key={pair.Key}, value={pair.Template}");
                }
                else if (pair.Key != "" && pair.Template != null)
                {
                    var embbedText = pair.Template.Generate(DoShareKaywords ? useTextTemplate : pair.Template);
                    text = text.Replace($"%{pair.Key}%", embbedText);
                }
                else
                {
                    Debug.LogWarning($"キー名または値が設定されていない埋め込みテキストが存在しています。この埋め込みテキストは展開しません。key={pair.Key}, value={pair.Template}");
                }
            }
            return text;
        }

        class AllKeywordEnumerable : IEnumerable<IEnumerable<(string, string)>>, IEnumerable
        {
            TextTemplateEngine _target;
            public AllKeywordEnumerable(TextTemplateEngine textEngine)
            {
                _target = textEngine;
            }

            public IEnumerator<IEnumerable<(string, string)>> GetEnumerator()
            {
                var keywords = _target.ContainsReplacementKeywords
                    ? _target.ReplacemenetKeywords.Keywords
                        //_targetにあるKeyのみを使用する
                        .Where(_k => _target.Keywords.Any(_t => _t.key == _k.key))
                    : _target._keywords;

                var keyAndValueLists = keywords.Where(_k => _k.key != "" && _k.values.Count > 0)
                    .Select(_key => _key.values.Select(_v => (key: _key.key, value: _v)))
                    .ToList();

                var indexList = Enumerable.Repeat(0, keyAndValueLists.Count())
                    .Zip(keyAndValueLists, (_i, _list) => (index: _i, list: _list, len: _list.Count()))
                    .ToList();

                if (indexList.Count <= 0) yield break;
                while(indexList[0].index < indexList[0].len)
                {
                    yield return indexList.Select(_i => _i.list.ElementAt(_i.index));

                    //後ろにあるリストの添字から動かしていく
                    for(var i=indexList.Count()-1; 0 <= i; --i)
                    {
                        var t = indexList[i];
                        t.index += 1;
                        if(t.index < t.len)
                        {
                            indexList[i] = t;
                            break;
                        }
                        else if(i == 0)
                        {
                            indexList[i] = t;
                            break;
                        }
                        else
                        {
                            t.index = 0;
                            indexList[i] = t;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [System.Serializable]
        public class Keyword
        {
            public string key = "";
            public List<string> values = new List<string>();

            public IEnumerable<(string, string)> GetEnumerable()
            {
                return new Enumerable(this);
            }

            class Enumerable : IEnumerable<(string, string)>, IEnumerable
            {
                Keyword _target;
                public Enumerable(Keyword keyword)
                {
                    _target = keyword;
                }

                public IEnumerator<(string, string)> GetEnumerator()
                {
                    return new Enumerator(_target);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                class Enumerator : IEnumerator<(string, string)>, IEnumerator, System.IDisposable
                {
                    Keyword _target;
                    int _index = -1;
                    public Enumerator(Keyword keyword)
                    {
                        _target = keyword;
                    }

                    public (string, string) Current
                    {
                        get
                        {
                            if(0 <= _index && _index < _target.values.Count)
                            {
                                return (_target.key, _target.values[_index]);
                            }
                            else
                            {
                                return default;
                            }
                        }
                    }

                    object IEnumerator.Current => Current;

                    public void Dispose()
                    {
                    }

                    public bool MoveNext()
                    {
                        _index++;
                        return 0 <= _index && _index < _target.values.Count;
                    }

                    public void Reset()
                    {
                        _index = -1;
                    }
                }
            }
        }

        [System.Serializable]
        public class IgnorePair
        {
            //[SerializeField] List<KeyStringObject> _testpairs3 = new List<KeyStringObject>();
            //[SerializeField] List<string> _testpairs2 = new List<string>();
            //[SerializeField] List<KeyValuePair<string, string>> _testpairs = new List<KeyValuePair<string, string>>();

            [SerializeField] List<Pair> _pairs = new List<Pair>();

            public IEnumerable<(string, string)> Pairs { get => _pairs.AsEnumerable().Select(_p => (_p.keyword, _p.value)); }

            public bool IsValid { get => _pairs.All(_p => _p.IsValid); }

            public IgnorePair(params (string, string)[] pairs)
            {
                _pairs.AddRange(pairs.Select(_p => new Pair(_p.Item1, _p.Item2)));
                //foreach(var p in pairs)
                //{
                //    _testpairs3.Add(new KeyStringObject(p.Item1, p.Item2));
                //    _testpairs2.Add(p.Item1);
                //    _testpairs.Add(new KeyValuePair<string, string>(p.Item1, p.Item2));
                //}
            }

            public bool DoIgnore(IEnumerable<(string, string)> keywordPairs)
            {
                return _pairs.All(_p => keywordPairs.Contains(_p));
            }

            [System.Serializable]
            public class Pair
            {
                [SerializeField] public string keyword;
                [SerializeField] public string value;

                public bool IsValid { get => keyword != "" && value != ""; }

                public Pair() : this("", "") { }
                public Pair(string keyword, string value)
                {
                    this.keyword = keyword;
                    this.value = value;
                }

                public static implicit operator (string, string)(Pair t)
                    => (t.keyword, t.value);
            }
        }

        [System.Serializable]
        public class EmbbedTemplate
        {
            [SerializeField] string _key;
            [SerializeField, ForcedObjectField] TextTemplateEngine _template;

            public string Key { get => _key; set => _key = value; }
            public TextTemplateEngine Template { get => _template; set => _template = value; }

            public EmbbedTemplate() { }
            public EmbbedTemplate(string key, TextTemplateEngine template)
            {
                this._key = key;
                this._template = template;
            }
        }

        [System.Serializable]
        public class SingleKeywordPair
        {
            [SerializeField] List<string> _pairKeywords = new List<string>();

            public List<string> List { get => _pairKeywords; }

            public SingleKeywordPair(params string[] pairKeyword)
                : this(pairKeyword.AsEnumerable())
            {}
            public SingleKeywordPair(IEnumerable<string> pairKeywords)
            {
                _pairKeywords = pairKeywords.ToList();
            }
        }
    }
}

