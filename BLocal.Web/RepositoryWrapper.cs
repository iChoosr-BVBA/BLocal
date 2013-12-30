using System;
using BLocal.Core;

namespace BLocal.Web
{
    public class RepositoryWrapper
    {
        public LocalizationRepository InnerRepository { get; private set; }
        public Part PartOverride { get; set; }
        public string ABSegment { get; private set; }

        public RepositoryWrapper(LocalizationRepository repo, Part partOverride, String abSegment = null)
        {
            InnerRepository = repo;
            PartOverride = partOverride;
            ABSegment = abSegment;
        }
        public RepositoryWrapper(RepositoryWrapper repository, Part part, String abSegment = null)
        {
            PartOverride = part;
            ABSegment = abSegment;
            InnerRepository = repository.InnerRepository;
        }

        public String Get(String key, String defaultValue = null)
        {
            return InnerRepository.Get(new Qualifier.Unique(GetSegmentPart(Part), Locale, key), defaultValue);
        }
        public String Get(Part part, Locale locale, String key, String defaultValue = null)
        {
            return InnerRepository.Get(new Qualifier.Unique(GetSegmentPart(part), locale, key), defaultValue);
        }
        public QualifiedValue GetQualified(String key, String defaultValue = null)
        {
            var value = InnerRepository.GetQualified(new Qualifier.Unique(GetSegmentPart(Part), Locale, key), defaultValue);
            return value;
        }
        public QualifiedValue GetQualified(Part part, Locale locale, String key, String defaultValue = null)
        {
            var value = InnerRepository.GetQualified(new Qualifier.Unique(GetSegmentPart(part), locale, key), defaultValue);
            return value;
        }

        private Part GetSegmentPart(Part part)
        {
            return ABSegment == null ? part : new Part(ABSegment, part);
        }

        public Part Part { get { return PartOverride ?? InnerRepository.Parts.GetCurrentPart(); } }
        public Locale Locale { get { return InnerRepository.Locales.GetCurrentLocale(); } }
    }
}