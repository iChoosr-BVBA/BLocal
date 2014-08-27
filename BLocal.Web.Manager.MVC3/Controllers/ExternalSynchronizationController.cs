using System;
using System.Web.Mvc;

namespace BLocal.Web.Manager.Controllers
{
    public class ExternalSynchronizationController
    {
        public JsonResult ExportTo(String targetName)
        {
            return null;
        }

        public JsonResult ReceiveExport(String senderName)
        {
            return null;
        }

        public JsonResult ImportFrom(String targetName)
        {
            return null;
        }

        public JsonResult ProvideImport(String senderName)
        {
            return null;
        }

        public JsonResult AnalyseSynchronization(String senderName)
        {
            return null;
        }
    }
}