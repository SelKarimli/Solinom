namespace FinalProject.MVC.ViewModels;

    public class PaginationItemsVM
    {
        public int ActivePage { get; set; }
        public int Take { get; set; }
        public decimal PageCount { get; set; }
        public PaginationItemsVM(decimal productCount,int take, int activePage)
        {
            PageCount = Math.Ceiling(productCount / take);
            Take = take;
            ActivePage = activePage;
        }
    }
