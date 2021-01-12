using System;
using System.Collections.Generic;

namespace AncestryWeb.Models
{
    public class Field
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public string Type { get; set; }

        public List<string> Options { get; set; }
    }
}