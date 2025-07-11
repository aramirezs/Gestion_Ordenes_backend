﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class PaginationResult<T>
    {
        public bool HasItems
        {
            get
            {
                return Items != null && Items.Any();
            }
        }
        public int Total { get; set; }

        public int Page { get; set; }

        public int Pages { get; set; }
        public ICollection<T> Items { get; set; }


    }
}
