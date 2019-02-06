using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Controllers
{
    [Route("api/authors/{authorID}/books")]
    public class BooksControllers : Controller
    {
        private ILibraryRepository _libRepository;
        private ILogger<BooksControllers> _logger;
        private IUrlHelper _urlHelper;

        //o logger pode ser criado com o logger factory ou injetado num construtor
        public BooksControllers(ILibraryRepository libRepository, ILogger<BooksControllers> logger, IUrlHelper urlHelper)
        {
            _libRepository = libRepository;
            _logger = logger;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {

            if (!_libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var booksForAuthorFromRepo = _libRepository.GetBooksForAuthor(authorId);

            //if (books == null)
            //{
            //    return NotFound();
            //}

            var booksForAuthor = Mapper.Map<IEnumerable<BookDto>>(booksForAuthorFromRepo);

            //foreach (var book in booksForAuthor)
            //{
            //    CreateLinksForBook(book);
            //}

            booksForAuthor = booksForAuthor.Select(book =>
            {
                book = CreateLinksForBook(book);
                return book;
            });

            var wrapper = new LinkedCollectionResourceWrapperDto<BookDto>(booksForAuthor);

            return Ok(CreateLinksForBooks(wrapper));
            
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {

            if (!_libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForAuthor = Mapper.Map<BookDto>(bookForAuthorFromRepo);

            return Ok(CreateLinksForBook(bookForAuthor));

        }

        [HttpPost()]

        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto),"The provided description should be diferent from the title.");
            }

            if (!ModelState.IsValid)
            {
                // unprocessable 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!_libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);

            _libRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBookForAuthor", new { authorId, id= bookToReturn.Id }, CreateLinksForBook(bookToReturn));
        }

        [HttpDelete ("{id}", Name = "DeleteBookForAuthor")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _libRepository.DeleteBook(bookForAuthorFromRepo);

            if (!_libRepository.Save())
            {
                throw new Exception($"Removing a book for author {authorId} failed.");
            }

            _logger.LogInformation(100, $"Book for author {authorId} removed.");
            return NoContent();

        }

        [HttpPut("{id}", Name = "UpdateBookForAuthor")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody] BookForUpdateDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (book.Description == book.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be diferent from the title.");
            }

            if (!ModelState.IsValid)
            {
                // unprocessable 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (! _libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;

                _libRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libRepository.Save())
                {
                    throw new Exception($"Upserting book{id} for author {authorId} failed.");
                }

                //return NotFound();
                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
            }

            Mapper.Map(book, bookForAuthorFromRepo);

            _libRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libRepository.Save())
            {
                throw new Exception($"Updating a book for author {authorId} failed.");
            }

            return NoContent();


        }


        [HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            
            if (!_libRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto,ModelState);

                if (bookDto.Description == bookDto.Title)
                {
                    ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be diferent from the title.");
                }

                TryValidateModel(bookDto);

                if (!ModelState.IsValid)
                {
                    // unprocessable 422
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;
                _libRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!_libRepository.Save())
                {
                    throw new Exception($"Upserting a book for author {authorId} failed.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);


            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState);

            if (bookToPatch.Description == bookToPatch.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be diferent from the title.");
            }

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                // unprocessable 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libRepository.Save())
            {
                throw new Exception($"Updating a book for author {authorId} failed.");
            }

            return NoContent();

        }

        private BookDto CreateLinksForBook(BookDto book)
        {
            //São as operações que se pode fazer sobre cada livro de forma individual
            //deve ser realizado sempre que um BookDto é devolvido
            book.Links.Add(new LinkDto(_urlHelper.Link("GetBookForAuthor", new { id = book.Id }), "self", "GET"));
            book.Links.Add(
                new LinkDto(_urlHelper.Link("DeleteBookForAuthor",
                new { id = book.Id }),
                "delete_book",
                "DELETE"));
            book.Links.Add(new LinkDto(_urlHelper.Link("UpdateBookForAuthor", new { id = book.Id }), "update_book", "PUT"));
            book.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateBookForAuthor", new { id = book.Id }), "partially_update_book", "PATCH"));
            return book;
        }

        private LinkedCollectionResourceWrapperDto<BookDto> CreateLinksForBooks(
            LinkedCollectionResourceWrapperDto<BookDto> booksWrapper)
        {

            booksWrapper.Links.Add(new LinkDto(_urlHelper.Link("GetBooksForAuthor", new { }),
                "self",
                "Get"));

            return booksWrapper;
        }
    }
}
