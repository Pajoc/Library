using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Library.Helpers;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Http;
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
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;

        public AuthorsController(ILibraryRepository libRepository,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService)
        {
            _libRepository = libRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {

            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto,Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            var authorsFromRepo = _libRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

            //Metadata
            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));


            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);

            return Ok(authors);

        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchSearchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchSearchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetAuthors",
                        new
                        {
                            orderBy = authorsResourceParameters.OrderBy,
                            searchSearchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }

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
        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {

            var authorFromRepo = _libRepository.GetAuthor(id);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _libRepository.DeleteAuthor(authorFromRepo);

            if (!_libRepository.Save())
            {
                throw new Exception($"Removing author {id} failed.");
            }

            return NoContent();

        }
    }
}
