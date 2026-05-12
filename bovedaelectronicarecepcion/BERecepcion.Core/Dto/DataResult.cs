using BERecepcion.Core.Admin.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BERecepcion.Core.Dto
{
    public class DataResult<T>
    {
        public T Data { get; set; }       
        public string Message { get; set; }
        public HttpStatusCode Status { get; set; }
        public Pager Pager { get; set; }
        public UsersDto User { get; set; }
    }

    public class Pager
    {
        public Pager()
        {

        }
        public Pager(int totalItems, int? page, int pageSize = 10)
        {
            // Total de paginación a mostrar
            int _totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            // Pagina actual
            int _currentPage = page != null ? (int)page : 1;
            // Paginación inicia con
            int _startPage = _currentPage - 5;
            // Paginación termina con
            int _endPage = _currentPage + 4;
            if (_startPage <= 0)
            {
                _endPage -= (_startPage - 1);
                _startPage = 1;
            }
            if (_endPage > _totalPages)
            {
                _endPage = _totalPages;
                if (_endPage > 10)
                {
                    _startPage = _endPage - 9;
                }
            }
            // Propiedades de paginación
            TotalItems = totalItems;
            CurrentPage = _currentPage;
            PageSize = pageSize;
            TotalPages = _totalPages;
            StartPage = _startPage;
            EndPage = _endPage;
        }

        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }
}
