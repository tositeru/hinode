﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Assertions;
using System.Runtime.Serialization;

namespace Hinode.Editors
{
    public class TextTemplateEngine : ScriptableObject
    {
        public enum Newline
        {
            Newline,
            ReturnAndNewline,
        }
#pragma warning disable CS0649
        [SerializeField, TextArea(15, 9999)] string _templateText;
        [SerializeField] List<Keyword> _keywords = new List<Keyword>();
        [SerializeField] List<IgnorePair> _ignorePairs = new List<IgnorePair>();
        [SerializeField] List<EmbbedTemplate> _embbedTemplates = new List<EmbbedTemplate>();
        //[SerializeField] Dictionary<string, TextTemplateEngine> _embbedDict = new Dictionary<string, TextTemplateEngine>();
        [SerializeField] Newline _newline = Newline.Newline;
#pragma warning restore CS0649

        public string TemplateText { get => _templateText; set => _templateText = value; }
        public IEnumerable<Keyword> Keywords { get => _keywords; }
        public IEnumerable<IgnorePair> IgnorePairs { get => _ignorePairs; }
        public IEnumerable<EmbbedTemplate> EmbbedTemplates { get => _embbedTemplates; }
        //public IEnumerable<KeyValuePair<string, TextTemplateEngine>> EmbbedTemplates { get => _embbedDict; }

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

        public void AddEmbbed(string key, TextTemplateEngine embbedTarget)
        {
            Assert.AreNotEqual(embbedTarget, this, "Don't set self to embbed...");

            _embbedTemplates.Add(new EmbbedTemplate(key, embbedTarget));
        }

        public string Generate()
        {
            var keywordEnumerable = new AllKeywordEnumerable(this);
            if (!keywordEnumerable.Any()) return _templateText;

            string newline = NewLineStr;
            string text = "";
            foreach (var keywordList in keywordEnumerable
                .Where(_l => !_ignorePairs.Any(_igPair =>_igPair.DoIgnore(_l)))
                )
            {
                text += (text.Length <= 0 ? "" : newline)
                    + Generate(_templateText, keywordList);
            }
            return text;
        }

        /// <summary>
        /// 単一のキーワードリストを使用してテキストを生成する
        /// </summary>
        /// <param name="templateText"></param>
        /// <param name="keywordList"></param>
        /// <returns></returns>
        string Generate(string templateText, IEnumerable<(string, string)> keywordList)
        {
            //重い処理なので最適化する？
            var text = templateText;
            foreach (var k in keywordList)
            {
                text = text.Replace($"${k.Item1}$", k.Item2);
            }
            foreach(var pair in _embbedTemplates)
            {
                var embbedText = pair.Template.Generate();
                text = text.Replace($"%{pair.Key}%", embbedText);
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
                var usedKeywordEnumerable = _target._keywords
                    .Where(_k => _k.key != "" && _k.values.Count > 0)
                    .Select(_k => {
                        var e = _k.GetEnumerable().GetEnumerator();
                        e.MoveNext();
                        return e;
                    })
                    .Where(_e => _e.Current != default);
                if (!usedKeywordEnumerable.Any()) yield break;

                var enumeratorList = usedKeywordEnumerable.ToList();
                while(enumeratorList[enumeratorList.Count - 1] != null)
                {
                    yield return enumeratorList.Select(_e => _e.Current);

                    // 次の組み合わせに移動する
                    foreach (var e in enumeratorList)
                    {
                        if (e.MoveNext()) break;
                        if(e == enumeratorList[enumeratorList.Count - 1])
                        {
                            //Loop終了
                            enumeratorList[enumeratorList.Count - 1] = null;
                            break;
                        }
                        e.Reset();
                        e.MoveNext();
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

            [SerializeField] List<KeyStringObject> _pairs = new List<KeyStringObject>();

            public IEnumerable<(string, string)> Pairs { get => _pairs.AsEnumerable().Select(_p => (_p.Key, _p.Value)); }

            public IgnorePair(params (string, string)[] pairs)
            {
                _pairs.AddRange(pairs.Select(_p => new KeyStringObject(_p.Item1, _p.Item2)));
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
    }
}

