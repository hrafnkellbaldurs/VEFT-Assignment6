using System.Net;
using System.Web.Http;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Services;

namespace CoursesAPI.Controllers
{
    [Authorize]
    [RoutePrefix("api/courses")]
	public class CoursesController : ApiController
	{
		private readonly CoursesServiceProvider _service;

		public CoursesController()
		{
			_service = new CoursesServiceProvider(new UnitOfWork<AppDataContext>());
		}

        /// <summary>
        /// This method represents a get request for all courses.
        /// It is accessible to everyone.
        /// </summary>
        /// <returns>StatusCode 200</returns>
		[HttpGet]
		[AllowAnonymous]
        [Route("")]
        public IHttpActionResult GetCourses()
		{
			return Ok();
		}

        /// <summary>
        /// This method represents a get request for a single course.
        /// It is only accessible as an authenticated user.
        /// </summary>
        /// <returns>StatusCode 401 if unauthorized, StatusCode 200 otherwise</returns>
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetCoursesByID()
        {
            return Ok();
        }

        /// <summary>
        /// This method represents a post request for a single course. 
        /// It is only accessible as an authenticated user.
        /// </summary>
        /// <returns>StatusCode 401 if unauthorized, StatusCode 201 otherwise</returns>
        [HttpPost]
		[Route("")]
		public IHttpActionResult AddCourse()
		{
			return StatusCode(HttpStatusCode.Created);
		}
	}
}
