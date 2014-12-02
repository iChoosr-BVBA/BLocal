using System;

namespace BLocal.Web.Manager.Models.ManualSynchronization
{
    public class SynchronizationItem
    {
        public Side Side { get; set; }
        public String Part { get; set; }
        public String Locale  { get; set; }
        public String Key { get; set; }
    }

    public enum Side
    {
        Right,
        Left
    }
}