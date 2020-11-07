﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;

        public MoviesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            IQueryable<string> genreQuery = from m in _context.Movies
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movies
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(movieGenre))
            {
                movies = movies.Where(x => x.Genre.ToLower() == movieGenre.ToLower());
            }

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };

            return View(movieGenreVM);
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return $"From [HttpPost] Index: filter on {searchString}";
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        #region Create
        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }


        #endregion

        #region Edit
        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
          
            ViewData["Directors"] = CreateDirectorDropdown(); ;


            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(new MovieEditViewModel { 
                DirectorID = movie.DirectorID,
                Genre = movie.Genre,
                ID =movie.ID,
                Price = movie.Price,
                Rating = movie.Rating,
                ReleaseDate = movie.ReleaseDate,
                Title = movie.Title,
            });
        }

    

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,ReleaseDate,Genre,Price,Rating,DirectorID,NewDirector")] MovieEditViewModel model)
        {

           
            if (id != model.ID)
            {
                return NotFound();
            }

            ViewData["Directors"] = CreateDirectorDropdown(); ;


            if (ModelState.IsValid)
            {
                try
                {
                    var movie = await _context.Movies.FindAsync(model.ID);
                    if (movie == null) return NotFound();


                    if((model.DirectorID == null && model.DirectorID == 0) && model.NewDirector == string.Empty)
                    {
                        ModelState.AddModelError("ID_Or_New","ID or NewDirector cannot be both null");

                        return View(model);
                    }

                    if(model.DirectorID == null || model.DirectorID == 0)
                    {
                        if (string.IsNullOrEmpty(model.NewDirector))
                        {
                            return View(model);
                        }
                        else
                        {
                            var dir = new Director
                            {
                                Name = model.NewDirector
                            };


                            _context.Directors.Add(dir);
                            await _context.SaveChangesAsync();

                            movie.DirectorID = dir.ID;

                        }
                    }
                    else
                    {
                        movie.DirectorID = model.DirectorID;
                    }

                    movie.Title = model.Title;
                    movie.Genre = model.Genre;
                    movie.Price = model.Price;
                    movie.Rating = model.Rating;
                    movie.ReleaseDate = model.ReleaseDate;


                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(model.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }


            return View(model);
        }


        #endregion


        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.ID == id);
        }


        private SelectList CreateDirectorDropdown()
        {
            var directors = _context.Directors.AsNoTracking().ToArray();

            var selectList = new SelectList(
                directors.Select(i => new SelectListItem { Text = i.Name, Value = i.ID.ToString() }),
                 "Value",
                "Text");
            return selectList;
        }
    }
}