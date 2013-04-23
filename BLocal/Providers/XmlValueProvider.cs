using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Provides xml-driven localization. Editing values will get slower with larger amounts of values stored.
    /// </summary>
    public class XmlValueProvider : ILocalizedValueManager
    {
        private readonly Dictionary<String, GroupNode> _groups = new Dictionary<String, GroupNode>();
        private readonly String _file;
        private readonly bool _insertDummyValues;

        #region XML operations

        /// <summary>
        /// Creates a new XmlValueProvider based on a given xml file.
        /// </summary>
        /// <param name="file">Name of the file in which to store all localization. Will be created if it doesn't exist.</param>
        /// <param name="insertDummyValues">If set to true, will not throw ValueNotFoundExceptions (which trigger the Notifier), but create dummy values instead.</param>
        public XmlValueProvider(String file, bool insertDummyValues = false)
        {
            _file = file;
            _insertDummyValues = insertDummyValues;
            Reload();
        }

        public void Reload()
        {
            lock (_groups)
            {
                _groups.Clear();
                lock (_file)
                {
                    File.Open(_file, FileMode.OpenOrCreate).Dispose();
                    var doc = new XmlDocument();
                    try {
                        doc.Load(_file);
                    }
                    catch (XmlException e) {
                        // thanks, XmlDocument!
                        if (e.Message == "Root element is missing.") {
                            doc.LoadXml("<mapping />");
                            doc.Save(_file);
                        }
                    }
                    var mappingElement = doc.DocumentElement;
                    Debug.Assert(mappingElement != null, "mappingElement != null");
                    if (mappingElement.Name != "mapping")
                        throw new Exception("Expected root element named 'mapping'");

                    foreach (var element in mappingElement.ChildNodes.OfType<XmlElement>())
                        ProcessPartnode(element, null);
                }
            }
        }
        private void ProcessPartnode(XmlElement partElement, GroupNode parent)
        {
            Debug.Assert(partElement.Attributes != null, "partNode.Attributes != null");
            var group = new GroupNode(partElement.Attributes["id"].Value, parent);
            _groups.Add(group.ToString(), group);
            foreach (var elem in partElement.ChildNodes.OfType<XmlElement>())
                if (elem.Name.Equals("part"))
                    ProcessPartnode(elem, group);
                else
                    ProcessTextnode(elem, group);
        }
        private static void ProcessTextnode(XmlElement textNode, GroupNode group)
        {
            var languages = textNode.ChildNodes.OfType<XmlElement>()
                .ToDictionary(langNode => langNode.Attributes["id"].Value, langNode => langNode.InnerXml);
            group.Content.Add(textNode.Attributes["key"].Value, languages);
        }

        public void Save()
        {
            lock (_groups)
            {
                lock (_file)
                {
                    using (var writer = XmlWriter.Create(_file, new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = false }))
                    {
                        writer.WriteStartElement("mapping");

                        foreach (var group in _groups.Values.Where(g => g.Parent == null))
                            WriteGroup(writer, group);

                        writer.WriteEndElement();
                    }
                }
            }
        }
        private static void WriteGroup(XmlWriter writer, GroupNode group)
        {
            writer.WriteStartElement("part");
            writer.WriteAttributeString("id", group.Id);

            foreach (var subgroup in group.GetChildren())
                WriteGroup(writer, subgroup);
            WriteText(writer, group);

            writer.WriteEndElement();
        }
        private static void WriteText(XmlWriter writer, GroupNode group)
        {
            foreach (var text in group.Content)
            {
                writer.WriteStartElement("text");
                writer.WriteAttributeString("key", text.Key);
                foreach (var language in text.Value)
                {
                    writer.WriteStartElement("lang");
                    writer.WriteAttributeString("id", language.Key);
                    writer.WriteRaw(language.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        public void Export(Stream exportStream, List<String> locales)
        {
            lock (_groups)
            {
                lock (_file)
                {
                    using (var writer = XmlWriter.Create(exportStream, new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineOnAttributes = false }))
                    {
                        writer.WriteStartElement("mappingexport");
                        foreach (var group in _groups)
                        {
                            writer.WriteStartElement("part");
                            writer.WriteAttributeString("path", group.Key);
                            foreach (var content in group.Value.Content)
                            {
                                writer.WriteStartElement("content");
                                writer.WriteAttributeString("key", content.Key);
                                foreach (var language in locales)
                                {
                                    writer.WriteStartElement("value");
                                    writer.WriteAttributeString("locale", language);
                                    // if it doesn't exist in this language, leave it empty
                                    try { writer.WriteRaw(content.Value[language]); }
                                    catch (KeyNotFoundException) { writer.WriteValue(""); }
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
            }
        }
        public void Import(Stream importStream, HashSet<String> locales, bool save)
        {
            lock (_groups)
            {
                lock (_file)
                {
                    var doc = new XmlDocument();
                    doc.Load(importStream);
                    Debug.Assert(doc.DocumentElement != null, "doc.DocumentElement != null");
                    foreach (var group in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
                    {
                        var groupPath = group.Attributes["path"].InnerText;
                        foreach (var content in group.ChildNodes.OfType<XmlElement>())
                        {
                            var contentKey = content.Attributes["key"].InnerText;
                            foreach (var contentValue in content.ChildNodes.OfType<XmlElement>())
                            {
                                var locale = contentValue.Attributes["locale"].InnerText;
                                if (locales.Contains(locale))
                                {
                                    try
                                    {
                                        _groups[groupPath].Content[contentKey][locale] = contentValue.InnerXml;
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        try
                                        {
                                            _groups[groupPath].Content[contentKey].Add(locale, contentValue.InnerXml);
                                        }
                                        catch (KeyNotFoundException e)
                                        {
                                            throw new Exception(String.Format("The file contained a key or path that does not exist! ({0} - {1})", groupPath, contentKey), e);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (save)
                Save();
        }

        #endregion

        #region IValueProvider

        public string GetValue(Qualifier.Unique qualifier)
        {
            try {
                try {
                    lock (_groups)
                        return _groups[qualifier.Part.ToString()].GetValue(qualifier);
                }
                catch(KeyNotFoundException e) {
                    throw new ValueNotFoundException(qualifier, e);
                }
            }
            catch(ValueNotFoundException) {
                if (!_insertDummyValues)
                    throw;

                CreateValue(qualifier, "[-" + qualifier.Key + "-]");
                return GetValue(qualifier);
            }
        }
        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier)
        {
            try {
                try {
                lock (_groups)
                    return _groups[qualifier.Part.ToString()].GetQualified(qualifier);
                }
                catch (KeyNotFoundException e) {
                    throw new ValueNotFoundException(qualifier, e);
                }
            }
            catch (ValueNotFoundException) {
                if (!_insertDummyValues)
                    throw;

                CreateValue(qualifier, "[" + qualifier.Key + "]");
                return GetQualifiedValue(qualifier);
            }
        }
        public void SetValue(Qualifier.Unique qualifier, string value)
        {
            lock (_groups)
                _groups[qualifier.Part.ToString()].SetValue(qualifier, value);
            Save();
        }

        #endregion

        #region IValueManager

        public void UpdateCreateValue(QualifiedValue value)
        {
            var group = GetOrCreateGroupNode(value.Qualifier.Part);
            group.SetOrCreateValue(value.Qualifier, value.Value.Content);
            Save();
        }

        public void CreateValue(Qualifier.Unique qualifier, string value)
        {
            var group = GetOrCreateGroupNode(qualifier.Part);
            group.SetOrCreateValue(qualifier, value);
            Save();
        }
        public IEnumerable<QualifiedValue> GetAllValuesQualified()
        {
            return _groups.SelectMany(group => group.Value.GetAllQualified());
        }
        public void DeleteValue(Qualifier.Unique qualifier)
        {
            _groups[qualifier.Part.ToString()].RemoveValue(qualifier, node => { lock (_groups) { _groups.Remove(node.ToString()); } });
        }
        public void DeleteLocalizationsFor(Part part, String key)
        {
            foreach(var localization in _groups[part.ToString()].GetAllQualified().Where(qv => qv.Qualifier.Key == key).ToArray())
                DeleteValue(localization.Qualifier);
        }

        private GroupNode GetOrCreateGroupNode(Part part)
        {
            var partString = part.ToString();
            if (_groups.ContainsKey(partString))
                return _groups[partString];

            var hierarchy = part.StackHierarchy();

            GroupNode node = null;
            foreach (var hierarchyPart in hierarchy) {
                var hierarchyPartString = hierarchyPart.ToString();
                if (_groups.ContainsKey(hierarchyPartString))
                    node = _groups[hierarchyPartString];
                else {
                    node = new GroupNode(hierarchyPart.Name, node);
                    _groups.Add(hierarchyPartString, node);
                }
            }
            return node;
        }

        #endregion

        #region Group node

        private class GroupNode
        {
            private const String Separator = Part.Separator;
            private const String Format = Separator + "{0}";
            private readonly List<GroupNode> _children = new List<GroupNode>();

            public GroupNode Parent { get; private set; }
            public String Id { get;  private set; }
            public Dictionary<String, Dictionary<String, String>> Content { get; private set; }

            public GroupNode(String id, GroupNode parent)
            {
                Content = new Dictionary<String, Dictionary<String, String>>();
                Id = id;
                Parent = parent;
                if (Parent != null)
                    parent.AddChild(this);
            } 

            public String GetValue(Qualifier.Unique qualifier)
            {
                try
                {
                    return Content[qualifier.Key][qualifier.Locale.ToString()];
                }
                catch (KeyNotFoundException)
                {
                    if (Parent == null)
                        throw new ValueNotFoundException(qualifier);
                    return Parent.GetValue(qualifier);
                }
            }
            public QualifiedValue GetQualified(Qualifier qualifier)
            {
                try
                {
                    return new QualifiedValue(new Qualifier.Unique(Part.Parse(ToString()), qualifier.Locale, qualifier.Key), new Value(ContentType.Unspecified, Content[qualifier.Key][qualifier.Locale.ToString()]));
                }
                catch (KeyNotFoundException)
                {
                    if (Parent == null)
                        throw new ValueNotFoundException(qualifier);
                    return Parent.GetQualified(qualifier);
                }
            }
            public void SetValue(Qualifier.Unique qualifier, String value)
            {
                try
                {
                    Content[qualifier.Key][qualifier.Locale.ToString()] = value;
                }
                catch (KeyNotFoundException)
                {
                    if (Parent == null)
                        throw new ValueNotFoundException(qualifier);
                    Parent.SetValue(qualifier, value);
                }
            }
            public void SetOrCreateValue(Qualifier.Unique qualifier, String value)
            {

                if(!Content.ContainsKey(qualifier.Key))
                    Content.Add(qualifier.Key, new Dictionary<String, String>());
                var keyContent = Content[qualifier.Key];

                if(!keyContent.ContainsKey(qualifier.Locale.ToString()))
                    keyContent.Add(qualifier.Locale.ToString(), value);
                else
                    keyContent[qualifier.Locale.ToString()] = value;
            }

            public override String ToString()
            {
                return ToStringBuilder().ToString();
            }
            private StringBuilder ToStringBuilder()
            {
                return Parent == null
                    ? new StringBuilder(Id)
                    : Parent.ToStringBuilder().AppendFormat(Format, Id);
            }

            public IEnumerable<GroupNode> GetChildren()
            {
                return _children;
            }
            private void AddChild(GroupNode child)
            {
                _children.Add(child);
            }

            public IEnumerable<QualifiedValue> GetAllQualified()
            {
                var myPart = Part.Parse(ToString());
                lock(Content)
                    foreach (var keyContainer in Content)
                        foreach (var localContainer in keyContainer.Value)
                            yield return new QualifiedValue(new Qualifier.Unique(myPart, new Locale(localContainer.Key), keyContainer.Key), new Value(ContentType.Unspecified, localContainer.Value));
            }

            public void RemoveValue(Qualifier.Unique qualifier, Action<GroupNode> killNode)
            {
                Content[qualifier.Key].Remove(qualifier.Locale.ToString());
                if (Content[qualifier.Key].Count == 0)
                    Content.Remove(qualifier.Key);
                SuicideIfEmpty(killNode);
            }

            private void SuicideIfEmpty(Action<GroupNode> killNode)
            {
                if (_children.Count != 0 || Content.Count != 0)
                    return;

                killNode(this);
                Parent.RemoveNode(this);
                Parent.SuicideIfEmpty(killNode);
            }

            private void RemoveNode(GroupNode node)
            {
                lock(_children)
                    _children.Remove(node);
            }
        }

        #endregion
    }
}
