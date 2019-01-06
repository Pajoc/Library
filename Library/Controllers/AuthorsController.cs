using AutoMapper;
using Library.API.Models;
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
            var authorsFromRepo = _libRepository.GetAuthors();

            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);

        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
           // throw new Exception("Random exception");

            var authorFromRepo = _libRepository.GetAuthor(id);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var authors = Mapper.Map<AuthorDto>(authorFromRepo);

            return Ok(authors);

        }
    }
}
