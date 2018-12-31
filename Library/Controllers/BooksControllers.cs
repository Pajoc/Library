using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/author")]
    public class BooksControllers : Controller
    {
        private ILibraryRepository _libRepository;

        public BooksControllers(ILibraryRepository libRepository)
        {
            _libRepository = libRepository;
        }

        [HttpGet("{author}/books")]
        public IActionResult GetBooks(Guid author)
        {
            var books = _libRepository.GetBooksForAuthor(author);

            return Ok(books);

        }
    }
}
