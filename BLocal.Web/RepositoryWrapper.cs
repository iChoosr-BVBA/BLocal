using System;
using BLocal.Core;

namespace BLocal.Web
{
    public class RepositoryWrapper
    {
        public LocalizationRepository InnerRepository { get; private set; }
        public Part PartOverride { get; set; }

        public RepositoryWrapper(LocalizationRepository repo, Part partOverride)
        {
            InnerRepository = repo;
            PartOverride = partOverride;
        }
        public RepositoryWrapper(RepositoryWrapper repository, Part part)
        {
            PartOverride = part;
            InnerRepository = repository.InnerRepository;
        }

        public String Get(String key)
        {
            return InnerRepository.Get(new Qualifier.Unique(Part, Locale, key));
        }
        public String Get(Part part, Locale locale, String key)
        {
            return InnerRepository.Get(new Qualifier.Unique(part, locale, key));
        }
        public QualifiedValue GetQualified(String key)
        {
            var value = InnerRepository.GetQualified(new Qualifier.Unique(Part, Locale, key));
            return value;
        }
        public QualifiedValue GetQualified(Part part, Locale locale, String key)
        {
            var value = InnerRepository.GetQualified(new Qualifier.Unique(part, locale, key));
            return value;
        }

        public Part Part { get { return PartOverride ?? InnerRepository.Parts.GetCurrentPart(); } }
        public Locale Locale { get { return InnerRepository.Locales.GetCurrentLocale(); } }
    }
}