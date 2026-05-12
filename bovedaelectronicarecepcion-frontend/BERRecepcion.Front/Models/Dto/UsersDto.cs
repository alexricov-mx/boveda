using BERRecepcion.Front.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BERRecepcion.Front.Models.Dto
{
    public class UsersDto : UsersDetailDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string UserType { get; set; }
        public string Token { get; set; }
        public string ManagementCenter { get; set; }
        public string CreditorNumber { get; set; }
        public string RFC { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public Guid ProfileID { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DateInitialValid { get; set; }
        public DateTime DateEndValid { get; set; }
        public string CreditorRFC { get; set; }

        //Virtual properties
        public virtual ProfilesDto Profile { get; set; }
        public virtual string ProfileName { get; set; }
        public virtual IEnumerable<OrganismDto> Organisms { get; set; }
        public virtual DateTime? UltimoAcceso { get; set; }

        public virtual string UserTypeDescription
        {
            get
            {
                return UserType == Enum.GetName(UserTypeEnum.UserTypeF) ? "Funcionario" :
                       UserType == Enum.GetName(UserTypeEnum.UserTypeP) ? "Proveedor" :
                       UserType == Enum.GetName(UserTypeEnum.UserTypeA) ? "Administrador" :
                       UserType == Enum.GetName(UserTypeEnum.UserTypeS) ? "Super Administrador" :
                       UserType == Enum.GetName(UserTypeEnum.Auditor) ? "Auditor" : "Usuario PPI";
            }
        }

    }
}
