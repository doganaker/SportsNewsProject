﻿using SportsNewsProject.Models.ORM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsNewsProject.Models.VM
{
    public class CategoryPageVM : MainBaseVM
    {
        public Category category { get; set; }
    }
}
