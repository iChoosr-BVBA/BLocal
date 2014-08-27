using System;

namespace BLocal.Web.Manager.Business
{
    public class ImportExportRecord
    {
        public String Part { get; set; }
        public String Key { get; set; }
        public String Value { get; set; }
        public bool DeleteOnImport { get; set; }

        public ImportExportRecord()
        {
        }

        public ImportExportRecord(string part, string key, string value)
        {
            Part = part;
            Key = key;
            Value = value;
            DeleteOnImport = false;
        }
    }
}