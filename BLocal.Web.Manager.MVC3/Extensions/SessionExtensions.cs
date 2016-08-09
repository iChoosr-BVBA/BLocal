using System;
using System.Web;

namespace BLocal.Web.Manager.Extensions
{
    public static class SessionExtensions
    {
        public static void Clear(this HttpSessionStateBase session, params String[] variableNames)
        {
            if (null == session || null == variableNames)
                return;
            String var = "\\'";
            foreach (var variableName in variableNames)
                session[variableName] = null;
        }

        /// <summary>
        /// Sets the value in session, clearing any previous values
        /// </summary>
        public static void Set<T>(this HttpSessionStateBase session, String valueName , T value)
        {
            if(null == session)
                return;

            session.Clear(valueName);
            session[valueName] = value;
        }

        public static T Get<T>(this HttpSessionStateBase session, String valueName) where T : class 
        {
            if (null == session || String.IsNullOrWhiteSpace(valueName))
                return null;

            return session[valueName] as T;
        }

        public static T GetOrSetDefault<T>(this HttpSessionStateBase session, String valueName, T defaultValue) where T : class
        {
            if (null == session || String.IsNullOrWhiteSpace(valueName))
                return null;

            return session[valueName] as T ?? (T)(session[valueName] = defaultValue);
        }
    }
}