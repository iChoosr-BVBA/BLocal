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

        /// <summary>
        /// Get a generic localization helper which is bound to a model
        /// </summary>
        /// <typeparam name="T">Type of the model</typeparam>
        /// <param name="model">The model to bind to</param>
        /// <returns></returns>
        public GenericLocalizationHelper<T> Bind<T>(T model)
        {
            return new GenericLocalizationHelper<T>(this, model);
        }

        /// <summary>
        /// Returns pure value for a given key. Not directly hover-debuggable.
        /// </summary>
        /// <param name="key">The key to look for in the repository.</param>
        /// <param name="defaultValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public MvcHtmlString Value(String key, String defaultValue = null)
        {
            var qValue = Repository.GetQualified(key, defaultValue);
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
        /// Creates a localized tag of the Heading type (H1, H2, ...) of the given level  with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="headingLevel">The level of the header (1-6 for valid html)</param>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <param name="defaultInnerHtmlValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public LocalizedHtmlString Heading(int headingLevel, String innerHtmlKey, String defaultInnerHtmlValue = null)
        {
            return Tag("H" + headingLevel, innerHtmlKey, defaultInnerHtmlValue);
        }

        /// <summary>
        /// Creates a localized tag of the Heading with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <param name="defaultInnerHtmlValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public LocalizedHtmlString H1(String innerHtmlKey, String defaultInnerHtmlValue = null)
        {
            return Heading(1, innerHtmlKey, defaultInnerHtmlValue);
        }
        /// <summary>
        /// Creates a localized tag of the Heading with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <param name="defaultInnerHtmlValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public LocalizedHtmlString H2(String innerHtmlKey, String defaultInnerHtmlValue = null)
        {
            return Heading(2, innerHtmlKey, defaultInnerHtmlValue);
        }
        /// <summary>
        /// Creates a localized tag of the Heading with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <param name="defaultInnerHtmlValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public LocalizedHtmlString H3(String innerHtmlKey, String defaultInnerHtmlValue = null)
        {
            return Heading(3, innerHtmlKey, defaultInnerHtmlValue);
        }
        /// <summary>
        /// Creates a localized tag of the Heading with an inner value set to the value for a given key.
        /// </summary>
        /// <param name="innerHtmlKey">The key of the inner value</param>
        /// <param name="defaultInnerHtmlValue">Default Value to create if no other value is found</param>
        /// <returns></returns>
        public LocalizedHtmlString H4(String innerHtmlKey, String defaultInnerHtmlValue = null)
        {
            return Heading(4, innerHtmlKey, defaultInnerHtmlValue);
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
        /// <param name="defaultInnerHtmlValue">Default value for the inner html if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Tag(String tagname, String innerHtmlKey, string defaultInnerHtmlValue = null)
        {
            return new LocalizedHtmlString(Repository, tagname, Debugmode).HtmlKey(innerHtmlKey, defaultInnerHtmlValue);
        }

        /// <summary>
        /// Returns the debuggable textvalue for a key.
        /// </summary>
        /// <param name="key">The key to look for in the repository.</param>
        /// <param name="defaultValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Text(String key, String defaultValue = null)
        {
            return Tag("span").HtmlKey(key, defaultValue);
        }

        /// <summary>
        /// Creates a "p" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <param name="defaultContentValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Paragraph(String contentKey, String defaultContentValue = null)
        {
            return Tag("p").HtmlKey(contentKey, defaultContentValue);
        }

        /// <summary>
        /// Creates an "a" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <param name="href">value for the "href" attribute of your tag</param>
        /// <param name="defaultContentValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Anchor(String contentKey, string defaultContentValue = null, String href = "#")
        {
            return Tag("a", contentKey, defaultContentValue).Attr("href", href);
        }

        /// <summary>
        /// Creates an "a" tag with the translated value of your content key for its title
        /// </summary>
        /// <param name="titleKey">the key for the value of the "title" attriute</param>
        /// <param name="href">value for the "href" attribute of your tag</param>
        /// <param name="defaultTitleValue">Default value for the title if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString AnchorEmpty(String titleKey, string defaultTitleValue = null, String href = "#")
        {
            return Tag("a").AttrKey("title", titleKey, defaultTitleValue).Attr("href", href);
        }

        /// <summary>
        /// Creates a "label" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="targetId">the Id for the "for" attribute of the generated label</param>
        /// <param name="contentKey">the key with which to fetch the content</param>
        /// <param name="defaultContentValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Label(String targetId, String contentKey, string defaultContentValue = null)
        {
            return Tag("label").Attr("for", targetId).HtmlKey(contentKey, defaultContentValue);
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
        /// <param name="defaultValueValue">Default value for the Value attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString Input(String type, String name, String valueKey, String defaultValueValue)
        {
            return Tag("input").Attr("type", type).Attr("name", name).AttrKey("value", valueKey, defaultValueValue);
        }

        /// <summary>
        /// Creates an input type="button" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="placeholderKey">the key with which to fetch teh content for the "placeholder" attribute</param>
        /// <param name="defaultPlaceholderValue">Default value for the placeholder if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString InputText(String valueKey, String placeholderKey, String defaultPlaceholderValue)
        {
            return Tag("input").Attr("type", "text").AttrKey("value", valueKey).AttrKey("placeholder", placeholderKey);
        }
        /// <summary>
        /// Creates an input type="hidden" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="defaultValue">Default Value to create if no other value is found</param>
        /// <param name="className">name of the class to set for the input</param>
        /// <returns></returns>
        public LocalizedHtmlString InputHidden(string valueKey, string defaultValue = null, params string[] className)
        {
            AddIndirectValue(Repository.GetQualified(valueKey, defaultValue));
            var tag = Tag("input").AttrKey("value", valueKey).Attr("type", "hidden");
            if (className != null && className.Length > 0)
                tag.Class(className);
            return tag;
        }

        /// <summary>
        /// Creates an input type="button" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="defaultValueValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString InputButton(String valueKey, string defaultValueValue = null)
        {
            return Tag("input").Attr("type", "button").AttrKey("value", valueKey, defaultValueValue);
        }

        /// <summary>
        /// Creates an input type="submit" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="defaultValueValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString InputSubmit(String valueKey, string defaultValueValue = null)
        {
            return Tag("input").Attr("type", "submit").AttrKey("value", valueKey, defaultValueValue);
        }

        /// <summary>
        /// Creates an input type="reset" tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="defaultValueValue">Default value if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString InputReset(String valueKey, string defaultValueValue = null)
        {
            return Tag("input").Attr("type", "reset").AttrKey("value", valueKey, defaultValueValue);
        }

        /// <summary>
        /// Creates an HTML5 button tag with the translated value of your content key inside it
        /// </summary>
        /// <param name="valueKey">the key with which to fetch the content for the "value" attribute</param>
        /// <param name="defaultValueValue">Default value if applicable</param>
        /// <param name="className">name of the class to set for the button</param>
        /// <returns></returns>
        public LocalizedHtmlString Button(string valueKey, string defaultValueValue = null, params string[] className)
        {
            var tag = Tag("button", valueKey, defaultValueValue);
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
        /// <param name="innerHtmlDefaultValue">Default value if applicable</param>
        /// <param name="routeValues">Values to append to the querystring of the link</param>
        /// <returns></returns>
        public LocalizedHtmlString ActionLink(String action, String controller, String innerHtmlKey, string innerHtmlDefaultValue = null, Object routeValues = null)
        {
            return Tag("a").Attr("href", new UrlHelper(Helper.ViewContext.RequestContext).Action(action, controller, routeValues)).HtmlKey(innerHtmlKey, innerHtmlDefaultValue);
        }

        /// <summary>
        /// Creates an a href="" tag with the translated value of your content key inside it, wrapped in a span tag and a link to your controller
        /// </summary>
        /// <param name="action">Action to link to</param>
        /// <param name="controller">Controller to link to</param>
        /// <param name="innerHtmlKey">Key for the text inside the a tag</param>
        /// <param name="innerHtmlDefaultValue">Default value if applicable</param>
        /// <param name="routeValues">Values to append to the querystring of the link</param>
        /// <returns></returns>
        public LocalizedHtmlString ActionLinkSpan(String action, String controller, String innerHtmlKey, string innerHtmlDefaultValue = null, Object routeValues = null)
        {
            return Tag("a")
                .Attr("href", new UrlHelper(Helper.ViewContext.RequestContext).Action(action, controller, routeValues))
                .Html(Text(innerHtmlKey).ToHtmlString());
        }

        /// <summary>
        /// Genarates an HTML Image element with a given key whose value will be used as the "src" attribute.
        /// </summary>
        /// <param name="srcKey">The key whose corresponding value will be used as the "src" attribute</param>
        /// <param name="defaultSrcValue">Devault value for the src attribute if applicable</param>
        /// <returns></returns>
        public LocalizedHtmlString Image(String srcKey, string defaultSrcValue)
        {
            return Tag("img").AttrKey("src", srcKey);
        }

        /// <summary>
        /// Genarates an HTML Image with src attribute and localized alt and title attributes
        /// </summary>
        /// <param name="contentPath">Path for the 'src' attribute (ie "~/Content/image.png")</param>
        /// <param name="altKey">Key for the value to put in the 'alt' attribute</param>
        /// <param name="titleKey">Key for the value to put in the 'title' attribute</param>
        /// <param name="defaultAltValue">Default value for the 'alt' attribute</param>
        /// <param name="defaultTitleValue">Default value for the 'title' attribute</param>
        /// <returns></returns>
        public LocalizedHtmlString Image(String contentPath, String altKey, String titleKey, String defaultAltValue = null, String defaultTitleValue = null)
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
            return Image(contentPath, altKey, titleKey).WithParent(ActionLink(action, controller, String.Empty, String.Empty, routeValues));
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
        /// <param name="itemDefaultValue">fufnction which decides the default value for the display of the item</param>
        /// <param name="firstValue">value for an extra item inserted as first, not added if NULL</param>
        /// <param name="firstDisplayKey">display key for an extra item inserted as first, not added if NULL</param>
        /// <param name="defaultFirstDisplayValue">default value for the first item displayed, not added if NULL</param>
        /// <returns></returns>
        public LocalizedHtmlString Selectbox<T>(String id, String name, IEnumerable<T> items, Func<T, bool> itemSelected, Func<T, object> itemValue, Func<T, string> itemDisplayKey, Func<T, string> itemDefaultValue = null , object firstValue = null, String firstDisplayKey = null, string defaultFirstDisplayValue = null)
        {
            var tag = Tag("select").Attr("name", name).Attr("id", id);

            // tuple contains selection - value - displaykey
            var convertedItems = items.Select(item => Tuple.Create(itemSelected(item), itemValue(item), itemDisplayKey(item), itemDefaultValue == null ? null : itemDefaultValue(item)));
            if (firstValue != null && firstDisplayKey != null)
                convertedItems = new[] { Tuple.Create(false, firstValue, firstDisplayKey, defaultFirstDisplayValue) }.Union(convertedItems);

            if (items != null) {
                var options = new StringBuilder();
                foreach (var item in convertedItems) {
                    var itemTag = Tag("option")
                        .HtmlKey(item.Item3, item.Item4)
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
        /// <param name="defaultContentValue">Default Value to create if no other value is found</param>
        /// <param name="value">value for the "value" attribute of your tag</param>
        /// <returns></returns>
        public LocalizedHtmlString Option(String contentKey, string defaultContentValue = null, Object value = null)
        {
            AddIndirectValue(Repository.GetQualified(contentKey, defaultContentValue));
            var tag = Tag("option", contentKey, defaultContentValue);
            if(value != null)
                tag.Attr("value", value.ToString());
            return tag;
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
        /// <param name="localizedAttributeDefaultValue">The default value for the given attribute</param>
        /// <returns></returns>
        public LocalizedHtmlTag BeginTag(String tagName, String localizedAttribute, String localizedAttributeKey, string localizedAttributeDefaultValue = null)
        {
            return BeginTag(tagName).AttrKey(localizedAttribute, localizedAttributeKey, localizedAttributeDefaultValue);
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
        /// <summary>
        /// Opens a localized HTML form. Please use .Open() with "using" statement.
        /// </summary>
        /// <param name="action">The action the form should navigate to</param>
        /// <param name="controller">The controller for the action to navigate to</param>
        /// <param name="routeValues">Route values for the navigation URL</param>
        /// <param name="method">Method for the form to use</param>
        /// <param name="target">Target to submit the form to (_self, _blank, name of iframe, ...)</param>
        /// <returns></returns>
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
        /// <param name="jsContentPath">Path to the jquery.blocal.js file (if null, assumes you reference this file manually)</param>
        /// <param name="cssContentPath">Path to the jquery.blocal.css file (if null, assumes you reference this file manually)</param>
        /// <param name="ajaxChangeAction">AJAX Action for the debugger to send localization updates to</param>
        /// <param name="ajaxRetrieveAction">AJAX Action for the debugger to retrieve missing localization values from</param>
        /// <param name="ajaxController">AJAX Controller to which change and retrieve actions can be sent</param>
        /// <param name="ajaxArea">The MVC Area in which to find the ajaxController</param>
        /// <returns></returns>
        public MvcHtmlString JsLink(String jsContentPath = "~/Scripts/jquery.blocal.js", String cssContentPath = "~/Content/jquery.blocal.css", String ajaxChangeAction = "ChangeValue", String ajaxRetrieveAction = "GetQualifiedValues", String ajaxController = "Localization", String ajaxArea = null)
        {
            var url = new UrlHelper(Helper.ViewContext.RequestContext);
            var ajaxChangeUrl = ajaxArea == null
                ? url.Action(ajaxChangeAction, ajaxController)
                : url.Action(ajaxChangeAction, ajaxController, new { area = ajaxArea });
            var ajaxRetrieveUrl = ajaxArea == null
                ? url.Action(ajaxRetrieveAction, ajaxController)
                : url.Action(ajaxRetrieveAction, ajaxController, new { area = ajaxArea });

            var link = new StringBuilder();

            if (cssContentPath != null) {
                var style = new TagBuilder("link");
                style.MergeAttribute("rel", "stylesheet");
                style.MergeAttribute("type", "text/css");
                style.MergeAttribute("href", UrlHelper.Content(cssContentPath));
                link.AppendLine(style.ToString());
            }

            if (jsContentPath != null) {
                var baseScript = new TagBuilder("script");
                baseScript.MergeAttribute("type", "text/javascript");
                baseScript.MergeAttribute("src", UrlHelper.Content(jsContentPath));
                link.AppendLine(baseScript.ToString());
            }

            var initScript = new TagBuilder("script");
            initScript.MergeAttribute("type", "text/javascript");
            initScript.InnerHtml = String.Format(
                "blocal.initialize({0}, '{1}', '{2}', '{3}', '{4}');",
                Debugmode ? "true" : "false",
                ajaxChangeUrl,
                ajaxRetrieveUrl,
                Repository.Part,
                Repository.Locale
            );
            link.AppendLine(initScript.ToString());

            return new MvcHtmlString(link.ToString());
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

            var content = String.Format("blocal.addOtherValues([{0}])", String.Join(",", qvJson));

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
