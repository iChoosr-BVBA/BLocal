using System;
using System.Web.Mvc;
using BLocal.Core;

namespace BLocal.Web
{
    public class SubPart : IDisposable
    {
        private readonly RepositoryWrapper _repository;
        private readonly Part _override;
        private readonly Part _previous;
        private readonly TagBuilder _wrappingTag;
        private readonly HtmlHelper _helper;

        internal SubPart(String subPart, RepositoryWrapper repo)
        {
            _repository = repo;
            _previous = _repository.PartOverride;
            _override = new Part(subPart, repo.Part);
            _repository.PartOverride = _override;
        }
        internal SubPart(String subPart, RepositoryWrapper repo, TagBuilder wrappingTag, HtmlHelper helper)
        {
            _repository = repo;
            _previous = _repository.PartOverride;
            _override = new Part(subPart, repo.Part);
            _repository.PartOverride = _override;
            _wrappingTag = wrappingTag;
            _helper = helper;
            helper.ViewContext.Writer.WriteLine(wrappingTag.ToString(TagRenderMode.StartTag));
        }
        internal SubPart(Part overridingPart, RepositoryWrapper repo)
        {
            _repository = repo;
            _previous = _repository.PartOverride;
            _override = overridingPart;
            _repository.PartOverride = _override;
        }
        internal SubPart(Part overridingPart, RepositoryWrapper repo, TagBuilder wrappingTag, HtmlHelper helper)
        {
            _repository = repo;
            _previous = _repository.PartOverride;
            _override = overridingPart;
            _repository.PartOverride = _override;
            _wrappingTag = wrappingTag;
            _helper = helper;
            helper.ViewContext.Writer.WriteLine(wrappingTag.ToString(TagRenderMode.StartTag));
        }

        public void Dispose()
        {
            _repository.PartOverride = _previous;
            if (_helper != null && _wrappingTag != null)
                _helper.ViewContext.Writer.Write(_wrappingTag.ToString(TagRenderMode.EndTag));
        }
    }
}