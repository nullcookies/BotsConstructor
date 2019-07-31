namespace Website.Models
{
	public class PageViewModel
    {
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }

        public PageViewModel(int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (count - 1) / pageSize + 1;
        }

		public bool HasPreviousPage => PageNumber > 1;

		public bool HasNextPage => PageNumber < TotalPages;
	}
}
