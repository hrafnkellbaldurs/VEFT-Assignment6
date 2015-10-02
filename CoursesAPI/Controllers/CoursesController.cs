using System.Net;
using System.Web.Http;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Services;
using WebApi.OutputCache.V2;

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
        /// <returns>StatusCode 200 with the list of courses</returns>
		[HttpGet]
		[AllowAnonymous]
        [CacheOutput(ClientTimeSpan = 86400, ServerTimeSpan = 86400)]
        [Route("")]
        public IHttpActionResult GetCourseInstancesBySemester(string semester = null)
		{
			return Ok(_service.GetCourseInstancesBySemester(semester));
		}

        /// <summary>
        /// This method represents a get request for a single course.
        /// It is only accessible as an authenticated user.
        /// </summary>
        /// <param name="id">The id of the course to get</param>
        /// <returns>StatusCode 401 if unauthorized, StatusCode 200 otherwise with the course</returns>
        [HttpGet]
        [CacheOutput(ClientTimeSpan = 86400, ServerTimeSpan = 86400)]
        [Route("{id}")]
        public IHttpActionResult GetCourseInstancesByID(int id)
        {
            try
            {
                return Ok(_service.GetCourseInstancesById(id));
            }
            catch (AppObjectNotFoundException e)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
            
        }

        /// <summary>
        /// This method represents a post request for a single course. 
        /// It is only accessible as an authenticated user.
        /// </summary>
        /// <returns>StatusCode 401 if unauthorized, StatusCode 201 otherwise</returns>
        [HttpPost]
        [InvalidateCacheOutput("GetCourseInstancesBySemester")]
        [Route("")]
		public IHttpActionResult AddCourse()
		{
			return StatusCode(HttpStatusCode.Created);
		}
	}
}
