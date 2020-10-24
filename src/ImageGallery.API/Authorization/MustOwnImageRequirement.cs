using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.API.Authorization
{
	public class MustOwnImageRequirement : IAuthorizationRequirement
	{
		public MustOwnImageRequirement()
	    {
		    
	    }

		// You can add as many requirements as possible here 
		// Example country

		//private readonly string _country;
		//public MustOwnImageRequirement(string country)
		//{
		//	_country = country;
		//}
	}
}
