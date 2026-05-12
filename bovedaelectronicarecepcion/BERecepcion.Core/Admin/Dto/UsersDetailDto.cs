using BERecepcion.Core.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERecepcion.Core.Admin.Dto
{
    public class UsersDetailDto : GeneralResponseDto
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
