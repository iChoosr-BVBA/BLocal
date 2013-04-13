using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    /// <summary>
    /// Provides extentions to the HtmlHelper to write localized HTML tags.
    /// </summary>
    public class LocalizationHelper
    {
        protected static readonly String[] EmptyStringArray = new String[0];
        public bool Debugmode { get; set; }
        protected HtmlHelper Helper;
        internal RepositoryWrapper Repository { get; private set; }

        /// <summary>
        /// The currently active locale
        /// </summary>
        public Locale Locale { get { return Repository.Locale; } }
        /// <summary>
        /// The currently active part
        /// </summary>
        public Part Part { get { return Repository.Part; } }

        public HtmlHelper HtmlHelper { get { return Helper; } }
        public UrlHelper UrlHelper { get { return new UrlHelper(Helper.ViewContext.RequestContext); } }

        private readonly HashSet<QualifiedValue> _indirectQualifiers;

        public LocalizationHelper(HtmlHelper helper, bool debug, RepositoryWrapper repo)
        {
            Repository = repo;
            Helper = helper;
            Debugmode = debug;

            if (HtmlHelper.ViewData["LocalizationIndirectQualifiers"] as HashSet<QualifiedValue> == null)
                HtmlHelper.ViewData["LocalizationIndirectQualifiers"] = new HashSet<QualifiedValue>();
            _indirectQualifiers = (HtmlHelper.ViewData["LocalizationIndirectQualifiers"] as HashSet<QualifiedValue>);
        }

        public GenericLocalizationHelper<T> Bind<T>(T model)
        {
            return new GenericLocalizationHelper<T>(this, model);
        }

        /// <summary>
        /// Returns pure value for a given key. Not directly hover-debuggable.
        /// </summary>
        /// <param name="key">The key to look for in the repository.</param>
        /// <returns></returns>
        public MvcHtmlString Value(String key)
        {
            var qValue = Repository.GetQualified(key);
            AddIndirectValue(qValue);
            return new MvcHtmlString(qValue.Value.Content);
        }

        /// <summary>
        /// Creates a localized tag of the Header type (H1, H2, ...) of the given level
        /// </summary>
        /// <param name="headingLevel">The level of the header (1-6 for valid html)</param>
        /// <returns></returns>
        public LocalizedHtmlString Heading(int headingLevel)
        {
            return Tag("H" + headingLevel);
        }
        /// <summary>
        /// Creates a localized tag of the Header type (H1, H2, ...) of the given level  with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="headingLevel">The level of the header (1-6 for valid html)</param>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <returns></returns>
        public LocalizedHtmlString Heading(int headingLevel, String innerHtmlKey)
        {
            return Tag("H" + headingLevel, innerHtmlKey);
        }

        /// <summary>
        /// Returns a localized tag of the given type.
        /// </summary>
        /// <param name="tagname">The name of the tag to generate (p, span, div, ...)</param>
        /// <returns></returns>
        public LocalizedHtmlString Tag(String tagname)
        {
            return new LocalizedHtmlString(Repository, tagname, Debugmode);
        }
        /// <summary>
        /// Returns a localized tag of the given type with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="tagname">The name of the tag to generate (p, span, div, ...)</param>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <returns></returns>
        public LocalizedHtmlString Tag(String tagname, String innerHtmlKey)
        {
            return new LocalizedHtmlString(Repository, tagname, Debugmode).HtmlKey(innerHtmlKey);
        }

        /// <summary>
        /// Returns the debuggable textvalue for a key.
        /// </summary>
        /// <param name="key">The key to look for in the repository.</param>
        /// <returns></returns>
        public LocalizedHtmlString Text(String key)
        {
            return Tag("span").HtmlKey(key);
        }
        /// <summary>
        /// Creates a "p" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <returns></returns>
        public LocalizedHtmlString Paragraph(String contentKey)
        {
            return Tag("p").HtmlKey(contentKey);
        }

        /// <summary>
        /// Creates an "a" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <param name="href">value for the "href" attribute of your tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Anchor(String contentKey, String href = "#")
        {
            return Tag("a", contentKey).Attr("href", href);
        }
        /// <summary>
        /// Creates an "a" tag with the translated value of your content key for its title
        /// </summary>
        /// <param name="titleKey">the key for the value of the "title" attriute</param>
        /// <param name="href">value for the "href" attribute of your tag</param>
        /// <returns></returns>
        public LocalizedHtmlString AnchorEmpty(String titleKey, String href = "#")
        {
            return Tag("a").AttrKey("title", titleKey).Attr("href", href);
        }
        /// <summary>
        /// Creates a "label" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="targetId">the Id for the "for" attribute of the generated label</param>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <returns></returns>
        public LocalizedHtmlString Label(String targetId, String contentKey)
        {
            return Tag("label").Attr("for", targetId).HtmlKey(contentKey);
        }
        /// <summary>
        /// Creates an input tag of a given type
        /// </summary>
        /// <param name="type">the type parameter of the input tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Input(String type)
        {
            return Tag("input").Attr("type", type);
        }
        /// <summary>
        /// Creates an input tag of a given type with a given name
        /// </summary>
        /// <param name="type">the type parameter of the input tag</param>
        /// <param name="name">the name parameter of the input tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Input(String type, String name)
        {
            return Tag("input").Attr("type", type).Attr("name", name);
        }

        /// <summary>
        /// Creates an input tag of a given type with a given name
        /// </summary>
        /// <param name="type">the type parameter of the input tag</param>
        /// <param name="name">the name parameter of the input tag</param>
        /// <param name="valueKey">the key with which to fetch the localized value as the value parameter of the input tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Input(String type, String name, String valueKey)
        {
            return Tag("input").Attr("type", type).Attr("name", name).AttrKey("value", valueKey);
        }
        /// <summary>
        /// Creates an input type="button" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="placeholderKey">the key with which to fetch teh content for the "placeholder" attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString InputText(String valueKey, String placeholderKey)
        {
            return Tag("input").Attr("type", "text").AttrKey("value", valueKey).AttrKey("placeholder", placeholderKey);
        }
        /// <summary>
        /// Creates an input type="hidden" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="className">name of the class to set for the input</param>
        /// <returns></returns>
        public LocalizedHtmlString InputHidden(string valueKey, params string[] className)
        {
            AddIndirectValue(Repository.GetQualified(valueKey));
            var tag = Tag("input").AttrKey("value", valueKey).Attr("type", "hidden");
            if (className != null && className.Length > 0)
                tag.Class(className);
            return tag;
        }
        /// <summary>
        /// Creates an input type="button" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString InputButton(String valueKey)
        {
            return Tag("input").Attr("type", "button").AttrKey("value", valueKey);
        }
        /// <summary>
        /// Creates an input type="submit" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString InputSubmit(String valueKey)
        {
            return Tag("input").Attr("type", "submit").AttrKey("value", valueKey);
        }
        /// <summary>
        /// Creates an input type="reset" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString InputReset(String valueKey)
        {
            return Tag("input").Attr("type", "reset").AttrKey("value", valueKey);
        }

        /// <summary>
        /// Creates an HTML5 button tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="className">name of the class to set for the button</param>
        /// <returns></returns>
        public LocalizedHtmlString Button(string valueKey, params string[] className)
        {
            var tag = Tag("button", valueKey);
            tag.Attr("type", "button");
            if (className != null && className.Length > 0)
                tag.Class(className);
            return tag;
        }

        /// <summary>
        /// Creates an a href="" tag with the translated value of your content key inside it and a link to your controller
        /// </summary>
        /// <param name="action">Action to link to</param>
        /// <param name="controller">Controller to link to</param>
        /// <param name="innerHtmlKey">Key for the text inside the a tag</param>
        /// <param name="routeValues">Values to append to the querystring of the link</param>
        /// <returns></returns>
        public LocalizedHtmlString ActionLink(String action, String controller, String innerHtmlKey, Object routeValues = null)
        {
            return Tag("a").Attr("href", new UrlHelper(Helper.ViewContext.RequestContext).Action(action, controller, routeValues)).HtmlKey(innerHtmlKey);
        }
        /// <summary>
        /// Creates an a href="" tag with the translated value of your content key inside it, wrapped in a span tag and a link to your controller
        /// </summary>
        /// <param name="action">Action to link to</param>
        /// <param name="controller">Controller to link to</param>
        /// <param name="innerHtmlKey">Key for the text inside the a tag</param>
        /// <param name="routeValues">Values to append to the querystring of the link</param>
        /// <returns></returns>
        public LocalizedHtmlString ActionLinkSpan(String action, String controller, String innerHtmlKey, Object routeValues = null)
        {
            return Tag("a")
                .Attr("href", new UrlHelper(Helper.ViewContext.RequestContext).Action(action, controller, routeValues))
                .Html(Text(innerHtmlKey).ToHtmlString());
        }

        /// <summary>
        /// Genarates an HTML Image element with a given key whose value will be used as the "src" attribute.
        /// </summary>
        /// <param name="srcKey">The key whose corresponding value will be used as the "src" attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString Image(String srcKey)
        {
            return Tag("img").AttrKey("src", srcKey);
        }

        /// <summary>
        /// Genarates an HTML Image with src attribute and localized alt and title attributes
        /// </summary>
        /// <param name="contentPath">Path for the 'src' attribute (ie "~/Content/image.png")</param>
        /// <param name="altKey">Key for the value to put in the 'alt' attribute</param>
        /// <param name="titleKey">Key for the value to put in the 'title' attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString Image(String contentPath, String altKey, String titleKey)
        {
            return Tag("img")
                .Attr("src", new UrlHelper(Helper.ViewContext.RequestContext).Content(contentPath))
                .AttrKey("alt", altKey)
                .AttrKey("title", titleKey);
        }

        /// <summary>
        /// Genarates an HTML Image with src attribute and localized alt and title attributes that will go to a certain link when clicked
        /// </summary>
        /// <param name="contentPath">Path for the 'src' attribute (ie "~/Content/image.png")</param>
        /// <param name="altKey">Key for the value to put in the 'alt' attribute</param>
        /// <param name="titleKey">Key for the value to put in the 'title' attribute</param>
        /// <param name="action">Action to link to</param>
        /// <param name="controller">Controller to link to</param>
        /// <param name="routeValues">Values to append to the querystring of the link</param>
        /// <returns></returns>
        public LocalizedHtmlString ActionLinkImage(String contentPath, String altKey, String titleKey, String action, String controller, object routeValues = null)
        {
            return Image(contentPath, altKey, titleKey).WithParent(ActionLink(action, controller, String.Empty, routeValues));
        }

        /// <summary>
        /// Creates a select box for a list of items
        /// </summary>
        /// <typeparam name="T">type of object to display in the listbox</typeparam>
        /// <param name="name">value for the select tag's name attribute</param>
        /// <param name="id">value for the select tag's id attribute</param>
        /// <param name="items">items to display in the selectbox</param>
        /// <param name="itemSelected">function which decides which items will be marked as selected</param>
        /// <param name="itemValue">function which decides what to render as value for the item</param>
        /// <param name="itemDisplayKey">function which decides what key to use to fetch the localized display value of the item</param>
        /// <param name="firstValue">value for an extra item inserted as first, not added if NULL</param>
        /// <param name="firstDisplayKey">display key for an extra item inserted as first, not added if NULL</param>
        /// <returns></returns>
        public LocalizedHtmlString Selectbox<T>(String id, String name, IEnumerable<T> items, Func<T, bool> itemSelected, Func<T, object> itemValue, Func<T, string> itemDisplayKey, object firstValue = null, String firstDisplayKey = null)
        {
            var tag = Tag("select").Attr("name", name).Attr("id", id);

            // tuple contains selection - value - displaykey
            var convertedItems = items.Select(item => Tuple.Create(itemSelected(item), itemValue(item), itemDisplayKey(item)));
            if (firstValue != null && firstDisplayKey != null)
                convertedItems = new[] { Tuple.Create(false, firstValue, firstDisplayKey) }.Union(convertedItems);

            if (items != null) {
                var options = new StringBuilder();
                foreach (var item in convertedItems) {
                    var itemTag = Tag("option")
                        .HtmlKey(item.Item3)
                        .Attr("value", item.Item2.ToString());
                    if (item.Item1)
                        itemTag.Attr("selected", "selected");
                    options.Append(itemTag.ToHtmlString());
                }
                tag.Html(options.ToString());
            }

            return tag;
        }

        /// <summary>
        /// Creates an "option" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <param name="value">value for the "value" attribute of your tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Option(String contentKey, String value)
        {
            AddIndirectValue(Repository.GetQualified(contentKey));
            return Tag("option", contentKey).Attr("value", value);
        }

        /// <summary>
        /// Enters a sub-part of the current part until disposed. Please use with a "using" statement.
        /// </summary>
        /// <param name="subPartName">The name of the subpart</param>
        /// <returns></returns>
        public SubPart BeginSubpart(String subPartName)
        {
            return new SubPart(subPartName, Repository);
        }
        /// <summary>
        /// Enters a sub-part of the current part until disposed. Please use with a "using" statement.
        /// The entire subpart will be rendered inside a section tag
        /// The section will have a class attribute with the subpart name as its value
        /// </summary>
        /// <param name="subPartName">The name of the subpart</param>
        /// <returns></returns>
        public SubPart BeginSubpartSection(String subPartName)
        {
            var tag = new TagBuilder("section");
            tag.MergeAttribute("class", subPartName);
            return new SubPart(subPartName, Repository, tag, Helper);
        }

        /// <summary>
        /// Enters a sub-part of the current part until disposed. Please use with a "using" statement.
        /// The entire part will be rendered inside a section tag
        /// The section will have a class attribute with the part name as its value
        /// </summary>
        /// <param name="newPart">The name of the new part</param>
        /// <returns></returns>
        public SubPart BeginPartSection(Part newPart)
        {
            var tag = new TagBuilder("section");
            tag.MergeAttribute("class", newPart.Name);
            return new SubPart (newPart, Repository, tag, Helper);
        }
        /// <summary>
        /// Enters a sub-part of the current part until disposed. Please use with a "using" statement.
        /// The entire subpart will be rendered inside a section tag
        /// The section will have a class attribute with the name as its value
        /// </summary>
        /// <param name="subPartName">The name of the subpart</param>
        /// <param name="sectionAttributes">Other attributes to append to the section element</param>
        /// <returns></returns>
        public SubPart BeginSubpartSection(String subPartName, object sectionAttributes)
        {
            var tag = new TagBuilder("section");
            tag.MergeAttribute("class", subPartName);

            if(sectionAttributes != null)
                foreach(var prop in sectionAttributes.GetType().GetProperties())
                    tag.MergeAttribute(prop.Name, prop.GetValue(sectionAttributes, null).ToString());

            return new SubPart(subPartName, Repository, tag, Helper);
        }
        /// <summary>
        /// Enters a new part (breaks out of the current part) until disposed. Please use with a "using" statement.
        /// </summary>
        /// <param name="newPart">The fully qualified name of the new part</param>
        /// <returns></returns>
        public SubPart BeginPart(Part newPart)
        {
            return new SubPart(newPart, Repository);
        }

        /// <summary>
        /// Returns a new object of the LocalizedExtention class, which operates on a subpart. The original object retains its normal part.
        /// </summary>
        /// <param name="subPartName">The name of the subpart to enter in the new object</param>
        public LocalizationHelper ForSubpart(String subPartName)
        {
            return new LocalizationHelper(Helper, Debugmode, new RepositoryWrapper(Repository, new Part(subPartName, Repository.Part)));
        }
        /// <summary>
        /// Returns a new object of the LocalizedExtention class, which operates on a different part. The original object retains its normal part.
        /// </summary>
        /// <param name="newPart">The name of the new part to inter in the new object</param>
        /// <returns></returns>
        public LocalizationHelper ForPart(Part newPart)
        {
            return new LocalizationHelper(Helper, Debugmode, new RepositoryWrapper(Repository, newPart));
        }

        /// <summary>
        /// Opens a localized HTML tag. Please use .Open() with "using" statement.
        /// </summary>
        /// <param name="tagName">The name of the tag to generate (p, span, div, ...)</param>
        /// <returns></returns>
        public LocalizedHtmlTag BeginTag(String tagName)
        {
            return new LocalizedHtmlTag(Repository, tagName, Debugmode, Helper.ViewContext);
        }
        /// <summary>
        /// Opens a localized HTML tag with an attribute. Please use .Open() with "using" statement.
        /// </summary>
        /// <param name="tagName">The name of the tag to generate (p, span, div, ...)</param>
        /// <param name="localizedAttribute">The attribute to localize</param>
        /// <param name="localizedAttributeKey">The key whose value to insert into the given attribute.</param>
        /// <returns></returns>
        public LocalizedHtmlTag BeginTag(String tagName, String localizedAttribute, String localizedAttributeKey)
        {
            return BeginTag(tagName).AttrKey(localizedAttribute, localizedAttributeKey);
        }
        /// <summary>
        /// Opens a localized HTML tag with multiple attributes. Please use .Open() with "using" statement.
        /// </summary>
        /// <param name="tagName">The name of the tag to generate (p, span, div, ...)</param>
        /// <param name="localizedAttributeMappings">Dictionary of property => key mappings</param>
        /// <returns></returns>
        public LocalizedHtmlTag BeginTag(String tagName, Dictionary<string, string> localizedAttributeMappings)
        {
            return BeginTag(tagName).AttrKey(localizedAttributeMappings);
        }

        public LocalizedHtmlTag BeginForm(String action, String controller, Object routeValues = null, FormMethod method = FormMethod.Post, String target = "_self")
        {
            return BeginTag("form")
                .Attr("action", new UrlHelper(Helper.ViewContext.RequestContext).Action(action, controller, routeValues))
                .Attr("method", method.ToString().ToLowerInvariant())
                .Attr("target", target);
        }

        /// <summary>
        /// Enables debugging via javascript overlay
        /// </summary>
        /// <param name="jsContentPath">Path to the jquery.localization.js file</param>
        /// <param name="cssContentPath">Path to the jquery.localization.css file</param>
        /// <param name="ajaxAction">AJAX Action for the debugger to send localization updates to</param>
        /// <param name="ajaxController">AJAX Controller for the debugger to send localization updates to</param>
        /// <param name="ajaxArea">AJAX Area for the debugger to send localization updates to</param>
        /// <returns></returns>
        public MvcHtmlString JsLink(String jsContentPath = "~/Scripts/jquery.localization.js", String cssContentPath = "~/Content/jquery.localization.css", String ajaxAction = "ChangeValue", String ajaxController = "Localization", String ajaxArea = null)
        {
            var url = new UrlHelper(Helper.ViewContext.RequestContext);
            var ajaxUrl = ajaxArea == null
                ? url.Action(ajaxAction, ajaxController)
                : url.Action(ajaxAction, ajaxController, new { area = ajaxArea });

            var style = new TagBuilder("link");
            style.MergeAttribute("rel", "stylesheet");
            style.MergeAttribute("type", "text/css");
            style.MergeAttribute("href", UrlHelper.Content(cssContentPath));

            var script = new StringBuilder();

            var baseScript = new TagBuilder("script");
            baseScript.MergeAttribute("type", "text/javascript");
            baseScript.MergeAttribute("src", UrlHelper.Content(jsContentPath));
            script.AppendLine(baseScript.ToString());

            var initScript = new TagBuilder("script");
            initScript.MergeAttribute("type", "text/javascript");
            initScript.InnerHtml = String.Format(
                "localization.initialize({0}, '{1}', '{2}', '{3}');",
                Debugmode ? "true" : "false",
                ajaxUrl,
                Repository.Part,
                Repository.Locale
            );
            script.AppendLine(initScript.ToString());

            return new MvcHtmlString(script.ToString());
        }

        /// <summary>
        /// Makes localiztion of all given keys available in JavaScript. Use AFTER JsLink.
        /// </summary>
        /// <returns></returns>
        public LocalizedJavascriptContainer JsLocalize()
        {
            return new LocalizedJavascriptContainer(this);
        }

        /// <summary>
        /// Creates a summary of values that have been but not made debuggable implicitely which gets picked up by the JS debugger. Best put near the end of the page.
        /// </summary>
        /// <returns></returns>
        public MvcHtmlString JsValueSummary()
        {
            if(!Debugmode)
                return new MvcHtmlString(String.Empty);

            var qvJson = _indirectQualifiers.Select(qv =>
                String.Format("{{part:'{0}', locale:'{1}', key: '{2}', value: '{3}', origvalue: '{4}'}}",
                    qv.Qualifier.Part.ToString().Replace("'", @"\'").Replace(@"\", @"\\"),
                    qv.Qualifier.Locale.ToString().Replace("'", @"\'").Replace(@"\", @"\\"),
                    qv.Qualifier.Key.Replace("'", @"\'").Replace(@"\", @"\\"),
                    qv.Value.DecodedContent.Replace("'", @"\'").Replace(@"\", @"\\"),
                    qv.Value.Content.Replace("'", @"\'").Replace(@"\", @"\\")
               )
            );

            var content = String.Format("localization.setOtherValues([{0}])", String.Join(",", qvJson));

            var script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = content;

            return new MvcHtmlString(script.ToString());
        }

        internal void AddIndirectValue(QualifiedValue qv)
        {
            if(!_indirectQualifiers.Contains(qv))
                _indirectQualifiers.Add(qv);
        }
    }
}
