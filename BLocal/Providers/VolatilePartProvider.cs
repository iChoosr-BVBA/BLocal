using BLocal.Core;

namespace BLocal.Providers
{
    /// <summary>
    /// Part provider whose part can be set at any given time. 
    /// </summary>
    public class VolatilePartProvider : IPartProvider
    {
        public Part CurrentPart { get; set; }

        public Part GetCurrentPart()
        {
            return CurrentPart;
        }

    }
}
