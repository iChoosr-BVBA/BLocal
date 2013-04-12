using System;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    public static class HtmlExtentions
    {
        /// <summary>
        /// Provides Localization Helper. Requires availability of LocalizationRepository on MVC DependencyResolver
        /// </summary>
        /// <param name="helper">Current HTML Helper</param>
        public static LocalizationHelper Local(this HtmlHelper helper)
        {
            var context = DependencyResolver.Current.GetService<ILocalizationContext>();
            if (context == null)
                throw new Exception("Could not retrieve LocalizationContext using 'DependencyResolver.Current.GetService<LocalizationContext>()'. Please set up dependency injection for MVC for this to work.");
            
            return new LocalizationHelper(helper, context.DebugMode, new RepositoryWrapper(context.Repository, context.Repository.DefaultPart));
        }

        /// <summary>
        /// Provides Localization Helper.
        /// </summary>
        /// <param name="helper">Current HTML Helper</param>
        /// <param name="repository">Repository to draw localizations from</param>
        /// <param name="debug">Whether or not to allow debug mode</param>
        /// <returns></returns>
        public static LocalizationHelper Local(this HtmlHelper helper, LocalizationRepository repository, bool debug)
        {
            return new LocalizationHelper(helper, debug, new RepositoryWrapper(repository, repository.DefaultPart));
        }
    }

    public interface ILocalizationContext
    {
        LocalizationRepository Repository { get; }
        bool DebugMode { get; }
    }

    public class AlwaysDebuggingContext : ILocalizationContext
    {
        public AlwaysDebuggingContext(LocalizationRepository repository)
        {
            Repository = repository;
        }

        public LocalizationRepository Repository { get; private set; }
        public bool DebugMode { get { return true; } }
    }

    public class NeverDebuggingContext : ILocalizationContext
    {
        public NeverDebuggingContext(LocalizationRepository repository)
        {
            Repository = repository;
        }

        public LocalizationRepository Repository { get; private set; }
        public bool DebugMode { get { return false; } }
    }
}