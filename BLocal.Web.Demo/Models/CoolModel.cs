using System;

namespace BLocal.Web.Demo.Models
{
    public class CoolModel
    {
        public String Name { get; set; }
        public int Id { get; set; }

        public CoolModel(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }
}