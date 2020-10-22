namespace ImageGallery.Client.ViewModels
{
    public class OrderFrameViewModel
    {
	    public string Address { get; private set; }

	    public OrderFrameViewModel(string address)
	    {
		    Address = address;
	    }
    }
}
