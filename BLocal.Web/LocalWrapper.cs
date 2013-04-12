using System;
using System.Web.Mvc;

namespace BLocal.Web
{
    public class LocalWrapper : IDisposable
    {
        private readonly ViewContext _context;
        private readonly TagBuilder _tag;

        internal LocalWrapper(TagBuilder tag, ViewContext context)
        {
            _context = context;
            _tag = tag;

            if (_tag != null)
                _context.Writer.WriteLine(tag.ToString(TagRenderMode.StartTag));
        }

        public void Dispose()
        {
            if (_tag != null)
                _context.Writer.WriteLine(_tag.ToString(TagRenderMode.EndTag));
        }
    }
}