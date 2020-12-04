﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsNewsProject.Models.ORM.Context;
using SportsNewsProject.Models.ORM.Entities;
using SportsNewsProject.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsNewsProject.Controllers
{
    public class AuthorController : Controller
    {
        private readonly SportsNewsContext _newscontext;

        public AuthorController(SportsNewsContext context)
        {
            _newscontext = context;
        }
        public IActionResult Index()
        {
            List<AuthorVM> authors = _newscontext.Authors.Where(q => q.IsDeleted == false).Select(q => new AuthorVM()
            {
                ID = q.ID,
                Name = q.Name,
                Surname = q.SurName,
                EMail = q.EMail,
                Phone = q.Phone,
                AddDate = q.AddDate,
            }).ToList();

            return View(authors);
        }

        public IActionResult Add()
        {
            //ViewBag.Categories = _newscontext.Categories.ToList();
            AuthorVM model = new AuthorVM();
            model.Categories = _newscontext.Categories.ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(AuthorVM model, int[] categoryid)
        {
            if (ModelState.IsValid)
            {
                Author author = new Author();
                author.ID = model.ID;
                author.Name = model.Name;
                author.SurName = model.Surname;
                author.EMail = model.EMail;
                author.Phone = model.Phone;
                author.AddDate = model.AddDate;

                _newscontext.Authors.Add(author);
                _newscontext.SaveChanges();

                int authorid = author.ID;

                //ViewBag.Categories = _newscontext.Categories.ToList();
                model.Categories = _newscontext.Categories.ToList();

                for (int i = 0; i < categoryid.Length; i++)
                {
                    AuthorCategory authorcategory = new AuthorCategory();
                    authorcategory.CategoryID = categoryid[i];
                    authorcategory.AuthorID = authorid;
                    _newscontext.AuthorCategories.Add(authorcategory);

                }
                _newscontext.SaveChanges();


            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            AuthorVM model = _newscontext.Authors.Select(q => new AuthorVM()
            {
                ID = q.ID,
                Name = q.Name,
                Surname = q.SurName,
                EMail = q.EMail,
                Phone = q.Phone,
                AddDate = q.AddDate,
                Categories = _newscontext.Categories.ToList(),
                AuthorCategories = _newscontext.AuthorCategories.Where(x => x.AuthorID == id).Select(q => q.CategoryID).ToList()

        }).FirstOrDefault(x => x.ID == id);


            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(AuthorVM model, int[] categoryid)
        {
            Author author = _newscontext.Authors.FirstOrDefault(x => x.ID == model.ID);

            if (ModelState.IsValid)
            {
                author.Name = model.Name;
                author.SurName = model.Surname;
                author.EMail = model.EMail;
                author.Phone = model.Phone;
                author.AddDate = model.AddDate;

                _newscontext.SaveChanges();

                int autherid = author.ID;
                model.Categories = _newscontext.Categories.ToList();
                model.AuthorCategories = _newscontext.AuthorCategories.Where(x => x.AuthorID == autherid).Select(q => q.CategoryID).ToList();

                for (int i = 0; i < categoryid.Length; i++)
                {
                    AuthorCategory authorCategory = new AuthorCategory();

                    authorCategory.CategoryID = categoryid[i];
                    authorCategory.AuthorID = autherid;

                    _newscontext.AuthorCategories.Add(authorCategory);
                    _newscontext.SaveChanges();
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Author author = _newscontext.Authors.FirstOrDefault(x => x.ID == id);
            author.IsDeleted = true;

            _newscontext.SaveChanges();

            return Json("Author Deleted Successfully!");
        }


    }
}