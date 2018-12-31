using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libRepository;

        public AuthorsController(ILibraryRepository libRepository)
        {
            _libRepository = libRepository;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authors = _libRepository.GetAuthors();

            return Ok(authors);

        }
    }
}
