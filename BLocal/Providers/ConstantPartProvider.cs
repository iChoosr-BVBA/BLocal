using BLocal.Core;

namespace BLocal.Providers
{
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
