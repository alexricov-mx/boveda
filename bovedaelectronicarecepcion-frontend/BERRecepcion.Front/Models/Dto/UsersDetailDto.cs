using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class UsersDetailDto
    {
        //NotMapped Attributes
        public virtual bool UserIsValid { get; set; }
        public virtual bool UserExists { get; set; }
        public virtual bool UserIsDeleted { get; set; }
        public virtual bool UserIsBlocked { get; set; }
        public virtual bool UserdateIsValid { get; set; }
        public virtual bool IsSapInterfaceEnabled { get; set; }
    }
}
