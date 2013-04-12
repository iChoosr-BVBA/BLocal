using BLocal.Core;

namespace BLocal.Providers
{
    public class VolatilePartProvider : IPartProvider
    {
        public Part CurrentPart { get; set; }

        public Part GetCurrentPart()
        {
            return CurrentPart;
        }

    }
}
