﻿namespace CourseProject.Models.ViewsModels
{
    public class AdminPanel
    {
        public IEnumerable<UserWithRolesVM> Users { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
