using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Always returns a single constant part.
    /// </summary>
    public class ConstantPartProvider : IPartProvider
    {
        private readonly Part _part;

        public ConstantPartProvider(Part part)
        {
            _part = part;
        }

        public Part GetCurrentPart()
        {
            return _part;
        }
    }
}
