//using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models;
using BERRecepcion.Front.Models.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BERRecepcion.Front.Interfaces
{
    public interface IRestUtility
    {
        Task<T> GetItem<T>(string apiEndPoint, IEnumerable<CustomHttpParameter> parameters = null);
        Task<IEnumerable<T>> GetList<T>(string apiEndPoint, IEnumerable<CustomHttpParameter> parameters = null);
        Task<T> Post<T>(T dto, string apiEndPoint);
        Task<T> Update<T>(T dto, Guid Id, string apiEndPoint);
        Task<IEnumerable<T>> UpdateReturnList<T>(T dto, Guid Id, string apiEndPoint);
        Task<T> Delete<T>(Guid Id, string apiEndPoint);
        Task<T> Delete<T>(T dto, string apiEndPoint);


    }
}
