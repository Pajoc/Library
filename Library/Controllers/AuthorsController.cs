using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Library.Models;
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

        [HttpGet("{id}", Name = "GetAuthor")]
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

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);
            _libRepository.AddAuthor(authorEntity);
            if (!_libRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
                //Já estou a tratar globalmente
                //return StatusCode(500, "A problem happened while handling your request. Please try again later.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { authorToReturn.Id }, authorToReturn);
        }
    }
}
