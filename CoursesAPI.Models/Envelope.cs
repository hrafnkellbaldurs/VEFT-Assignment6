using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesAPI.Models
{
    /// <summary>
    /// This class represents a container for a generic type collection and paging info about it
    /// </summary>
    /// <typeparam name="T">The type of collection of items to keep track of</typeparam>
    public class Envelope<T>
    {
        /// <summary>
        /// The collection of items in the envelope
        /// </summary>
        public T Items { get; set; }

        public class PagingInfo
        {
            /// <summary>
            /// The number of pages in the envelope
            /// </summary>
            public int PageCount            { get; set; }

            /// <summary>
            /// The number of items in each page
            /// </summary>
            public int PageSize             { get; set; }

            /// <summary>
            /// A 1-based index of the current page being returned
            /// </summary>
            public int PageNumber           { get; set; }

            /// <summary>
            /// The total number of items in the collection
            /// </summary>
            public int TotalNumberOfItems   { get; set; }
        }

        /// <summary>
        /// The paging object containing the paginfo object
        /// </summary>
        public PagingInfo Paging { get; set; }
    }
}
