using System;

namespace BERRecepcion.Front.Models
{
    public class CustomHttpParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public CustomHttpParameter(string name, string value)
        {
            Name = name;
            Value = value;  
        }

        public CustomHttpParameter(string name, int value)
        {
            Name = name;
            Value = value.ToString();
        }

        public CustomHttpParameter(string name, Guid? value)
        {
            Name = name;
            Value = value.ToString();
        }

        public CustomHttpParameter(string name, bool value)
        {
            Name = name;
            Value = value.ToString();
        }

    }
}
