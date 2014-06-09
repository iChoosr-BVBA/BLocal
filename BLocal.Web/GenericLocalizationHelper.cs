using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    public class GenericLocalizationHelper<TModel>
    {
        public static readonly Part LocalizedModelPart = new Part(typeof(TModel).Name, new Part("LocalizedModels"));
        public readonly LocalizationHelper Base;
        public readonly TModel Model;

        public GenericLocalizationHelper(LocalizationHelper @base, TModel model)
        {
            Base = @base;
            Model = model;
        }

        public LocalizedHtmlString Input<TProperty>(Expression<Func<TModel, TProperty>> property, String type, String defaultContentValue = null)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return Base.Input(type).Name(info.Path).Id(info.Path + hash).Val(info.Value);
        }

        public LocalizedHtmlString TextArea<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return Base.Tag("textarea").Name(info.Path).Id(info.Path + hash).Val(info.Value);
        }

        public LocalizedHtmlString InputLabel<TProperty>(Expression<Func<TModel, TProperty>> property, String defaultContentValue = null)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return Base.ForPart(LocalizedModelPart).Label(info.Path + hash, info.Key, defaultContentValue);
        }

        public LocalizedHtmlString Radio<TProperty>(Expression<Func<TModel, TProperty>> property, TProperty radioValue)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return Base.Input("radio")
                .Name(info.Path).Id(info.Path + radioValue + hash).Val(radioValue.ToString())
                .AttrIf(info.Value == radioValue.ToString(), "checked", "checked");
        }

        public LocalizedHtmlString RadioLabel<TProperty>(Expression<Func<TModel, TProperty>> property, TProperty radioValue)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return Base.ForPart(LocalizedModelPart).Label(info.Path + radioValue + hash, info.Key);
        }

        public String GetName<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            return GetInfo(property).Path;
        }

        public String GetId<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return info.Path + hash;
        }

        public String GetRadioId<TProperty>(Expression<Func<TModel, TProperty>> property, TProperty radioValue)
        {
            var info = GetInfo(property);
            var hash = Model as Object == null ? 0 : Model.GetHashCode();
            return info.Path + radioValue + hash;
        }

        private ExpressionInfo GetInfo<TProperty>(Expression<Func<TModel, TProperty>> property)
        {
            var path = ExpressionHelper.GetExpressionText((LambdaExpression) property);
            var metadata = ModelMetadata.FromLambdaExpression(property, new ViewDataDictionary<TModel>(Model));
            var metaModel = metadata.Model;
            return new ExpressionInfo(metaModel == null ? String.Empty : metaModel.ToString(), path);
        }

        private static IEnumerable<MemberExpression> Flatten(Expression expression)
        {
            var memberExpression = expression as MemberExpression;
            return memberExpression == null
                ? Enumerable.Empty<MemberExpression>()
                : Flatten(memberExpression.Expression).Concat(new[] {memberExpression});
        }

        private class ExpressionInfo
        {
            public readonly String Value;
            public readonly String Path;
            public readonly String Key;

            public ExpressionInfo(string value, String path)
            {
                Value = value;
                Path = path;
                Key = path.Replace(".", "-");
            }
        }
    }
}
